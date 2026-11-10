using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Asks the machine whether a Windows service is up. A seam, so the dependency wait of a managed
    /// service can be tested without a service control manager.
    /// </summary>
    public interface IWindowsServiceStatus
    {
        /// <summary>
        /// True while the named service runs. A name the machine does not offer counts as not running.
        /// </summary>
        bool IsRunning(String name);
    }
}
