using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Asks the machine whether the own process runs with elevated rights. A seam, so the interface can
    /// decide what to mark without reading the access token of the running process.
    /// </summary>
    public interface IProcessElevation
    {
        /// <summary>
        /// True when the own process holds elevated rights. A process that was started by a member of the
        /// administrators group but without "Run as administrator" does not.
        /// </summary>
        bool IsElevated();
    }
}
