using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.data;
using System;

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

        // Every line has the same length, so the tail can be measured line by line.
        private static String Line(int Number)
        {
            return "line" + Number.ToString("D6") + "\r\n";
        }
    }
}
