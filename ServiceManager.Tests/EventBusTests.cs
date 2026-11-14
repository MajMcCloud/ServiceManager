using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base.Events;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The bus is the one place every part of the manager learns what the other parts do. It has to reach
    /// every listener, keep a broken listener from costing the others theirs, and hand an event over without
    /// letting the thread that noticed it wait for the slowest one.
    /// </summary>
    [TestClass]
    public class EventBusTests
    {
        private EventBus bus;


        [TestInitialize]
        public void Setup()
        {
            bus = new EventBus();
        }


        [TestCleanup]
        public void Teardown()
        {
            bus.Complete();
        }


        [TestMethod]
        public void a_subscriber_receives_the_event()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count);
            Assert.AreEqual(NotificationKind.ServiceStart, recorder.Events[0].Kind);
        }


        [TestMethod]
        public void every_subscriber_of_a_group_receives_the_event()
        {
            var first = new Recorder();
            var second = new Recorder();

            bus.Subscribe(eGroup.providers, first.Handle);
            bus.Subscribe(eGroup.providers, second.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            bus.Flush();

            Assert.AreEqual(1, first.Events.Count);
            Assert.AreEqual(1, second.Events.Count);
        }


        [TestMethod]
        public void the_subscribers_that_came_first_are_reached_first()
        {
            var order = new List<String>();

            bus.Subscribe(eGroup.providers, e => order.Add("push"));
            bus.Subscribe(eGroup.providers, e => order.Add("providers"));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            CollectionAssert.AreEqual(new[] { "push", "providers" }, order);
        }


        [TestMethod]
        public void an_event_that_only_the_clients_read_never_reaches_a_provider()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Callback = eCallback.updated });

            bus.Flush();

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void an_event_that_only_the_providers_read_never_reaches_a_client()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.clients, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void an_event_that_carries_both_halves_reaches_both_groups()
        {
            var clients = new Recorder();
            var providers = new Recorder();

            bus.Subscribe(eGroup.clients, clients.Handle);
            bus.Subscribe(eGroup.providers, providers.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.DependencyWait, Callback = eCallback.updated });

            bus.Flush();

            Assert.AreEqual(1, clients.Events.Count);
            Assert.AreEqual(1, providers.Events.Count);
        }


        [TestMethod]
        public void the_events_of_a_group_keep_the_order_they_were_published_in()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.clients, recorder.Handle);

            for (int i = 0; i < 50; i++)
            {
                bus.Publish(new ServiceEvent() { Callback = eCallback.updated, Details = i.ToString() });
            }

            bus.Flush();

            Assert.AreEqual(50, recorder.Events.Count);

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(i.ToString(), recorder.Events[i].Details);
            }
        }


        [TestMethod]
        public void publishing_does_not_wait_for_a_slow_subscriber()
        {
            var watch = new Stopwatch();

            bus.Subscribe(eGroup.providers, e => Thread.Sleep(500));

            watch.Start();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds < 200, "the thread that noticed the event hands it over and returns");
        }


        [TestMethod]
        public void a_slow_provider_does_not_hold_up_the_push_to_the_client()
        {
            var blocked = new Blocked();
            var clients = new Recorder();

            bus.Subscribe(eGroup.providers, blocked.Handle);
            bus.Subscribe(eGroup.clients, clients.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart, Callback = eCallback.updated });

            var reached = WaitsFor(() => clients.Events.Count == 1, 1000);

            blocked.Release();

            Assert.IsTrue(reached, "the client is told while the provider is still busy with the same event");
        }


        [TestMethod]
        public void log_lines_over_the_high_water_mark_are_dropped_and_counted()
        {
            bus.LogHighWaterMark = 5;

            var blocked = new Blocked();

            bus.Subscribe(eGroup.providers, blocked.Handle);

            for (int i = 0; i < 20; i++)
            {
                bus.Publish(new ServiceEvent() { Kind = NotificationKind.ConsoleOutput, Details = i.ToString() });
            }

            Assert.AreEqual(15, bus.Dropped);

            blocked.Release();

            bus.Flush();

            Assert.AreEqual(5, blocked.Events.Count);
        }


        [TestMethod]
        public void a_state_event_is_kept_while_the_log_lines_are_dropped()
        {
            bus.LogHighWaterMark = 2;

            var blocked = new Blocked();

            bus.Subscribe(eGroup.providers, blocked.Handle);

            for (int i = 0; i < 5; i++)
            {
                bus.Publish(new ServiceEvent() { Kind = NotificationKind.ConsoleOutput, Details = i.ToString() });
            }

            for (int i = 0; i < 10; i++)
            {
                bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop, Details = i.ToString() });
            }

            blocked.Release();

            bus.Flush();

            Assert.AreEqual(12, blocked.Events.Count, "two log lines and every state event");
            Assert.AreEqual(3, bus.Dropped);
        }


        [TestMethod]
        public void a_live_log_line_counts_as_a_log_line()
        {
            bus.LogHighWaterMark = 1;

            var blocked = new Blocked();

            bus.Subscribe(eGroup.clients, blocked.Handle);

            bus.Publish(new ServiceEvent() { Callback = eCallback.logs, Details = "one" });
            bus.Publish(new ServiceEvent() { Callback = eCallback.logs, Details = "two" });

            Assert.AreEqual(1, bus.Dropped);

            blocked.Release();

            bus.Flush();

            Assert.AreEqual(1, blocked.Events.Count);
        }


        [TestMethod]
        public void the_first_drop_is_reported_once()
        {
            bus.LogHighWaterMark = 1;

            var blocked = new Blocked();

            bus.Subscribe(eGroup.clients, blocked.Handle);

            bus.Publish(new ServiceEvent() { Callback = eCallback.logs, Details = "one" });

            var lines = Capture(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    bus.Publish(new ServiceEvent() { Callback = eCallback.logs, Details = i.ToString() });
                }
            });

            blocked.Release();

            Assert.AreEqual(1, lines.Count, "a flood is not worth one line per event");
            Assert.AreEqual(10, bus.Dropped);
        }


        [TestMethod]
        public void flush_reports_that_the_queues_emptied()
        {
            bus.Subscribe(eGroup.providers, e => Thread.Sleep(50));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            Assert.IsTrue(bus.Flush());
        }


        [TestMethod]
        public void completing_the_bus_stops_the_delivery()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();
            bus.Complete();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count, "what is published after the queues are gone goes nowhere");
        }


        [TestMethod]
        public void a_subscriber_that_throws_does_not_cost_the_next_one_the_event()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, e => throw new InvalidOperationException("the push failed"));
            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count, "a listener that cannot do its job must not stop the delivery");
        }


        [TestMethod]
        public void a_subscriber_that_throws_does_not_reach_itself_twice()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, e => recorder.Handle(e));
            bus.Subscribe(eGroup.providers, e => throw new InvalidOperationException("the push failed"));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count);
        }


        [TestMethod]
        public void disposing_the_token_stops_the_delivery()
        {
            var recorder = new Recorder();

            var token = bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            token.Dispose();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count, "the event after the token is gone must not arrive");
        }


        [TestMethod]
        public void disposing_a_token_twice_is_fine()
        {
            var recorder = new Recorder();

            var token = bus.Subscribe(eGroup.providers, recorder.Handle);

            token.Dispose();
            token.Dispose();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void an_event_without_a_subscriber_goes_nowhere_and_throws_nothing()
        {
            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServerShutdown });

            Assert.IsTrue(bus.Flush());
        }


        [TestMethod]
        public void a_missing_event_is_ignored()
        {
            var recorder = new Recorder();

            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(null);

            bus.Flush();

            Assert.AreEqual(0, recorder.Events.Count);
        }


        [TestMethod]
        public void a_subscriber_without_a_handler_is_ignored()
        {
            var recorder = new Recorder();

            var token = bus.Subscribe(eGroup.providers, null);
            bus.Subscribe(eGroup.providers, recorder.Handle);

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count);

            token.Dispose();
        }


        [TestMethod]
        public void a_subscriber_that_subscribes_during_an_event_is_not_reached_by_that_event()
        {
            var late = new Recorder();

            bus.Subscribe(eGroup.providers, e => bus.Subscribe(eGroup.providers, late.Handle));

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            Assert.AreEqual(0, late.Events.Count, "the event that is being delivered goes to the listeners of that moment");

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            bus.Flush();

            Assert.AreEqual(1, late.Events.Count);
        }


        [TestMethod]
        public void a_subscriber_that_unsubscribes_during_an_event_still_gets_the_rest_of_that_event()
        {
            var recorder = new Recorder();

            IDisposable token = null;

            token = bus.Subscribe(eGroup.providers, e =>
            {
                recorder.Handle(e);
                token.Dispose();
            });

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStart });

            bus.Flush();

            bus.Publish(new ServiceEvent() { Kind = NotificationKind.ServiceStop });

            bus.Flush();

            Assert.AreEqual(1, recorder.Events.Count);
        }


        static bool WaitsFor(Func<bool> condition, int milliseconds)
        {
            var deadline = Environment.TickCount + milliseconds;

            while (Environment.TickCount < deadline)
            {
                if (condition())
                {
                    return true;
                }

                Thread.Sleep(10);
            }

            return condition();
        }


        /// <summary>Runs an action and keeps the lines it writes to the console.</summary>
        static List<String> Capture(Action action)
        {
            var lines = new List<String>();

            var writer = new LineCollector(lines);
            var previous = Console.Out;

            Console.SetOut(writer);

            try
            {
                action();
            }
            finally
            {
                Console.SetOut(previous);
                writer.Flush();
            }

            return lines;
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


        /// <summary>A subscriber that holds the event it took first until the test lets it go.</summary>
        private class Blocked
        {
            readonly ManualResetEventSlim Gate = new ManualResetEventSlim(false);

            public List<ServiceEvent> Events { get; } = new List<ServiceEvent>();


            public void Handle(ServiceEvent e)
            {
                Events.Add(e);

                if (Events.Count == 1)
                {
                    Gate.Wait(5000);
                }
            }


            public void Release()
            {
                Gate.Set();
            }
        }


        /// <summary>A console writer that keeps every line instead of printing it.</summary>
        private class LineCollector : System.IO.StringWriter
        {
            readonly List<String> Lines;


            internal LineCollector(List<String> lines)
            {
                Lines = lines;
            }


            public override void WriteLine(String value)
            {
                Lines.Add(value);
            }
        }
    }
}
