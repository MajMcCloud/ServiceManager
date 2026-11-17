using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.UI.Columns;

namespace ServiceManager.Tests
{
    [TestClass]
    public class ValueFormatTests
    {
        private static readonly DateTime now = new DateTime(2024, 5, 1, 12, 0, 0);

        [TestMethod]
        public void no_uptime_reads_as_a_dash()
        {
            Assert.AreEqual("-", ValueFormat.Uptime(null, now));
        }

        [TestMethod]
        public void a_start_in_the_future_reads_as_a_dash()
        {
            Assert.AreEqual("-", ValueFormat.Uptime(now.AddMinutes(5), now));
        }

        [TestMethod]
        public void an_uptime_below_a_minute_shows_the_seconds()
        {
            Assert.AreEqual("48s", ValueFormat.Uptime(now.AddSeconds(-48), now));
        }

        [TestMethod]
        public void an_uptime_above_a_minute_shows_minutes_and_seconds()
        {
            Assert.AreEqual("2m 5s", ValueFormat.Uptime(now.AddSeconds(-125), now));
        }

        [TestMethod]
        public void an_uptime_above_an_hour_shows_hours_and_minutes()
        {
            Assert.AreEqual("2h 15m", ValueFormat.Uptime(now.AddMinutes(-135), now));
        }

        [TestMethod]
        public void an_uptime_above_a_day_shows_days_and_hours()
        {
            Assert.AreEqual("3d 4h", ValueFormat.Uptime(now.AddHours(-76), now));
        }

        [TestMethod]
        public void a_service_that_never_answered_reads_as_never()
        {
            Assert.AreEqual("never", ValueFormat.LastReply(DateTime.MinValue, now));
        }

        [TestMethod]
        public void the_last_reply_shows_the_time_since_it_happened()
        {
            Assert.AreEqual("30s", ValueFormat.LastReply(now.AddSeconds(-30), now));
        }

        [TestMethod]
        public void no_memory_reads_as_a_dash()
        {
            Assert.AreEqual("-", ValueFormat.Memory(null));
        }

        [TestMethod]
        public void memory_below_a_kilobyte_shows_the_bytes()
        {
            Assert.AreEqual("512 B", ValueFormat.Memory(512));
        }

        [TestMethod]
        public void memory_shows_kilobytes_above_a_kilobyte()
        {
            Assert.AreEqual("2 KB", ValueFormat.Memory(2048));
        }

        [TestMethod]
        public void memory_shows_megabytes_above_a_megabyte()
        {
            Assert.AreEqual("12.3 MB", ValueFormat.Memory(12897484));
        }

        [TestMethod]
        public void memory_shows_gigabytes_above_a_gigabyte()
        {
            Assert.AreEqual("1.5 GB", ValueFormat.Memory(1610612736));
        }

        [TestMethod]
        public void no_processor_use_reads_as_a_dash()
        {
            Assert.AreEqual("-", ValueFormat.Cpu(null));
        }

        [TestMethod]
        public void the_processor_use_shows_one_decimal()
        {
            Assert.AreEqual("12.5 %", ValueFormat.Cpu(12.5));
        }

        [TestMethod]
        public void a_processor_use_without_a_value_shows_a_zero()
        {
            Assert.AreEqual("0.0 %", ValueFormat.Cpu(0));
        }

        [TestMethod]
        public void no_thread_count_reads_as_a_dash()
        {
            Assert.AreEqual("-", ValueFormat.Threads(null));
        }

        [TestMethod]
        public void the_thread_count_is_the_number()
        {
            Assert.AreEqual("37", ValueFormat.Threads(37));
        }
    }
}
