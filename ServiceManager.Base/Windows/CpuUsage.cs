using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// The share of the machine a process used between two samples.
    /// </summary>
    public static class CpuUsage
    {
        /// <summary>
        /// The percent of all processors a process used between two samples. Null when the two samples
        /// cannot be compared, for example when nothing lay between them or the process was replaced.
        /// </summary>
        public static Double? GetPercent(ProcessSample previous, ProcessSample current, Int32 processorCount)
        {
            if (previous == null || current == null)
            {
                return null;
            }

            Double elapsed = (current.ReadTime - previous.ReadTime).TotalMilliseconds;

            if (elapsed <= 0)
            {
                return null;
            }

            Double busy = (current.TotalProcessorTime - previous.TotalProcessorTime).TotalMilliseconds;

            //A processor counter that went backwards means a new process took the id of the old one
            if (busy < 0)
            {
                return null;
            }

            Int32 processors = processorCount < 1 ? 1 : processorCount;

            Double percent = busy / (elapsed * processors) * 100;

            //Both values come from different clocks, so a busy process can sit a little over the line
            return percent > 100 ? 100 : percent;
        }
    }
}
