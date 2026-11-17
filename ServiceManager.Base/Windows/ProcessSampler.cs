using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Fills the measured values of the analytics of a service. The share of the processor is a difference,
    /// so the sampler keeps the last sample of every process it has read.
    /// </summary>
    public class ProcessSampler
    {
        private readonly IProcessProbe m_Probe;
        private readonly Int32 m_ProcessorCount;
        private readonly Dictionary<Int32, ProcessSample> m_Previous = new Dictionary<Int32, ProcessSample>();

        /// <summary>
        /// Reads the real processes of this machine.
        /// </summary>
        public ProcessSampler() : this(new ProcessProbe(), Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Measures through a seam, so a test can hand in samples of its own.
        /// </summary>
        public ProcessSampler(IProcessProbe probe, Int32 processorCount)
        {
            m_Probe = probe;
            m_ProcessorCount = processorCount;
        }

        /// <summary>
        /// Measures one service. A service without a process has no values at all, a process that cannot be
        /// read keeps the values of the last read until it is gone for good.
        /// </summary>
        public void Apply(ServiceAnalytics analytics)
        {
            if (analytics == null)
            {
                return;
            }

            if (analytics.ProcessID <= 0)
            {
                analytics.WorkingSetBytes = null;
                analytics.CpuPercent = null;
                analytics.ThreadCount = null;

                return;
            }

            ProcessSample current = m_Probe.GetSample(analytics.ProcessID);

            if (current == null)
            {
                lock (m_Previous)
                {
                    m_Previous.Remove(analytics.ProcessID);
                }

                return;
            }

            //Several clients may poll at the same time, and the two samples of one process belong together
            lock (m_Previous)
            {
                ProcessSample previous;

                if (m_Previous.TryGetValue(analytics.ProcessID, out previous))
                {
                    analytics.CpuPercent = CpuUsage.GetPercent(previous, current, m_ProcessorCount);
                }
                else
                {
                    analytics.CpuPercent = null;
                }

                m_Previous[analytics.ProcessID] = current;
            }

            analytics.WorkingSetBytes = current.WorkingSetBytes;
            analytics.ThreadCount = current.ThreadCount;
        }
    }
}
