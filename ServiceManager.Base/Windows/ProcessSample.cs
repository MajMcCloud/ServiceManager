using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// How one process looked at one moment. The values come straight from the operating system.
    /// </summary>
    public class ProcessSample
    {
        public ProcessSample(Int32 threadCount, Int64 workingSetBytes, TimeSpan totalProcessorTime, DateTime readTime)
        {
            this.ThreadCount = threadCount;
            this.WorkingSetBytes = workingSetBytes;
            this.TotalProcessorTime = totalProcessorTime;
            this.ReadTime = readTime;
        }

        /// <summary>
        /// How many threads the process had.
        /// </summary>
        public Int32 ThreadCount { get; private set; }

        /// <summary>
        /// Memory the process holds in physical memory, in bytes.
        /// </summary>
        public Int64 WorkingSetBytes { get; private set; }

        /// <summary>
        /// Processor time the process has used since it started.
        /// </summary>
        public TimeSpan TotalProcessorTime { get; private set; }

        /// <summary>
        /// When the sample was taken. Only samples taken on the same machine may be compared.
        /// </summary>
        public DateTime ReadTime { get; private set; }
    }
}
