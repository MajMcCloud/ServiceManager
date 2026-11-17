using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ServiceManager.Base.Data;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Starts one process under another account through a token of its own. The framework path of
    /// <see cref="ProcessStartInfo.UserName"/> goes by logon and is refused for a caller running as LocalSystem,
    /// because that token carries no logon identifier. Going by token instead is what Microsoft names as the
    /// replacement: the account is logged on, its token is made primary, and the process is created from it.
    ///
    /// The process is created standing still and only runs once <see cref="Resume"/> is called, so the caller can
    /// put its watchers on it first. Its three standard streams cannot come from the framework either, they are
    /// handed over as pipes the manager keeps the other end of.
    /// </summary>
    public sealed class WindowsUserProcess : IDisposable
    {
        /// <summary>The logon that asks for the interactive right of the account and loads its profile.</summary>
        public const int Logon_Type_Interactive = 2;

        public const int Logon_Provider_Default = 0;

        public const int Creation_Flag_Suspended = 0x0004;

        public const int Creation_Flag_Unicode_Environment = 0x0400;

        public const int Startup_Flag_Use_Std_Handles = 0x0100;

        private const int Security_Impersonation = 2;
        private const int Token_Primary = 1;
        private const int Maximum_Allowed = 0x02000000;
        private const int Handle_Flag_Inherit = 0x00000001;

        private readonly Process m_Process;

        private readonly IntPtr m_Thread;

        private readonly SafeFileHandle m_InputWrite;
        private readonly SafeFileHandle m_OutputRead;
        private readonly SafeFileHandle m_ErrorRead;

        private readonly StreamWriter m_Input;

        private readonly object m_Sync = new object();

        private bool m_Resumed;
        private bool m_Disposed;

        /// <summary>
        /// The line the process writes to its standard output, set before <see cref="Resume"/> is called.
        /// </summary>
        public Action<String> OutputLine { get; set; }

        /// <summary>
        /// The line the process writes to its standard error stream, set before <see cref="Resume"/> is called.
        /// </summary>
        public Action<String> ErrorLine { get; set; }

        /// <summary>The process the manager watches, an ordinary one so the rest of the manager stays as it is.</summary>
        public Process Process
        {
            get { return m_Process; }
        }

        private WindowsUserProcess(Process process, IntPtr thread, SafeFileHandle inputWrite, SafeFileHandle outputRead, SafeFileHandle errorRead)
        {
            m_Process = process;
            m_Thread = thread;
            m_InputWrite = inputWrite;
            m_OutputRead = outputRead;
            m_ErrorRead = errorRead;

            //The child reads its standard input as text, so the writer must not prepend a byte order mark
            m_Input = new StreamWriter(new FileStream(inputWrite, FileAccess.Write), new UTF8Encoding(false));
            m_Input.AutoFlush = true;
        }

        /// <summary>
        /// True when the account has to be started through a token. Only LocalSystem is refused the logon of the
        /// framework, and every other host keeps the shorter path with its streams of its own.
        /// </summary>
        public static bool WantsTokenPath(RunAsAccount account, bool hostIsLocalSystem)
        {
            return hostIsLocalSystem && account != null && account.UsesLogon;
        }

        /// <summary>
        /// The command line of the process. The path is quoted, a program in "Program Files" would otherwise be
        /// looked for under "Program".
        /// </summary>
        public static String CommandLine(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return String.Empty;
            }

            return "\"" + path + "\"";
        }

        /// <summary>
        /// Logs the account on, builds its token and creates the process from it. The process does not run yet, it
        /// waits for <see cref="Resume"/>.
        /// </summary>
        public static WindowsUserProcess Start(RunAsAccount account, String path)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (!account.UsesLogon)
            {
                throw new ArgumentException("The account is inherited, no logon was asked for", "account");
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The program of the service is not named", "path");
            }

            IntPtr token = IntPtr.Zero;
            IntPtr primary = IntPtr.Zero;
            IntPtr environment = IntPtr.Zero;

            SafeFileHandle childInput = null;
            SafeFileHandle childOutput = null;
            SafeFileHandle childError = null;

            SafeFileHandle parentInput = null;
            SafeFileHandle parentOutput = null;
            SafeFileHandle parentError = null;

            WindowsUserProcess session = null;

            try
            {
                if (!LogonUser(account.UserName, account.Domain, account.Password, Logon_Type_Interactive, Logon_Provider_Default, out token))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "The logon of " + account.FullName + " was refused");
                }

                if (!DuplicateTokenEx(token, Maximum_Allowed, IntPtr.Zero, Security_Impersonation, Token_Primary, out primary))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "The token of " + account.FullName + " could not be used");
                }

                //Without the block the child would inherit the environment of the manager, including its paths
                if (!CreateEnvironmentBlock(out environment, primary, false))
                {
                    environment = IntPtr.Zero;
                }

                //The child reads its input and writes its output, so it gets the end of the pipe that fits
                childInput = readEnd(out parentInput);
                childOutput = writeEnd(out parentOutput);
                childError = writeEnd(out parentError);

                STARTUPINFO info = new STARTUPINFO();
                info.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                info.dwFlags = Startup_Flag_Use_Std_Handles;
                info.hStdInput = childInput.DangerousGetHandle();
                info.hStdOutput = childOutput.DangerousGetHandle();
                info.hStdError = childError.DangerousGetHandle();

                int flags = Creation_Flag_Suspended;

                if (environment != IntPtr.Zero)
                {
                    flags |= Creation_Flag_Unicode_Environment;
                }

                PROCESS_INFORMATION created;

                //The handles marked inheritable are handed to the child, which is what makes the three streams
                //work. Nothing of the manager is marked inheritable today.
                if (!CreateProcessAsUser(primary, path, CommandLine(path), IntPtr.Zero, IntPtr.Zero, true, flags, environment, null, ref info, out created))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "The process could not be started under " + account.FullName);
                }

                Process process;

                try
                {
                    process = Process.GetProcessById(created.dwProcessId);
                }
                catch (Exception)
                {
                    //The process is still standing still, so it can be taken away instead of left running unwatched
                    TerminateProcess(created.hProcess, 1);

                    CloseHandle(created.hThread);
                    CloseHandle(created.hProcess);

                    throw;
                }

                CloseHandle(created.hProcess);

                session = new WindowsUserProcess(process, created.hThread, parentInput, parentOutput, parentError);

                return session;
            }
            finally
            {
                //The child owns the ends it was given. The manager has to let go of its copies, a write end it
                //kept open itself would keep its own reader from ever seeing the end of the output
                if (childInput != null)
                {
                    childInput.Dispose();
                }

                if (childOutput != null)
                {
                    childOutput.Dispose();
                }

                if (childError != null)
                {
                    childError.Dispose();
                }

                //A start that never made it to a session would leave the other ends of the pipes behind
                if (session == null)
                {
                    if (parentInput != null)
                    {
                        parentInput.Dispose();
                    }

                    if (parentOutput != null)
                    {
                        parentOutput.Dispose();
                    }

                    if (parentError != null)
                    {
                        parentError.Dispose();
                    }
                }

                if (environment != IntPtr.Zero)
                {
                    DestroyEnvironmentBlock(environment);
                }

                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }

                if (primary != IntPtr.Zero)
                {
                    CloseHandle(primary);
                }
            }
        }

        /// <summary>
        /// Lets the process run and starts the two readers of its output. The watchers of the caller are on the
        /// process by then, which is why it was created standing still.
        /// </summary>
        public void Resume()
        {
            lock (m_Sync)
            {
                if (m_Resumed || m_Disposed)
                {
                    return;
                }

                m_Resumed = true;
            }

            readLines(m_OutputRead, OutputLine);
            readLines(m_ErrorRead, ErrorLine);

            ResumeThread(m_Thread);
        }

        /// <summary>
        /// Writes one line to the standard input of the process. A process that already ended costs nothing more
        /// than the line, the manager keeps talking to services that are on their way out.
        /// </summary>
        public void WriteInput(String line)
        {
            try
            {
                m_Input.WriteLine(line);
                m_Input.Flush();
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void Dispose()
        {
            lock (m_Sync)
            {
                if (m_Disposed)
                {
                    return;
                }

                m_Disposed = true;
            }

            m_Input.Dispose();
            m_Process.Dispose();

            if (m_Thread != IntPtr.Zero)
            {
                CloseHandle(m_Thread);
            }
        }

        /// <summary>
        /// One pipe with the read end for the child. The end the manager keeps is taken off the list of
        /// inheritable handles, otherwise the child would hold a copy of it as well.
        /// </summary>
        private static SafeFileHandle readEnd(out SafeFileHandle parentWrite)
        {
            SafeFileHandle read;
            SafeFileHandle write;

            pair(out read, out write);

            notInheritable(write);

            parentWrite = write;

            return read;
        }

        /// <summary>
        /// One pipe with the write end for the child.
        /// </summary>
        private static SafeFileHandle writeEnd(out SafeFileHandle parentRead)
        {
            SafeFileHandle read;
            SafeFileHandle write;

            pair(out read, out write);

            notInheritable(read);

            parentRead = read;

            return write;
        }

        private static void pair(out SafeFileHandle read, out SafeFileHandle write)
        {
            SECURITY_ATTRIBUTES attributes = new SECURITY_ATTRIBUTES();
            attributes.nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
            attributes.bInheritHandle = true;

            IntPtr readHandle;
            IntPtr writeHandle;

            if (!CreatePipe(out readHandle, out writeHandle, ref attributes, 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "The pipe of the process could not be made");
            }

            read = new SafeFileHandle(readHandle, true);
            write = new SafeFileHandle(writeHandle, true);
        }

        private static void notInheritable(SafeFileHandle handle)
        {
            if (!SetHandleInformation(handle, Handle_Flag_Inherit, 0))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "The pipe of the process could not be kept back");
            }
        }

        private void readLines(SafeFileHandle handle, Action<String> onLine)
        {
            if (onLine == null)
            {
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    using (StreamReader reader = new StreamReader(new FileStream(handle, FileAccess.Read), new UTF8Encoding(false)))
                    {
                        String line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            onLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ServiceManager: the output of the process was lost: " + ex.Message);
                }
            });
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String userName, String domain, String password, int logonType, int logonProvider, out IntPtr token);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(IntPtr existingToken, int desiredAccess, IntPtr tokenAttributes, int impersonationLevel, int tokenType, out IntPtr newToken);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(out IntPtr environment, IntPtr token, bool inherit);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool DestroyEnvironmentBlock(IntPtr environment);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(IntPtr token, String applicationName, String commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, int creationFlags, IntPtr environment, String currentDirectory, ref STARTUPINFO startupInfo, out PROCESS_INFORMATION information);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr process, int exitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int ResumeThread(IntPtr thread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreatePipe(out IntPtr readHandle, out IntPtr writeHandle, ref SECURITY_ATTRIBUTES attributes, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetHandleInformation(SafeFileHandle handle, int flags, int mask);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
    }
}
