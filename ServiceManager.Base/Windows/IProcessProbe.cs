using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Reads one process of the machine. A seam, so the measuring can be tested without a process.
    /// </summary>
    public interface IProcessProbe
    {
        /// <summary>
        /// The sample of one process, null when the process is gone or cannot be read.
        /// </summary>
        ProcessSample GetSample(Int32 processId);
    }
}
