using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Both streams are sent to the UI with every analytics answer and they collect the whole
    /// console of a service, so they must not grow without end.
    /// </summary>
    [TestClass]
    public class ServiceAnalyticsTests
    {
        [TestMethod]
        public void append_output_keeps_everything_below_the_limit()
        {
            var analytics = new ServiceAnalytics();

            analytics.AppendOutput("first\r\n");
            analytics.AppendOutput("second\r\n");

            Assert.AreEqual("first\r\nsecond\r\n", analytics.Output);
        }

        [TestMethod]
        public void append_output_never_grows_past_the_limit()
        {
            var analytics = new ServiceAnalytics();

            for (var i = 0; i < 40000; i++)
            {
                analytics.AppendOutput(Line(i));
            }

            Assert.IsTrue(analytics.Output.Length <= ServiceAnalytics.MaxStreamLength, "the stream has to stay below " + ServiceAnalytics.MaxStreamLength);
        }

        [TestMethod]
        public void append_output_keeps_the_newest_lines()
        {
            var analytics = new ServiceAnalytics();

            for (var i = 0; i < 40000; i++)
            {
                analytics.AppendOutput(Line(i));
            }

            Assert.IsTrue(analytics.Output.EndsWith(Line(39999)), "the newest line is the last one in the stream");
        }

        [TestMethod]
        public void append_output_cuts_on_a_line_boundary()
        {
            var analytics = new ServiceAnalytics();

            for (var i = 0; i < 40000; i++)
            {
                analytics.AppendOutput(Line(i));
            }

            Assert.AreEqual(0, analytics.Output.Length % Line(0).Length, "the first line of the tail has to be a whole line");
        }

        [TestMethod]
        public void append_error_never_grows_past_the_limit()
        {
            var analytics = new ServiceAnalytics();

            for (var i = 0; i < 40000; i++)
            {
                analytics.AppendError(Line(i));
            }

            Assert.IsTrue(analytics.Error.Length <= ServiceAnalytics.MaxStreamLength, "the error stream has to stay below " + ServiceAnalytics.MaxStreamLength);
        }

        [TestMethod]
        public void waiting_for_dependency_reads_the_way_a_person_would()
        {
            Assert.AreEqual("waiting for dependency", ServiceAnalytics.StatusText(ServiceAnalytics.eStatus.waiting_for_dependency));
        }

        [TestMethod]
        public void every_status_has_its_own_text()
        {
            var texts = new List<String>();

            foreach (ServiceAnalytics.eStatus status in Enum.GetValues(typeof(ServiceAnalytics.eStatus)))
            {
                String text = ServiceAnalytics.StatusText(status);

                Assert.IsFalse(String.IsNullOrWhiteSpace(text), status + " has to be readable");
                Assert.IsFalse(texts.Contains(text), status + " shares its text with another status");

                texts.Add(text);
            }
        }

        [TestMethod]
        public void an_unknown_status_has_an_unknown_text()
        {
            Assert.AreEqual("unknown", ServiceAnalytics.StatusText((ServiceAnalytics.eStatus)42));
        }

        [TestMethod]
        public void status_values_are_stable_because_the_ui_receives_them_as_numbers()
        {
            // The analytics cross the WCF boundary as JSON, and an enum without a converter is a number
            // there. Renaming a status is free, changing its number relabels every running installation.
            var expected = new Dictionary<String, int>
            {
                { "unknown", -1 },
                { "starting", 1 },
                { "started", 2 },
                { "running", 3 },
                { "shutting_down", 4 },
                { "offline", 5 },
                { "start_failed", 6 },
                { "failed", 7 },
                { "updated", 8 },
                { "waiting_for_dependency", 9 }
            };

            foreach (var pair in expected)
            {
                Assert.IsTrue(Enum.IsDefined(typeof(ServiceAnalytics.eStatus), pair.Key), pair.Key + " has to stay a status");

                var analytics = new ServiceAnalytics();

                analytics.Status = (ServiceAnalytics.eStatus)Enum.Parse(typeof(ServiceAnalytics.eStatus), pair.Key);

                var json = JObject.Parse(JsonConvert.SerializeObject(analytics));

                Assert.AreEqual(pair.Value, (int)json["Status"], pair.Key + " has to keep the number " + pair.Value);
            }
        }

        // Every line has the same length, so the tail can be measured line by line.
        private static String Line(int Number)
        {
            return "line" + Number.ToString("D6") + "\r\n";
        }
    }
}
