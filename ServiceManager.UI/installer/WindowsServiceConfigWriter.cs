using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;

namespace ServiceManager.UI.Installer
{
    /// <summary>
    /// The answer of a write to the service control manager.
    /// </summary>
    public class WindowsServiceWriteResult
    {
        public bool Success { get; set; }

        /// <summary>What to show the user. Empty when everything worked out.</summary>
        public String Message { get; set; }

        /// <summary>True when the service has to be restarted for the new values to take effect.</summary>
        public bool RequiresRestart { get; set; }
    }

    /// <summary>
    /// Writes the settings of an installed ServiceManager service back to the service control manager.
    /// The installer only runs once, so an edit in the settings dialog has to be applied here instead.
    /// </summary>
    public class WindowsServiceConfigWriter
    {
        private const UInt32 SC_MANAGER_CONNECT = 0x00000001;
        private const UInt32 SERVICE_QUERY_CONFIG = 0x00000001;
        private const UInt32 SERVICE_CHANGE_CONFIG = 0x00000002;

        private const UInt32 SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const UInt32 SERVICE_ERROR_NORMAL = 0x00000001;
        private const UInt32 SERVICE_AUTO_START = 0x00000002;
        private const UInt32 SERVICE_DEMAND_START = 0x00000003;
        private const UInt32 SERVICE_DISABLED = 0x00000004;

        private const UInt32 SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 0x00000003;

        private const Int32 ERROR_SERVICE_DOES_NOT_EXIST = 0x00000424;
        private const Int32 ERROR_ACCESS_DENIED = 0x00000005;
        private const Int32 ERROR_PRIVILEGE_NOT_HELD = 0x00000522;

        /// <summary>The sentence shown whenever Windows refuses because the rights are missing.</summary>
        public const String MissingRightsHint = "Administrator rights are required. Start the application with \"Run as administrator\" and try again.";

        private const Int32 POLICY_LOOKUP_NAMES = 0x0800;
        private const Int32 POLICY_CREATE_ACCOUNT = 0x0010;

        private const String SE_SERVICE_LOGON_RIGHT = "SeServiceLogonRight";

        /// <summary>True when the service is registered under this name.</summary>
        public bool IsInstalled(String serviceName)
        {
            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_CONNECT);

            if (scm == IntPtr.Zero)
            {
                Int32 error = Marshal.GetLastWin32Error();

                throw new Win32Exception(error, WithMissingRights("The service control manager could not be opened.", error));
            }

            try
            {
                IntPtr service = OpenService(scm, serviceName, SERVICE_QUERY_CONFIG);

                if (service == IntPtr.Zero)
                {
                    Int32 error = Marshal.GetLastWin32Error();
                    if (error == ERROR_SERVICE_DOES_NOT_EXIST)
                    {
                        return false;
                    }

                    throw new Win32Exception(error, WithMissingRights("The service configuration could not be read.", error));
                }

                CloseServiceHandle(service);

                return true;
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        /// <summary>
        /// Applies the startup type, the dependencies and the log on account to an installed service.
        /// The password is read from its protected form in the settings file. A named account also
        /// gets the right to log on as a service, which the service control manager does not grant on
        /// its own even though services.msc does.
        /// </summary>
        public WindowsServiceWriteResult Apply(String serviceName, WindowsService settings, bool accountChanged = true)
        {
            if (String.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            String account = settings.GetAccountName();

            if (account == null)
            {
                return Fail("The service account needs a user name.");
            }

            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_CONNECT);

            if (scm == IntPtr.Zero)
            {
                return Fail(WithMissingRights("The service control manager could not be opened.", Marshal.GetLastWin32Error()));
            }

            try
            {
                IntPtr service = OpenService(scm, serviceName, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

                if (service == IntPtr.Zero)
                {
                    Int32 error = Marshal.GetLastWin32Error();

                    if (error == ERROR_SERVICE_DOES_NOT_EXIST)
                    {
                        return Fail("The service \"" + serviceName + "\" is not installed.");
                    }

                    return Fail(WithMissingRights("The service \"" + serviceName + "\" could not be opened.", error));
                }

                IntPtr dependencies = IntPtr.Zero;
                try
                {
                    UInt32 startType = ToStartType(settings.StartMode);
                    dependencies = Marshal.StringToHGlobalUni(BuildDependencyList(settings.GetDependencies()));
                    String password = accountChanged && settings.UsesPassword() ? SecretStorage.Unprotect(settings.Password) : null;

                    if (accountChanged && settings.UsesPassword())
                    {
                        String rightMessage = GrantLogOnAsServiceRight(account);
                        if (rightMessage != null)
                        {
                            return Fail(rightMessage);
                        }
                    }

                    if (!ChangeServiceConfig(service, SERVICE_NO_CHANGE, startType, SERVICE_NO_CHANGE,
                        null, null, IntPtr.Zero, dependencies, accountChanged ? account : null, password, null))
                    {
                        return Fail(WithMissingRights("The service configuration could not be written.", Marshal.GetLastWin32Error()));
                    }

                    if (startType == SERVICE_AUTO_START)
                    {
                        SERVICE_DELAYED_AUTO_START_INFO delayed = new SERVICE_DELAYED_AUTO_START_INFO();
                        delayed.fDelayedAutostart = settings.StartMode == WindowsService.eStartMode.AutomaticDelayed;

                        if (!ChangeServiceConfig2(service, SERVICE_CONFIG_DELAYED_AUTO_START_INFO, ref delayed))
                        {
                            return Fail(WithMissingRights("The service settings were partially applied, but delayed start could not be written. Check the Windows service configuration before retrying.",
                                Marshal.GetLastWin32Error()));
                        }
                    }

                    return new WindowsServiceWriteResult { Success = true, RequiresRestart = accountChanged };
                }
                finally
                {
                    Marshal.FreeHGlobal(dependencies);
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        /// <summary>True when the service is registered and running.</summary>
        public bool IsRunning(String serviceName)
        {
            using (ServiceController controller = new ServiceController(serviceName))
            {
                return controller.Status == ServiceControllerStatus.Running;
            }
        }

        /// <summary>
        /// Stops and starts the service again. A service that is not running is left alone, its new
        /// values are picked up the next time it starts.
        /// </summary>
        public WindowsServiceWriteResult Restart(String serviceName)
        {
            WindowsServiceWriteResult result = new WindowsServiceWriteResult();
            result.Success = true;

            try
            {
                using (ServiceController controller = new ServiceController(serviceName))
                {
                    controller.Refresh();

                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        return result;
                    }

                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));

                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException)
            {
                result.Success = false;

                if (IsMissingRights(ex))
                {
                    result.Message = "The service could not be restarted." + Environment.NewLine + Environment.NewLine + MissingRightsHint;
                }
                else
                {
                    result.Message = "The service could not be restarted: " + ex.Message;
                }
            }

            return result;
        }

        private static UInt32 ToStartType(WindowsService.eStartMode startMode)
        {
            switch (startMode)
            {
                case WindowsService.eStartMode.Automatic:
                case WindowsService.eStartMode.AutomaticDelayed:

                    return SERVICE_AUTO_START;

                case WindowsService.eStartMode.Manual:

                    return SERVICE_DEMAND_START;

                case WindowsService.eStartMode.Disabled:

                    return SERVICE_DISABLED;

                default:

                    throw new ArgumentOutOfRangeException(nameof(startMode));
            }
        }

        /// <summary>
        /// The dependency list in the shape the service control manager expects it: one name per
        /// entry, each closed by a null character, the whole list closed by a second one. An empty
        /// string means no dependencies, which is something else than a null pointer, that one means
        /// the list stays as it is.
        /// </summary>
        private static String BuildDependencyList(String[] dependencies)
        {
            if (dependencies == null || dependencies.Length == 0)
            {
                return "\0\0";
            }

            return String.Join("\0", dependencies) + "\0\0";
        }

        /// <summary>
        /// Adds the "Log on as a service" right to a named account. Without it the service starts and
        /// the service control manager reports a logon failure, and the API does not add the right by
        /// itself. Null when the right is in place, the text to show otherwise.
        /// </summary>
        public String GrantLogOnAsServiceRight(String accountName)
        {
            IntPtr sid = IntPtr.Zero;
            GCHandle handle = new GCHandle();
            IntPtr policy = IntPtr.Zero;

            try
            {
                SecurityIdentifier identifier;

                try
                {
                    identifier = (SecurityIdentifier)(new NTAccount(accountName)).Translate(typeof(SecurityIdentifier));
                }
                catch (IdentityNotMappedException ex)
                {
                    return "The account \"" + accountName + "\" could not be resolved: " + ex.Message;
                }

                byte[] binary = new byte[identifier.BinaryLength];
                identifier.GetBinaryForm(binary, 0);

                handle = GCHandle.Alloc(binary, GCHandleType.Pinned);
                sid = handle.AddrOfPinnedObject();

                LSA_OBJECT_ATTRIBUTES attributes = new LSA_OBJECT_ATTRIBUTES();
                attributes.Length = Marshal.SizeOf(attributes);

                Int32 status = LsaOpenPolicy(IntPtr.Zero, ref attributes, POLICY_LOOKUP_NAMES | POLICY_CREATE_ACCOUNT, out policy);

                if (status != 0)
                {
                    Int32 error = LsaNtStatusToWinError(status);

                    return WithMissingRights("The local security policy could not be opened.", error);
                }

                LSA_UNICODE_STRING[] rights = new LSA_UNICODE_STRING[1];
                rights[0] = new LSA_UNICODE_STRING();
                rights[0].Buffer = SE_SERVICE_LOGON_RIGHT;
                rights[0].Length = (UInt16)(SE_SERVICE_LOGON_RIGHT.Length * 2);
                rights[0].MaximumLength = (UInt16)((SE_SERVICE_LOGON_RIGHT.Length + 1) * 2);

                status = LsaAddAccountRights(policy, sid, rights, 1);

                if (status != 0)
                {
                    Int32 error = LsaNtStatusToWinError(status);

                    return WithMissingRights("The account \"" + accountName + "\" was set, but it is not allowed to log on as a service.", error);
                }

                return null;
            }
            finally
            {
                if (policy != IntPtr.Zero)
                {
                    LsaClose(policy);
                }

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        private static WindowsServiceWriteResult Fail(String message)
        {
            WindowsServiceWriteResult result = new WindowsServiceWriteResult();
            result.Success = false;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// Adds the hint about missing administrator rights to a failure message. The error code of the
        /// service control manager says "Access is denied", which is of no use to whoever clicked the button.
        /// </summary>
        private static String WithMissingRights(String message, Int32 error)
        {
            if (error != ERROR_ACCESS_DENIED && error != ERROR_PRIVILEGE_NOT_HELD)
            {
                return message + " (" + error + ")";
            }

            return message + "\r\n\r\n" + MissingRightsHint;
        }

        /// <summary>
        /// True when an exception reports that the application does not run with administrator rights. The
        /// installer answers with a security exception, the service control manager with access denied, and
        /// the local security policy with a privilege that is not held.
        /// </summary>
        public static bool IsMissingRights(Exception ex)
        {
            if (ex is SecurityException)
            {
                return true;
            }

            Win32Exception win32 = ex as Win32Exception;
            if (win32 == null)
            {
                return false;
            }

            return win32.NativeErrorCode == ERROR_ACCESS_DENIED || win32.NativeErrorCode == ERROR_PRIVILEGE_NOT_HELD;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_DELAYED_AUTO_START_INFO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool fDelayedAutostart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public UInt32 Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;

            [MarshalAs(UnmanagedType.LPWStr)]
            public String Buffer;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenSCManager(String lpMachineName, String lpDatabaseName, UInt32 dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr hSCManager, String lpServiceName, UInt32 dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ChangeServiceConfigW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig(IntPtr hService, UInt32 dwServiceType, UInt32 dwStartType,
            UInt32 dwErrorControl, String lpBinaryPathName, String lpLoadOrderGroup, IntPtr lpdwTagId,
            IntPtr lpDependencies, String lpServiceStartName,
            String lpPassword, String lpDisplayName);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "ChangeServiceConfig2W")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig2(IntPtr hService, UInt32 dwInfoLevel,
            ref SERVICE_DELAYED_AUTO_START_INFO lpInfo);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 LsaOpenPolicy(IntPtr SystemHandle, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            Int32 AccessMask, out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid,
            LSA_UNICODE_STRING[] UserRights, Int32 CountOfRights);

        [DllImport("advapi32.dll")]
        private static extern Int32 LsaClose(IntPtr ObjectHandle);

        [DllImport("advapi32.dll")]
        private static extern Int32 LsaNtStatusToWinError(Int32 status);
    }
}
