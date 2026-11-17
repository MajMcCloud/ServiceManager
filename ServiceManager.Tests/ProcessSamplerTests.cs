using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Base.Windows;

namespace ServiceManager.Tests
{
    [TestClass]
    public class ProcessSamplerTests
    {
        [TestMethod]
        public void a_service_without_a_process_has_no_values()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 0;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 1024, 3)), 4);

            sampler.Apply(analytics);

            Assert.IsNull(analytics.WorkingSetBytes);
            Assert.IsNull(analytics.CpuPercent);
            Assert.IsNull(analytics.ThreadCount);
        }

        [TestMethod]
        public void null_analytics_are_ignored()
        {
            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 1024, 3)), 4);

            sampler.Apply(null);
        }

        [TestMethod]
        public void the_first_read_has_no_processor_use_yet()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 42;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 1024, 3)), 4);

            sampler.Apply(analytics);

            Assert.IsNull(analytics.CpuPercent);
            Assert.AreEqual(1024, analytics.WorkingSetBytes.Value);
            Assert.AreEqual(3, analytics.ThreadCount.Value);
        }

        [TestMethod]
        public void the_second_read_knows_the_processor_use()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 42;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 1024, 3), sample(1000, 200, 1024, 3)), 1);

            sampler.Apply(analytics);
            sampler.Apply(analytics);

            Assert.AreEqual(20, analytics.CpuPercent.Value, 0.01);
        }

        [TestMethod]
        public void a_process_that_cannot_be_read_keeps_the_last_values()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 42;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 2048, 5)), 4);

            sampler.Apply(analytics);
            sampler.Apply(analytics);

            Assert.AreEqual(2048, analytics.WorkingSetBytes.Value);
            Assert.AreEqual(5, analytics.ThreadCount.Value);
        }

        [TestMethod]
        public void a_process_that_cannot_be_read_forgets_the_previous_sample()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 42;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 1024, 3), null, sample(2000, 400, 1024, 3)), 1);

            sampler.Apply(analytics);
            sampler.Apply(analytics);
            sampler.Apply(analytics);

            Assert.IsNull(analytics.CpuPercent);
        }

        [TestMethod]
        public void a_new_process_with_the_same_id_gives_no_processor_use()
        {
            ServiceAnalytics analytics = new ServiceAnalytics();
            analytics.ProcessID = 42;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 9000, 1024, 3), sample(1000, 10, 1024, 3)), 4);

            sampler.Apply(analytics);
            sampler.Apply(analytics);

            Assert.IsNull(analytics.CpuPercent);
        }

        [TestMethod]
        public void every_service_keeps_its_own_samples()
        {
            ServiceAnalytics first = new ServiceAnalytics();
            first.ProcessID = 1;

            ServiceAnalytics second = new ServiceAnalytics();
            second.ProcessID = 2;

            ProcessSampler sampler = new ProcessSampler(new FakeProbe(sample(0, 0, 10, 1), sample(0, 0, 20, 2), sample(1000, 500, 10, 1), sample(1000, 250, 20, 2)), 1);

            sampler.Apply(first);
            sampler.Apply(second);
            sampler.Apply(first);
            sampler.Apply(second);

            Assert.AreEqual(50, first.CpuPercent.Value, 0.01);
            Assert.AreEqual(25, second.CpuPercent.Value, 0.01);
        }

        private static ProcessSample sample(Int32 offsetMs, Int32 busyMs, Int64 workingSetBytes, Int32 threadCount)
        {
            return new ProcessSample(threadCount, workingSetBytes, TimeSpan.FromMilliseconds(busyMs), new DateTime(2024, 5, 1, 12, 0, 0).AddMilliseconds(offsetMs));
        }

        private class FakeProbe : IProcessProbe
        {
            private readonly Queue<ProcessSample> m_Samples = new Queue<ProcessSample>();

            public FakeProbe(params ProcessSample[] samples)
            {
                foreach (ProcessSample sample in samples)
                {
                    m_Samples.Enqueue(sample);
                }
            }

            public ProcessSample GetSample(Int32 processId)
            {
                return m_Samples.Count > 0 ? m_Samples.Dequeue() : null;
            }
        }
    }
}
