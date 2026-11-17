using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Reads a process of this machine.
    /// </summary>
    public class ProcessProbe : IProcessProbe
    {
        /// <summary>
        /// The sample of one process. A process that is gone, was never there or belongs to someone else
        /// gives null, the service list has to keep working for it.
        /// </summary>
        public ProcessSample GetSample(Int32 processId)
        {
            if (processId <= 0)
            {
                return null;
            }

            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    process.Refresh();

                    return new ProcessSample(process.Threads.Count, process.WorkingSet64, process.TotalProcessorTime, DateTime.Now);
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (Win32Exception)
            {
                return null;
            }
        }
    }
}
