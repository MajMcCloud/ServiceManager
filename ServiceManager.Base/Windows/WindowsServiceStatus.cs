using System;
using System.ServiceProcess;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Reads the state of one Windows service through the service control manager.
    /// </summary>
    public class WindowsServiceStatus : IWindowsServiceStatus
    {
        public WindowsServiceStatus()
        {
        }

        /// <summary>
        /// True while the named service runs. A service the machine does not offer, and a service the caller
        /// is not allowed to open, both answer "not running" rather than breaking the start of our own service.
        /// </summary>
        public bool IsRunning(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            try
            {
                using (ServiceController controller = new ServiceController(name.Trim()))
                {
                    return controller.Status == ServiceControllerStatus.Running;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
