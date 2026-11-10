using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A managed service only starts once its Windows dependencies run. The decisions behind that wait are
    /// here, the machine itself is replaced by a fake.
    /// </summary>
    [TestClass]
    public class WindowsDependencyWaitTests
    {
        [TestMethod]
        public void first_missing_is_null_when_every_dependency_runs()
        {
            var status = new FakeStatus("Tcpip", "Dhcp");

            Assert.IsNull(WindowsDependencyWait.FirstMissing(new List<String> { "Tcpip", "Dhcp" }, status));
        }

        [TestMethod]
        public void first_missing_names_the_dependency_that_does_not_run()
        {
            var status = new FakeStatus("Tcpip");

            Assert.AreEqual("Dhcp", WindowsDependencyWait.FirstMissing(new List<String> { "Tcpip", "Dhcp" }, status));
        }

        [TestMethod]
        public void first_missing_names_the_dependency_the_machine_does_not_offer()
        {
            var status = new FakeStatus();

            Assert.AreEqual("TelegramApi", WindowsDependencyWait.FirstMissing(new List<String> { "TelegramApi" }, status));
        }

        [TestMethod]
        public void first_missing_is_null_without_dependencies()
        {
            var status = new FakeStatus();

            Assert.IsNull(WindowsDependencyWait.FirstMissing(new List<String>(), status));
            Assert.IsNull(WindowsDependencyWait.FirstMissing(null, status));
        }

        [TestMethod]
        public void first_missing_skips_names_without_a_value()
        {
            var status = new FakeStatus("Dhcp");

            Assert.IsNull(WindowsDependencyWait.FirstMissing(new List<String> { "", null, "  ", "Dhcp" }, status));
        }

        [TestMethod]
        public void first_missing_never_asks_the_machine_without_a_seam()
        {
            Assert.IsNull(WindowsDependencyWait.FirstMissing(new List<String> { "Tcpip" }, null));
        }

        [TestMethod]
        public void has_timed_out_is_false_while_the_budget_is_open()
        {
            var started = new DateTime(2025, 5, 1, 12, 0, 0);

            Assert.IsFalse(WindowsDependencyWait.HasTimedOut(started, 120, started.AddSeconds(119)));
        }

        [TestMethod]
        public void has_timed_out_is_true_once_the_budget_is_used()
        {
            var started = new DateTime(2025, 5, 1, 12, 0, 0);

            Assert.IsTrue(WindowsDependencyWait.HasTimedOut(started, 120, started.AddSeconds(120)));
        }

        [TestMethod]
        public void has_timed_out_is_true_at_once_for_a_budget_of_zero()
        {
            var started = new DateTime(2025, 5, 1, 12, 0, 0);

            Assert.IsTrue(WindowsDependencyWait.HasTimedOut(started, 0, started), "a budget of zero means do not wait");
            Assert.IsTrue(WindowsDependencyWait.HasTimedOut(started, -1, started));
        }

        [TestMethod]
        public void wait_text_names_the_instance_the_service_and_the_missing_service()
        {
            String text = WindowsDependencyWait.WaitText("Manager", "Telegram bot", "Tcpip");

            StringAssert.Contains(text, "Manager");
            StringAssert.Contains(text, "Telegram bot");
            StringAssert.Contains(text, "Tcpip");
        }

        [TestMethod]
        public void timeout_text_names_the_missing_service_and_the_budget()
        {
            String text = WindowsDependencyWait.TimeoutText("Manager", "Telegram bot", "Tcpip", 120);

            StringAssert.Contains(text, "Tcpip");
            StringAssert.Contains(text, "120");
        }

        [TestMethod]
        public void wait_error_lands_in_the_error_stream_of_the_service()
        {
            String error = WindowsDependencyWait.WaitError("Tcpip");

            StringAssert.Contains(error, "Tcpip");
            Assert.IsTrue(error.EndsWith("\r\n"), "a stream line has to end with a line break");
        }

        [TestMethod]
        public void timeout_error_names_the_missing_service_and_the_budget()
        {
            String error = WindowsDependencyWait.TimeoutError("Tcpip", 120);

            StringAssert.Contains(error, "Tcpip");
            StringAssert.Contains(error, "120");
            Assert.IsTrue(error.EndsWith("\r\n"), "a stream line has to end with a line break");
        }

        /// <summary>A machine on which exactly the listed services run.</summary>
        private class FakeStatus : IWindowsServiceStatus
        {
            private readonly List<String> running;

            public FakeStatus(params String[] running)
            {
                this.running = new List<String>(running);
            }

            public bool IsRunning(String name)
            {
                return running.Contains(name, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
