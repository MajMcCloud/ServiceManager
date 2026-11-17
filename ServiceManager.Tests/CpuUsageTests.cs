using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Windows;

namespace ServiceManager.Tests
{
    [TestClass]
    public class CpuUsageTests
    {
        [TestMethod]
        public void no_value_without_a_previous_sample()
        {
            Double? percent = CpuUsage.GetPercent(null, sample(1000, 100), 4);

            Assert.IsNull(percent);
        }

        [TestMethod]
        public void no_value_without_a_current_sample()
        {
            Double? percent = CpuUsage.GetPercent(sample(1000, 100), null, 4);

            Assert.IsNull(percent);
        }

        [TestMethod]
        public void no_value_when_the_two_reads_lie_on_the_same_moment()
        {
            DateTime now = new DateTime(2024, 5, 1, 12, 0, 0);

            Double? percent = CpuUsage.GetPercent(new ProcessSample(1, 1, TimeSpan.FromMilliseconds(100), now), new ProcessSample(1, 1, TimeSpan.FromMilliseconds(200), now), 4);

            Assert.IsNull(percent);
        }

        [TestMethod]
        public void no_value_when_the_processor_time_went_backwards()
        {
            DateTime now = new DateTime(2024, 5, 1, 12, 0, 0);

            ProcessSample previous = new ProcessSample(1, 1, TimeSpan.FromMilliseconds(5000), now);
            ProcessSample current = new ProcessSample(1, 1, TimeSpan.FromMilliseconds(20), now.AddSeconds(1));

            Double? percent = CpuUsage.GetPercent(previous, current, 4);

            Assert.IsNull(percent);
        }

        [TestMethod]
        public void a_quarter_of_a_processor_is_25_percent()
        {
            Double? percent = CpuUsage.GetPercent(sample(0, 0), sample(1000, 250), 1);

            Assert.AreEqual(25, percent.Value, 0.01);
        }

        [TestMethod]
        public void the_share_is_spread_over_the_processors_of_the_machine()
        {
            Double? percent = CpuUsage.GetPercent(sample(0, 0), sample(1000, 500), 4);

            Assert.AreEqual(12.5, percent.Value, 0.01);
        }

        [TestMethod]
        public void a_busy_processor_on_every_core_reads_as_100_percent()
        {
            Double? percent = CpuUsage.GetPercent(sample(0, 0), sample(1000, 4000), 4);

            Assert.AreEqual(100, percent.Value, 0.01);
        }

        [TestMethod]
        public void the_value_never_grows_over_100_percent()
        {
            Double? percent = CpuUsage.GetPercent(sample(0, 0), sample(1000, 9000), 4);

            Assert.AreEqual(100, percent.Value, 0.01);
        }

        [TestMethod]
        public void a_machine_without_processor_count_reads_as_one_processor()
        {
            Double? percent = CpuUsage.GetPercent(sample(0, 0), sample(1000, 250), 0);

            Assert.AreEqual(25, percent.Value, 0.01);
        }

        private static ProcessSample sample(Int32 elapsedMs, Int32 busyMs)
        {
            return new ProcessSample(1, 1, TimeSpan.FromMilliseconds(busyMs), new DateTime(2024, 5, 1, 12, 0, 0).AddMilliseconds(elapsedMs));
        }
    }
}
