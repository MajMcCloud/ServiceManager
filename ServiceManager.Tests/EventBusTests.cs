using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Events;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The bus is the one place every part of the manager learns what the other parts do, so it has to
    /// reach every listener and keep a broken listener from costing the others theirs.
    /// </summary>
    [TestClass]
    public class EventBusTests
    {
        [TestMethod]
        public void a_subscriber_receives_the_event()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            bus.Subscribe(recorder.Handle);
            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(1, recorder.Events.Count);
            Assert.AreEqual(NotificationKind.ServiceStart, recorder.Events[0].Kind);
        }


        [TestMethod]
        public void every_subscriber_receives_the_event()
        {
            var bus = new EventBus();

            var first = new Recorder();
            var second = new Recorder();

            bus.Subscribe(first.Handle);
            bus.Subscribe(second.Handle);
            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            Assert.AreEqual(1, first.Events.Count);
            Assert.AreEqual(1, second.Events.Count);
        }


        [TestMethod]
        public void the_subscribers_that_came_first_are_reached_first()
        {
            var bus = new EventBus();

            var order = new List<String>();

            bus.Subscribe(e => order.Add("push"));
            bus.Subscribe(e => order.Add("providers"));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            CollectionAssert.AreEqual(new[] { "push", "providers" }, order);
        }


        [TestMethod]
        public void a_subscriber_that_throws_does_not_cost_the_next_one_the_event()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            bus.Subscribe(e => throw new InvalidOperationException("the push failed"));
            bus.Subscribe(recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(1, recorder.Events.Count, "a listener that cannot do its job must not stop the delivery");
        }


        [TestMethod]
        public void a_subscriber_that_throws_does_not_reach_itself_twice()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            bus.Subscribe(e => recorder.Handle(e));
            bus.Subscribe(e => throw new InvalidOperationException("the push failed"));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(1, recorder.Events.Count);
        }


        [TestMethod]
        public void disposing_the_token_stops_the_delivery()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            var token = bus.Subscribe(recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            token.Dispose();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            Assert.AreEqual(1, recorder.Events.Count, "the event after the token is gone must not arrive");
        }


        [TestMethod]
        public void disposing_a_token_twice_is_fine()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            var token = bus.Subscribe(recorder.Handle);

            token.Dispose();
            token.Dispose();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void an_event_without_a_subscriber_goes_nowhere_and_throws_nothing()
        {
            var bus = new EventBus();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServerShutdown });
        }


        [TestMethod]
        public void a_missing_event_is_ignored()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            bus.Subscribe(recorder.Handle);

            bus.Publish(null);

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void a_subscriber_without_a_handler_is_ignored()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            var token = bus.Subscribe(null);
            bus.Subscribe(recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(1, recorder.Events.Count);

            token.Dispose();
        }


        [TestMethod]
        public void a_subscriber_that_subscribes_during_an_event_is_not_reached_by_that_event()
        {
            var bus = new EventBus();

            var late = new Recorder();

            bus.Subscribe(e => bus.Subscribe(late.Handle));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.AreEqual(0, late.Events.Count, "the event that is being delivered goes to the listeners of that moment");

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            Assert.AreEqual(1, late.Events.Count);
        }


        [TestMethod]
        public void a_subscriber_that_unsubscribes_during_an_event_still_gets_the_rest_of_that_event()
        {
            var bus = new EventBus();

            var recorder = new Recorder();

            IDisposable token = null;

            token = bus.Subscribe(e =>
            {
                recorder.Handle(e);
                token.Dispose();
            });

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });
            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            Assert.AreEqual(1, recorder.Events.Count);
        }


        /// <summary>Keeps what the bus hands over so a test can read it back.</summary>
        private class Recorder
        {
            public List<ServiceEvent> Events { get; } = new List<ServiceEvent>();


            public void Handle(ServiceEvent e)
            {
                Events.Add(e);
            }
        }
    }
}
