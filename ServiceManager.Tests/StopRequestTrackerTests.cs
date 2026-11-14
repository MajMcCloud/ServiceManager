using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Data;
using ServiceManager.Extensions.Interfaces;
using System;

namespace ServiceManager.Tests
{
    /// <summary>
    /// A process that goes away can either be the stop somebody asked for or a crash nobody asked
    /// for, and the only thing that tells the two apart is whether a stop was noted beforehand.
    /// </summary>
    [TestClass]
    public class StopRequestTrackerTests
    {
        private static readonly Guid Service = new Guid("11111111-1111-1111-1111-111111111111");


        [TestMethod]
        public void a_service_nobody_asked_to_stop_ends_as_a_crash()
        {
            var tracker = new StopRequestTracker();

            Assert.AreEqual(NotificationKind.ServiceCrashed, tracker.ReasonOf(Service));
        }


        [TestMethod]
        public void a_marked_stop_ends_as_a_stop()
        {
            var tracker = new StopRequestTracker();

            tracker.Mark(Service);

            Assert.AreEqual(NotificationKind.ServiceStop, tracker.ReasonOf(Service));
        }


        [TestMethod]
        public void clearing_forgets_the_stop_again()
        {
            var tracker = new StopRequestTracker();

            tracker.Mark(Service);
            tracker.Clear(Service);

            Assert.AreEqual(NotificationKind.ServiceCrashed, tracker.ReasonOf(Service));
        }


        [TestMethod]
        public void a_stop_of_one_service_does_not_cover_another()
        {
            var other = new Guid("22222222-2222-2222-2222-222222222222");

            var tracker = new StopRequestTracker();

            tracker.Mark(Service);

            Assert.IsTrue(tracker.Contains(Service));
            Assert.IsFalse(tracker.Contains(other));
        }


        [TestMethod]
        public void a_service_that_was_stopped_twice_is_still_one_stop()
        {
            var tracker = new StopRequestTracker();

            tracker.Mark(Service);
            tracker.Mark(Service);

            Assert.IsTrue(tracker.Contains(Service));
        }


        [TestMethod]
        public void clearing_a_service_that_was_never_stopped_changes_nothing()
        {
            var tracker = new StopRequestTracker();

            tracker.Clear(Service);

            Assert.IsFalse(tracker.Contains(Service));
        }
    }
}
