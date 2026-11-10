using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Progress;
using System;

namespace ServiceManager.Tests
{
    [TestClass]
    public class ProgressReportTests
    {
        [TestMethod]
        public void running_report_stays_indeterminate()
        {
            var report = ProgressReport.Running("Stopping the service");

            Assert.IsTrue(report.IsIndeterminate);
        }

        [TestMethod]
        public void running_report_has_no_percentage()
        {
            var report = ProgressReport.Running("Stopping the service");

            Assert.AreEqual(-1, report.Percent);
        }

        [TestMethod]
        public void a_step_report_names_the_share_of_the_steps()
        {
            var report = ProgressReport.Step("Starting a service", 3, 4);

            Assert.AreEqual(75, report.Percent);
        }

        [TestMethod]
        public void a_step_report_is_not_indeterminate()
        {
            var report = ProgressReport.Step("Starting a service", 1, 2);

            Assert.IsFalse(report.IsIndeterminate);
        }

        [TestMethod]
        public void the_last_step_of_a_single_step_run_is_complete()
        {
            var report = ProgressReport.Step("Installing the service", 1, 1);

            Assert.AreEqual(100, report.Percent);
        }

        [TestMethod]
        public void a_step_past_the_total_is_cut_to_the_total()
        {
            var report = ProgressReport.Step("Starting a service", 7, 4);

            Assert.AreEqual(4, report.Current);
        }

        [TestMethod]
        public void a_step_past_the_total_never_grows_past_a_hundred_percent()
        {
            var report = ProgressReport.Step("Starting a service", 7, 4);

            Assert.AreEqual(100, report.Percent);
        }

        [TestMethod]
        public void a_run_without_steps_stays_indeterminate()
        {
            var report = ProgressReport.Step("Starting a service", 1, 0);

            Assert.IsTrue(report.IsIndeterminate);
        }

        [TestMethod]
        public void a_step_that_has_not_started_stays_indeterminate()
        {
            var report = ProgressReport.Step("Starting a service", -1, 5);

            Assert.AreEqual(-1, report.Total);
        }

        [TestMethod]
        public void the_message_is_kept_as_it_was_given()
        {
            var report = ProgressReport.Running("Restarting the service");

            Assert.AreEqual("Restarting the service", report.Message);
        }

        [TestMethod]
        public void a_missing_message_reads_as_an_empty_one()
        {
            var report = ProgressReport.Running(null);

            Assert.AreEqual(String.Empty, report.Message);
        }
    }
}
