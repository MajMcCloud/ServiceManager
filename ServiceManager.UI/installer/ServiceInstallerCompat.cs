using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ServiceManager.UI.Installer
{
    /// <summary>
    /// What the service control manager answered to an install or a removal. The form shows the
    /// message when the work did not go through.
    /// </summary>
    public class ServiceInstallResult
    {
        public bool Success { get; set; }

        /// <summary>What to show the user. Empty when everything worked out.</summary>
        public String Message { get; set; }

        /// <summary>The error code of the service control manager, zero when nothing failed.</summary>
        public Int32 ErrorCode { get; set; }
    }

    /// <summary>
    /// Installs and removes the ServiceManager service. The install and uninstall classes of
    /// .NET Framework are gone in .NET 11, so the service control manager is written to directly,
    /// the same way <see cref="WindowsServiceConfigWriter"/> does it for an installed service.
    /// </summary>
    public class ServiceInstallerCompat
    {
        /// <summary>The startup type the service is registered with.</summary>
        public enum eStartType
        {
            Automatic = 0,
            AutomaticDelayed = 1,
            Manual = 2,
            Disabled = 3
        }

        /// <summary>The account the service process logs on with.</summary>
        public enum eAccountType
        {
            LocalSystem = 0,
            LocalService = 1,
            NetworkService = 2,
            User = 3
        }

        /// <summary>The name the service is registered under.</summary>
        public String ServiceName { get; set; }

        /// <summary>The name shown in the list of services.</summary>
        public String DisplayName { get; set; }

        /// <summary>The description shown in the properties of the service.</summary>
        public String Description { get; set; }

        /// <summary>
        /// The executable of the service with its command line. It has to be quoted when the path
        /// holds spaces, and ServiceManager needs its "-s" switch here.
        /// </summary>
        public String BinaryPath { get; set; }

        public eStartType StartType { get; set; } = eStartType.Automatic;

        /// <summary>The services that have to be up before this one starts.</summary>
        public String[] ServicesDependedOn { get; set; }

        public eAccountType Account { get; set; } = eAccountType.LocalSystem;

        /// <summary>The account name, only read for the User account.</summary>
        public String UserName { get; set; }

        /// <summary>The password of the named account, plain here and never stored here.</summary>
        public String Password { get; set; }

        /// <summary>Starts the service once it is registered.</summary>
        public bool StartOnInstall { get; set; }

        /// <summary>The time in milliseconds the start of the service is followed on install.</summary>
        public Int32 StartTimeout { get; set; } = 15000;

        /// <summary>The time in seconds the failure count of the service is kept, the recovery page of the dialog.</summary>
        public Int32 FailCountResetTime { get; set; }

        /// <summary>The message sent to the console when the service reboots the machine.</summary>
        public String FailRebootMsg { get; set; }

        /// <summary>The program that is started when a failure action runs a command.</summary>
        public String FailRunCommand { get; set; }

        /// <summary>The recovery actions of the service, read in the order they are given.</summary>
        public List<FailureAction> FailureActions { get; } = new List<FailureAction>();

        /// <summary>
        /// Registers the service and writes the settings that go with it. A service with the same
        /// name is reported instead of changed, that is what removing it first is for.
        /// </summary>
        public ServiceInstallResult Install()
        {
            if (String.IsNullOrEmpty(ServiceName))
            {
                return Fail("The service needs a name.", 0);
            }

            if (String.IsNullOrEmpty(BinaryPath))
            {
                return Fail("The service needs the path of its executable.", 0);
            }

            String account = ToAccountName();

            if (Account == eAccountType.User && account == null)
            {
                return Fail("The service account needs a user name.", 0);
            }

            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);

            if (scm == IntPtr.Zero)
            {
                return FromLastError("The service control manager could not be opened.");
            }

            try
            {
                IntPtr dependencies = Marshal.StringToHGlobalUni(BuildDependencyList(ServicesDependedOn));

                try
                {
                    IntPtr service = CreateService(scm, ServiceName, DisplayName, SERVICE_ALL_ACCESS,
                        SERVICE_WIN32_OWN_PROCESS, ToStartType(StartType), SERVICE_ERROR_NORMAL,
                        BinaryPath, IntPtr.Zero, IntPtr.Zero, dependencies, account, Password, null);

                    if (service == IntPtr.Zero)
                    {
                        Int32 error = Marshal.GetLastWin32Error();

                        if (error == ERROR_SERVICE_EXISTS)
                        {
                            return Fail("The service \"" + ServiceName + "\" is already installed. Remove it before installing it again.", error);
                        }

                        return FromLastError("The service \"" + ServiceName + "\" could not be registered.", error);
                    }

                    try
                    {
                        ServiceInstallResult result = ApplyExtraConfig(service);

                        if (!result.Success)
                        {
                            return result;
                        }
                    }
                    finally
                    {
                        CloseServiceHandle(service);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(dependencies);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }

            if (!StartOnInstall)
            {
                return new ServiceInstallResult { Success = true };
            }

            return StartInstalledService();
        }

        /// <summary>
        /// Removes the service from the service control manager. A running service is stopped first,
        /// a service that still has open handles stays behind until they are closed.
        /// </summary>
        public ServiceInstallResult Uninstall()
        {
            if (String.IsNullOrEmpty(ServiceName))
            {
                return Fail("The service needs a name.", 0);
            }

            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_CONNECT);

            if (scm == IntPtr.Zero)
            {
                return FromLastError("The service control manager could not be opened.");
            }

            try
            {
                IntPtr service = OpenService(scm, ServiceName, DELETE | SERVICE_STOP | SERVICE_QUERY_STATUS);

                if (service == IntPtr.Zero)
                {
                    Int32 error = Marshal.GetLastWin32Error();

                    if (error == ERROR_SERVICE_DOES_NOT_EXIST)
                    {
                        return Fail("The service \"" + ServiceName + "\" is not installed.", error);
                    }

                    return FromLastError("The service \"" + ServiceName + "\" could not be opened.", error);
                }

                try
                {
                    StopInstalledService();

                    if (!DeleteService(service))
                    {
                        Int32 error = Marshal.GetLastWin32Error();

                        if (error == ERROR_SERVICE_MARKED_FOR_DELETE)
                        {
                            return Fail("The service \"" + ServiceName + "\" is marked for removal, but a program still holds it open. Close the list of services and try again.", error);
                        }

                        return FromLastError("The service \"" + ServiceName + "\" could not be removed.", error);
                    }
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                CloseServiceHandle(scm);
            }

            return new ServiceInstallResult { Success = true };
        }

        /// <summary>
        /// Looks the service up in the service control manager. A service that is not registered is
        /// told apart from a lookup that itself failed, so the form never blames the user for it.
        /// </summary>
        public ServiceInstallResult CheckInstalled()
        {
            if (String.IsNullOrEmpty(ServiceName))
            {
                return Fail("The service needs a name.", 0);
            }

            IntPtr scm = OpenSCManager(null, null, SC_MANAGER_CONNECT);

            if (scm == IntPtr.Zero)
            {
                return FromLastError("The service control manager could not be opened.");
            }

            try
            {
                IntPtr service = OpenService(scm, ServiceName, SERVICE_QUERY_CONFIG);

                if (service == IntPtr.Zero)
                {
                    Int32 error = Marshal.GetLastWin32Error();

                    if (error == ERROR_SERVICE_DOES_NOT_EXIST)
                    {
                        return Fail("The service \"" + ServiceName + "\" is not installed.", error);
                    }

                    return FromLastError("The service \"" + ServiceName + "\" could not be opened.", error);
                }

                CloseServiceHandle(service);

                return new ServiceInstallResult { Success = true };
            }
            finally
            {
                CloseServiceHandle(scm);
            }
        }

        /// <summary>
        /// Writes the parts the service control manager keeps apart from the service itself: the
        /// description, the delayed start and the recovery actions.
        /// </summary>
        private ServiceInstallResult ApplyExtraConfig(IntPtr service)
        {
            if (!String.IsNullOrEmpty(Description))
            {
                SERVICE_DESCRIPTION description = new SERVICE_DESCRIPTION();
                description.lpDescription = Description;

                if (!ChangeServiceConfig2(service, SERVICE_CONFIG_DESCRIPTION, ref description))
                {
                    return FromLastError("The service was registered, but its description could not be written.", Marshal.GetLastWin32Error());
                }
            }

            if (StartType == eStartType.Automatic || StartType == eStartType.AutomaticDelayed)
            {
                SERVICE_DELAYED_AUTO_START_INFO delayed = new SERVICE_DELAYED_AUTO_START_INFO();
                delayed.fDelayedAutostart = StartType == eStartType.AutomaticDelayed;

                if (!ChangeServiceConfig2(service, SERVICE_CONFIG_DELAYED_AUTO_START_INFO, ref delayed))
                {
                    return FromLastError("The service was registered, but its start behaviour could not be written.", Marshal.GetLastWin32Error());
                }
            }

            if (FailureActions.Count == 0)
            {
                return new ServiceInstallResult { Success = true };
            }

            return ApplyFailureActions(service);
        }

        /// <summary>
        /// Writes the recovery actions of the service. The list is variable in length, so its entries
        /// are built in unmanaged memory and handed over as a pointer. A reboot action is only taken
        /// when this process is allowed to shut the machine down.
        /// </summary>
        private ServiceInstallResult ApplyFailureActions(IntPtr service)
        {
            Int32[] actions = new Int32[FailureActions.Count * 2];
            Int32 index = 0;
            bool needsShutdownRight = false;

            foreach (FailureAction action in FailureActions)
            {
                actions[index] = (Int32)action.Type;
                actions[index + 1] = action.Delay;
                index += 2;

                if (action.Type == RecoverAction.Reboot)
                {
                    needsShutdownRight = true;
                }
            }

            if (needsShutdownRight && !GrantShutdownPrivilege())
            {
                return Fail("The service was registered, but this program is not allowed to reboot the machine, which one of the recovery actions asks for.", 0);
            }

            IntPtr actionsBuffer = Marshal.AllocHGlobal(actions.Length * sizeof(Int32));

            try
            {
                Marshal.Copy(actions, 0, actionsBuffer, actions.Length);

                SERVICE_FAILURE_ACTIONS failure = new SERVICE_FAILURE_ACTIONS();
                failure.dwResetPeriod = FailCountResetTime;
                failure.lpRebootMsg = FailRebootMsg;
                failure.lpCommand = FailRunCommand;
                failure.cActions = (UInt32)FailureActions.Count;
                failure.lpActions = actionsBuffer;

                if (!ChangeServiceFailureActions(service, SERVICE_CONFIG_FAILURE_ACTIONS, ref failure))
                {
                    return FromLastError("The service was registered, but its recovery actions could not be written.", Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                Marshal.FreeHGlobal(actionsBuffer);
            }

            return new ServiceInstallResult { Success = true };
        }

        /// <summary>
        /// Starts the service through the service control manager and waits until it runs. The service
        /// is registered by then, so a start that fails is reported instead of hiding the service.
        /// </summary>
        private ServiceInstallResult StartInstalledService()
        {
            try
            {
                using (ServiceController controller = new ServiceController(ServiceName))
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(StartTimeout));
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException)
            {
                return Fail("The service was installed, but it could not be started: " + ex.Message, 0);
            }

            return new ServiceInstallResult { Success = true };
        }

        /// <summary>
        /// Stops a running service before it is removed. What the stop costs is not asked for, the
        /// removal itself reports a service that stays behind.
        /// </summary>
        private void StopInstalledService()
        {
            try
            {
                using (ServiceController controller = new ServiceController(ServiceName))
                {
                    controller.Refresh();

                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        return;
                    }

                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1));
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Grants the shutdown privilege to this process, which the recovery action "reboot the
        /// machine" asks for. The service control manager refuses the whole set of actions without it.
        /// </summary>
        private static bool GrantShutdownPrivilege()
        {
            IntPtr token = IntPtr.Zero;

            try
            {
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out token))
                {
                    return false;
                }

                Int64 luid;

                if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out luid))
                {
                    return false;
                }

                TOKEN_PRIVILEGES privileges = new TOKEN_PRIVILEGES();
                privileges.PrivilegeCount = 1;
                privileges.Privileges.Luid = luid;
                privileges.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

                if (!AdjustTokenPrivileges(token, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    return false;
                }

                return Marshal.GetLastWin32Error() == 0;
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }
        }

        /// <summary>The account the service control manager logs the service on with, null for LocalSystem.</summary>
        private String ToAccountName()
        {
            switch (Account)
            {
                case eAccountType.LocalService:

                    return "NT AUTHORITY\\LocalService";

                case eAccountType.NetworkService:

                    return "NT AUTHORITY\\NetworkService";

                case eAccountType.User:

                    if (String.IsNullOrWhiteSpace(UserName))
                    {
                        return null;
                    }

                    return UserName.Trim();

                case eAccountType.LocalSystem:

                    return null;

                default:

                    throw new ArgumentOutOfRangeException(nameof(Account));
            }
        }

        private static UInt32 ToStartType(eStartType startType)
        {
            switch (startType)
            {
                case eStartType.Automatic:
                case eStartType.AutomaticDelayed:

                    return SERVICE_AUTO_START;

                case eStartType.Manual:

                    return SERVICE_DEMAND_START;

                case eStartType.Disabled:

                    return SERVICE_DISABLED;

                default:

                    throw new ArgumentOutOfRangeException(nameof(startType));
            }
        }

        /// <summary>
        /// The dependency list in the shape the service control manager expects it: one name per
        /// entry, each closed by a null character, the whole list closed by a second one. An empty
        /// list is a closed list, not a null pointer, that one would mean the set stays as it is.
        /// </summary>
        private static String BuildDependencyList(String[] dependencies)
        {
            if (dependencies == null || dependencies.Length == 0)
            {
                return "\0\0";
            }

            return String.Join("\0", dependencies) + "\0\0";
        }

        private static ServiceInstallResult Fail(String message, Int32 error)
        {
            ServiceInstallResult result = new ServiceInstallResult();
            result.Success = false;
            result.Message = message;
            result.ErrorCode = error;

            return result;
        }

        /// <summary>Builds the failure message out of the last error of the service control manager.</summary>
        private static ServiceInstallResult FromLastError(String message)
        {
            return FromLastError(message, Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Adds the hint about missing administrator rights to a failure message, the error code of
        /// the service control manager is of no use to whoever clicked the button.
        /// </summary>
        private static ServiceInstallResult FromLastError(String message, Int32 error)
        {
            String text;

            if (error == ERROR_ACCESS_DENIED || error == ERROR_PRIVILEGE_NOT_HELD)
            {
                text = message + "\r\n\r\n" + WindowsServiceConfigWriter.MissingRightsHint;
            }
            else
            {
                text = message + " (" + error + ")";
            }

            return Fail(text, error);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SERVICE_DESCRIPTION
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public String lpDescription;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SERVICE_FAILURE_ACTIONS
        {
            public Int32 dwResetPeriod;

            [MarshalAs(UnmanagedType.LPWStr)]
            public String lpRebootMsg;

            [MarshalAs(UnmanagedType.LPWStr)]
            public String lpCommand;

            public UInt32 cActions;

            /// <summary>The recovery entries, eight bytes each, built in unmanaged memory.</summary>
            public IntPtr lpActions;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_DELAYED_AUTO_START_INFO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool fDelayedAutostart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public Int64 Luid;
            public UInt32 Attributes;
        }

        // The privileges field is a list of variable length, the packing puts it right behind the count.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        private const UInt32 SC_MANAGER_CONNECT = 0x00000001;
        private const UInt32 SC_MANAGER_CREATE_SERVICE = 0x00000002;

        private const UInt32 SERVICE_ALL_ACCESS = 0x000F01FF;
        private const UInt32 SERVICE_QUERY_CONFIG = 0x00000001;
        private const UInt32 SERVICE_QUERY_STATUS = 0x00000004;
        private const UInt32 SERVICE_STOP = 0x00000020;
        private const UInt32 DELETE = 0x00010000;

        private const UInt32 SERVICE_WIN32_OWN_PROCESS = 0x00000010;

        private const UInt32 SERVICE_ERROR_NORMAL = 0x00000001;
        private const UInt32 SERVICE_AUTO_START = 0x00000002;
        private const UInt32 SERVICE_DEMAND_START = 0x00000003;
        private const UInt32 SERVICE_DISABLED = 0x00000004;

        private const UInt32 SERVICE_CONFIG_DESCRIPTION = 0x00000001;
        private const UInt32 SERVICE_CONFIG_FAILURE_ACTIONS = 0x00000002;
        private const UInt32 SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 0x00000003;

        private const Int32 ERROR_ACCESS_DENIED = 5;
        private const Int32 ERROR_PRIVILEGE_NOT_HELD = 0x00000522;
        private const Int32 ERROR_SERVICE_DOES_NOT_EXIST = 0x00000424;
        private const Int32 ERROR_SERVICE_EXISTS = 0x0000042D;
        private const Int32 ERROR_SERVICE_MARKED_FOR_DELETE = 0x00000430;

        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const UInt32 TOKEN_QUERY = 0x00000008;
        private const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        private const String SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "OpenSCManagerW")]
        private static extern IntPtr OpenSCManager(String machineName, String databaseName, UInt32 desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "OpenServiceW")]
        private static extern IntPtr OpenService(IntPtr scm, String serviceName, UInt32 desiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(IntPtr serviceHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteService(IntPtr service);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateServiceW")]
        private static extern IntPtr CreateService(IntPtr scm, String lpServiceName, String lpDisplayName,
            UInt32 desiredAccess, UInt32 serviceType, UInt32 startType, UInt32 errorControl,
            String lpBinaryPathName, IntPtr lpLoadOrderGroup, IntPtr lpdwTagId, IntPtr lpDependencies,
            String lpServiceStartName, String lpPassword, String lpDisplayNameAfterCreate);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "ChangeServiceConfig2W")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig2(IntPtr service, UInt32 infoLevel, ref SERVICE_DESCRIPTION info);

        [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "ChangeServiceConfig2W")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig2(IntPtr service, UInt32 infoLevel, ref SERVICE_DELAYED_AUTO_START_INFO info);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "ChangeServiceConfig2W")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceFailureActions(IntPtr service, UInt32 infoLevel, ref SERVICE_FAILURE_ACTIONS info);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr process, UInt32 desiredAccess, out IntPtr token);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "LookupPrivilegeValueW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValue(String systemName, String name, out Int64 luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(IntPtr token, bool disableAll,
            ref TOKEN_PRIVILEGES newState, UInt32 bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);
    }

    /// <summary>The recovery action the service control manager takes after a failure.</summary>
    public enum RecoverAction
    {
        None = 0,
        Restart = 1,
        Reboot = 2,
        RunCommand = 3
    }

    /// <summary>A recovery action of the service: what it does and how long it waits beforehand.</summary>
    public class FailureAction
    {
        /// <summary>The action the service control manager takes.</summary>
        public RecoverAction Type { get; set; }

        /// <summary>The delay in milliseconds before the action runs.</summary>
        public Int32 Delay { get; set; }

        public FailureAction()
        {

        }

        public FailureAction(RecoverAction type, Int32 delay)
        {
            this.Type = type;
            this.Delay = delay;
        }
    }
}
