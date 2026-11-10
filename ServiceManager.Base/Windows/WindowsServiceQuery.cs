using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using ServiceManager.Base.Data.Settings;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// The decisions behind a service list: which services are worth showing, which one a filter keeps and
    /// how the machine reports a start type. Nothing here talks to the machine, which is what makes these
    /// rules testable.
    /// </summary>
    public static class WindowsServiceQuery
    {
        /// <summary>
        /// True when the service control manager is able to start the service. Drivers loaded during boot and
        /// disabled services are not, and both are only noise in a dependency list.
        /// </summary>
        public static bool IsStartable(WindowsServiceInfo info)
        {
            if (info == null)
            {
                return false;
            }

            return info.StartType == WindowsServiceInfo.eStartType.Automatic
                || info.StartType == WindowsServiceInfo.eStartType.Manual;
        }

        /// <summary>
        /// True when a service passes the filter. The filter is matched against the title and the service
        /// name, without regard to case, and an empty filter keeps everything.
        /// </summary>
        public static bool Matches(WindowsServiceInfo info, String filter)
        {
            if (info == null)
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            String needle = filter.Trim();

            return Contains(info.DisplayName, needle) || Contains(info.ServiceName, needle);
        }

        /// <summary>
        /// Builds the list a picker offers: the services the manager can start, the network stack on top when
        /// it is asked for, and everything ordered by title. The network stack is asked for on purpose, so its
        /// drivers stay in the list even though the manager cannot start them by themselves.
        /// </summary>
        public static List<WindowsServiceInfo> Select(IEnumerable<WindowsServiceInfo> all, bool includeNetworkStack)
        {
            List<WindowsServiceInfo> source = new List<WindowsServiceInfo>();

            if (all != null)
            {
                source.AddRange(all);
            }

            List<WindowsServiceInfo> ret = new List<WindowsServiceInfo>();

            foreach (WindowsServiceInfo info in source)
            {
                if (IsStartable(info))
                {
                    ret.Add(info);
                }
            }

            if (includeNetworkStack)
            {
                foreach (String name in WindowsService.StandardNetworkDependencies)
                {
                    if (ContainsName(ret, name))
                    {
                        continue;
                    }

                    WindowsServiceInfo found = source.FirstOrDefault(m => String.Equals(m.ServiceName, name, StringComparison.OrdinalIgnoreCase));

                    ret.Add(found ?? CreatePlaceholder(name));
                }
            }

            return ret.OrderBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase)
                      .ThenBy(m => m.ServiceName, StringComparer.OrdinalIgnoreCase)
                      .ToList();
        }

        /// <summary>Maps the start type of the service control manager onto our own values.</summary>
        public static WindowsServiceInfo.eStartType ToStartType(ServiceStartMode mode)
        {
            switch (mode)
            {
                case ServiceStartMode.Boot:
                    return WindowsServiceInfo.eStartType.Boot;
                case ServiceStartMode.System:
                    return WindowsServiceInfo.eStartType.System;
                case ServiceStartMode.Automatic:
                    return WindowsServiceInfo.eStartType.Automatic;
                case ServiceStartMode.Manual:
                    return WindowsServiceInfo.eStartType.Manual;
                case ServiceStartMode.Disabled:
                    return WindowsServiceInfo.eStartType.Disabled;
                default:
                    return WindowsServiceInfo.eStartType.Unknown;
            }
        }

        /// <summary>
        /// Maps the Start value of the registry onto our own values. The registry uses the same numbers as the
        /// service control manager, a number outside that range stays unknown.
        /// </summary>
        public static WindowsServiceInfo.eStartType ToStartType(int start)
        {
            switch (start)
            {
                case 0:
                    return WindowsServiceInfo.eStartType.Boot;
                case 1:
                    return WindowsServiceInfo.eStartType.System;
                case 2:
                    return WindowsServiceInfo.eStartType.Automatic;
                case 3:
                    return WindowsServiceInfo.eStartType.Manual;
                case 4:
                    return WindowsServiceInfo.eStartType.Disabled;
                default:
                    return WindowsServiceInfo.eStartType.Unknown;
            }
        }

        /// <summary>The start type as English text.</summary>
        public static String StartTypeText(WindowsServiceInfo.eStartType startType)
        {
            switch (startType)
            {
                case WindowsServiceInfo.eStartType.Boot:
                    return "Boot";
                case WindowsServiceInfo.eStartType.System:
                    return "System";
                case WindowsServiceInfo.eStartType.Automatic:
                    return "Automatic";
                case WindowsServiceInfo.eStartType.Manual:
                    return "Manual";
                case WindowsServiceInfo.eStartType.Disabled:
                    return "Disabled";
                default:
                    return "Unknown";
            }
        }

        /// <summary>The state of a service as English text, empty when there is nothing to report.</summary>
        public static String StatusText(ServiceControllerStatus status)
        {
            switch (status)
            {
                case ServiceControllerStatus.Running:
                    return "Running";
                case ServiceControllerStatus.Stopped:
                    return "Stopped";
                case ServiceControllerStatus.StartPending:
                    return "Starting";
                case ServiceControllerStatus.StopPending:
                    return "Stopping";
                case ServiceControllerStatus.Paused:
                    return "Paused";
                case ServiceControllerStatus.PausePending:
                    return "Pausing";
                case ServiceControllerStatus.ContinuePending:
                    return "Continuing";
                default:
                    return "";
            }
        }

        /// <summary>
        /// True when a registry value is a reference to a resource inside a file, "@%SystemRoot%\system32\
        /// dnsapi.dll,-101" for example, rather than the title itself.
        /// </summary>
        public static bool IsIndirectString(String value)
        {
            return value != null && value.StartsWith("@", StringComparison.Ordinal);
        }

        static bool Contains(String value, String needle)
        {
            if (String.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool ContainsName(List<WindowsServiceInfo> list, String name)
        {
            return list.Any(m => String.Equals(m.ServiceName, name, StringComparison.OrdinalIgnoreCase));
        }

        static WindowsServiceInfo CreatePlaceholder(String name)
        {
            WindowsServiceInfo info = new WindowsServiceInfo();

            info.ServiceName = name;
            info.DisplayName = name;
            info.Status = "";
            info.StartType = WindowsServiceInfo.eStartType.Unknown;

            return info;
        }
    }
}
