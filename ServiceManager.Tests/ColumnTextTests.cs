using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.UI.Columns;

namespace ServiceManager.Tests
{
    [TestClass]
    public class ColumnTextTests
    {
        private static readonly DateTime now = new DateTime(2024, 5, 1, 12, 0, 0);

        [TestMethod]
        public void the_activity_column_carries_no_text()
        {
            Assert.AreEqual(String.Empty, ColumnText.Get(eColumn.Activity, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void a_service_without_a_process_has_no_pid()
        {
            ServiceAnalytics data = newAnalytics();
            data.ProcessID = 0;

            Assert.AreEqual("-", ColumnText.Get(eColumn.Pid, newService(), data, now));
        }

        [TestMethod]
        public void the_pid_of_a_running_process_is_the_number()
        {
            ServiceAnalytics data = newAnalytics();
            data.ProcessID = Process.GetCurrentProcess().Id;

            Assert.AreEqual(data.ProcessID.ToString(), ColumnText.Get(eColumn.Pid, newService(), data, now));
        }

        [TestMethod]
        public void the_title_is_the_title_of_the_service()
        {
            ServiceItem item = newService();
            item.Title = "Backup agent";

            Assert.AreEqual("Backup agent", ColumnText.Get(eColumn.Title, item, newAnalytics(), now));
        }

        [TestMethod]
        public void the_status_is_the_english_text()
        {
            ServiceAnalytics data = newAnalytics();
            data.Status = ServiceAnalytics.eStatus.shutting_down;

            Assert.AreEqual("shutting down", ColumnText.Get(eColumn.Status, newService(), data, now));
        }

        [TestMethod]
        public void the_restarts_are_the_number()
        {
            ServiceAnalytics data = newAnalytics();
            data.Restarts = 3;

            Assert.AreEqual("3", ColumnText.Get(eColumn.Restarts, newService(), data, now));
        }

        [TestMethod]
        public void auto_restart_reads_as_yes()
        {
            ServiceItem item = newService();
            item.ForceRestart = true;

            Assert.AreEqual("yes", ColumnText.Get(eColumn.AutoRestart, item, newAnalytics(), now));
        }

        [TestMethod]
        public void auto_restart_reads_as_no()
        {
            Assert.AreEqual("no", ColumnText.Get(eColumn.AutoRestart, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void a_service_that_never_started_has_no_uptime()
        {
            Assert.AreEqual("-", ColumnText.Get(eColumn.Uptime, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void the_uptime_is_the_time_since_the_service_started()
        {
            ServiceAnalytics data = newAnalytics();
            data.Started = now.AddMinutes(-90);

            Assert.AreEqual("1h 30m", ColumnText.Get(eColumn.Uptime, newService(), data, now));
        }

        [TestMethod]
        public void a_service_that_never_answered_has_no_last_reply()
        {
            Assert.AreEqual("never", ColumnText.Get(eColumn.LastReply, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void nothing_measured_in_memory_reads_as_a_dash()
        {
            Assert.AreEqual("-", ColumnText.Get(eColumn.Memory, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void nothing_measured_in_processor_use_reads_as_a_dash()
        {
            Assert.AreEqual("-", ColumnText.Get(eColumn.Cpu, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void nothing_measured_in_threads_reads_as_a_dash()
        {
            Assert.AreEqual("-", ColumnText.Get(eColumn.Threads, newService(), newAnalytics(), now));
        }

        [TestMethod]
        public void the_measured_memory_shows_up_in_the_column()
        {
            ServiceAnalytics data = newAnalytics();
            data.WorkingSetBytes = 2048;

            Assert.AreEqual("2 KB", ColumnText.Get(eColumn.Memory, newService(), data, now));
        }

        [TestMethod]
        public void the_measured_processor_use_shows_up_in_the_column()
        {
            ServiceAnalytics data = newAnalytics();
            data.CpuPercent = 4;

            Assert.AreEqual("4.0 %", ColumnText.Get(eColumn.Cpu, newService(), data, now));
        }

        [TestMethod]
        public void the_measured_thread_count_shows_up_in_the_column()
        {
            ServiceAnalytics data = newAnalytics();
            data.ThreadCount = 12;

            Assert.AreEqual("12", ColumnText.Get(eColumn.Threads, newService(), data, now));
        }

        [TestMethod]
        public void a_service_that_went_offline_reads_empty_in_every_measured_column()
        {
            ServiceAnalytics data = newAnalytics();

            data.Started = now.AddMinutes(-90);
            data.LastPing = now.AddSeconds(-30);
            data.WorkingSetBytes = 2048;
            data.CpuPercent = 4;
            data.ThreadCount = 12;

            data.ClearMeasurements();

            Assert.AreEqual("-", ColumnText.Get(eColumn.Uptime, newService(), data, now));
            Assert.AreEqual("never", ColumnText.Get(eColumn.LastReply, newService(), data, now));
            Assert.AreEqual("-", ColumnText.Get(eColumn.Memory, newService(), data, now));
            Assert.AreEqual("-", ColumnText.Get(eColumn.Cpu, newService(), data, now));
            Assert.AreEqual("-", ColumnText.Get(eColumn.Threads, newService(), data, now));
        }

        private static ServiceItem newService()
        {
            return new ServiceItem();
        }

        private static ServiceAnalytics newAnalytics()
        {
            return new ServiceAnalytics();
        }
    }
}
