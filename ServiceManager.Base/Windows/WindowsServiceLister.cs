using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;
using ServiceManager.Base.Data.Settings;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Reads the services of the machine. The list itself comes from the service control manager, the network
    /// stack on top of it from the registry: the manager does not report drivers such as Tcpip, and those are
    /// exactly the dependencies a service that needs the network asks for.
    /// </summary>
    public class WindowsServiceLister
    {
        const String ServicesKey = @"SYSTEM\CurrentControlSet\Services";

        // The documentation names shcore, the export itself sits in shlwapi on every system we support.
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int SHLoadIndirectString(String source, StringBuilder target, int size, IntPtr options);

        readonly Object m_Lock = new Object();

        List<WindowsServiceInfo> m_Services;

        /// <summary>
        /// Returns the services the picker offers, read once per lister and kept. A machine that does not
        /// answer yields an empty list rather than an exception.
        /// </summary>
        public List<WindowsServiceInfo> ListServices(bool includeNetworkStack)
        {
            lock (m_Lock)
            {
                if (m_Services == null)
                {
                    m_Services = Read();
                }

                return WindowsServiceQuery.Select(m_Services, includeNetworkStack);
            }
        }

        static List<WindowsServiceInfo> Read()
        {
            List<WindowsServiceInfo> ret = new List<WindowsServiceInfo>();

            foreach (ServiceController controller in ServiceController.GetServices())
            {
                WindowsServiceInfo info = new WindowsServiceInfo();

                info.ServiceName = controller.ServiceName;
                info.DisplayName = controller.DisplayName;
                if (String.IsNullOrEmpty(info.DisplayName))
                {
                    info.DisplayName = controller.ServiceName;
                }

                try
                {
                    info.Status = WindowsServiceQuery.StatusText(controller.Status);
                    info.StartType = WindowsServiceQuery.ToStartType(controller.StartType);
                }
                catch (Exception)
                {
                    // A handful of services report neither their state nor their start type.
                }

                ret.Add(info);
            }

            foreach (String name in WindowsService.StandardNetworkDependencies)
            {
                if (!ContainsName(ret, name))
                {
                    WindowsServiceInfo info = ReadRegistryService(name);

                    if (info != null)
                    {
                        ret.Add(info);
                    }
                }
            }

            return ret;
        }

        static WindowsServiceInfo ReadRegistryService(String name)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ServicesKey + "\\" + name))
                {
                    if (key == null)
                    {
                        return null;
                    }

                    WindowsServiceInfo info = new WindowsServiceInfo();

                    info.ServiceName = name;
                    info.DisplayName = ReadTitle(key, name);
                    info.Status = "";
                    info.StartType = WindowsServiceQuery.ToStartType(ReadStart(key));

                    return info;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static String ReadTitle(RegistryKey key, String name)
        {
            String value = key.GetValue("DisplayName") as String;

            if (String.IsNullOrEmpty(value))
            {
                return name;
            }

            if (WindowsServiceQuery.IsIndirectString(value))
            {
                String title = LoadIndirectString(value);

                return String.IsNullOrEmpty(title) ? name : title;
            }

            return Environment.ExpandEnvironmentVariables(value);
        }

        static int ReadStart(RegistryKey key)
        {
            try
            {
                Object value = key.GetValue("Start");

                if (value is Int32)
                {
                    return (Int32)value;
                }
            }
            catch (Exception)
            {
                // A damaged value stays unknown.
            }

            return -1;
        }

        static String LoadIndirectString(String source)
        {
            StringBuilder builder = new StringBuilder(1024);

            try
            {
                if (SHLoadIndirectString(source, builder, builder.Capacity, IntPtr.Zero) == 0)
                {
                    return builder.ToString();
                }
            }
            catch (Exception)
            {
                // A reference that cannot be resolved, the service name is shown instead.
            }

            return "";
        }

        static bool ContainsName(List<WindowsServiceInfo> list, String name)
        {
            foreach (WindowsServiceInfo info in list)
            {
                if (String.Equals(info.ServiceName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
