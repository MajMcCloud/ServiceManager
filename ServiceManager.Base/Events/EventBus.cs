using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace ServiceManager.Base.Events
{
    /// <summary>
    /// Delivers an event on the thread of its group. Whoever publishes hands the event over and returns, so
    /// a slow subscriber - a provider that waits for its webhook - cannot hold up the thread that noticed the
    /// event. Every group has its own queue and its own thread, which keeps a slow provider from delaying the
    /// push to the client and puts the calls of one group on a single thread.
    ///
    /// The queues are unbounded and a producer never waits. What keeps them from growing is the high water
    /// mark over log lines, the only events that arrive in floods. A subscriber that throws only costs
    /// itself, which is the rule the provider dispatch already follows.
    /// </summary>
    public class EventBus : IServiceEventBus
    {
        /// <summary>How many log lines may be waiting before the next ones are dropped.</summary>
        public int LogHighWaterMark { get; set; }

        /// <summary>How long <see cref="Flush"/> waits for the queues to empty.</summary>
        public int FlushTimeout { get; set; }

        /// <summary>How many log lines the high water mark dropped.</summary>
        public int Dropped
        {
            get { return Interlocked.CompareExchange(ref DroppedCount, 0, 0); }
        }

        int DroppedCount;

        volatile bool Completed;

        readonly Dictionary<eGroup, Group> Groups = new Dictionary<eGroup, Group>();


        public EventBus()
        {
            LogHighWaterMark = 2000;
            FlushTimeout = 5000;

            Groups[eGroup.clients] = new Group(eGroup.clients);
            Groups[eGroup.providers] = new Group(eGroup.providers);
        }


        /// <summary>
        /// Hands the event to the groups that read it: the callback says what the clients are told, the kind
        /// says what the providers opted into. An event that carries both is queued twice on purpose, so one
        /// subscriber can never delay the other.
        /// </summary>
        public void Publish(ServiceEvent e)
        {
            if (e == null)
            {
                return;
            }

            if (e.Callback != eCallback.none)
            {
                Write(eGroup.clients, e);
            }

            if (e.Kind != null)
            {
                Write(eGroup.providers, e);
            }
        }


        /// <summary>Listens to the events of one group until the returned token is disposed.</summary>
        public IDisposable Subscribe(eGroup group, Action<ServiceEvent> handler)
        {
            var g = Groups[group];

            if (handler != null)
            {
                g.Add(handler);
                g.Start(Run);
            }

            return new Subscription(g, handler);
        }


        /// <summary>
        /// Waits until every queued event has been delivered. The shutdown handshake needs the events in
        /// front of it to reach the client first.
        /// </summary>
        public bool Flush()
        {
            var deadline = Environment.TickCount + FlushTimeout;

            while (Pending() > 0)
            {
                if (Environment.TickCount - deadline >= 0)
                {
                    return false;
                }

                Thread.Sleep(10);
            }

            return true;
        }


        /// <summary>Ends the queues. What is still queued is delivered, what follows goes nowhere.</summary>
        public void Complete()
        {
            Completed = true;

            foreach (var g in Groups.Values)
            {
                g.Queue.Writer.TryComplete();
            }
        }


        void Write(eGroup group, ServiceEvent e)
        {
            var g = Groups[group];

            if (Completed || g.SubscriberCount == 0)
            {
                return;
            }

            //The backlog, not the rate: a consumer that keeps up never trips over its own high water mark.
            if (IsLogLine(e) && g.Pending >= LogHighWaterMark)
            {
                CountDrop();
                return;
            }

            Interlocked.Increment(ref g.Pending);

            if (!g.Queue.Writer.TryWrite(e))
            {
                Interlocked.Decrement(ref g.Pending);
            }
        }


        /// <summary>Console output and the live log are the only events that come in floods.</summary>
        static bool IsLogLine(ServiceEvent e)
        {
            return e.Callback == eCallback.logs
                || e.Kind == NotificationKind.ConsoleOutput
                || e.Kind == NotificationKind.ConsoleError;
        }


        void CountDrop()
        {
            var total = Interlocked.Increment(ref DroppedCount);

            //One line for the whole run, a flood is not worth one line per event.
            if (total == 1)
            {
                Console.WriteLine("ServiceManager: " + LogHighWaterMark + " log lines are waiting, further log lines are dropped.");
            }
        }


        int Pending()
        {
            var total = 0;

            foreach (var g in Groups.Values)
            {
                total += g.Pending;
            }

            return total;
        }


        void Run(Group g)
        {
            var reader = g.Queue.Reader;

            while (true)
            {
                ServiceEvent e;

                while (reader.TryRead(out e))
                {
                    Deliver(g, e);

                    Interlocked.Decrement(ref g.Pending);
                }

                bool more;

                try
                {
                    more = reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult();
                }
                catch (ChannelClosedException)
                {
                    break;
                }

                if (!more)
                {
                    break;
                }
            }
        }


        void Deliver(Group g, ServiceEvent e)
        {
            //The list may change while a subscriber unsubscribes or subscribes, the snapshot does not
            foreach (var handler in g.Snapshot())
            {
                try
                {
                    handler(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ServiceManager: an event subscriber failed: " + ex.Message);
                }
            }
        }


        /// <summary>The queue, the subscribers and the thread of one group.</summary>
        sealed class Group
        {
            //The name Channel is taken by the WCF channel of this assembly, so the queue is named fully.
            internal readonly System.Threading.Channels.Channel<ServiceEvent> Queue;

            /// <summary>How many events were handed over and have not been delivered yet.</summary>
            internal int Pending;

            readonly List<Action<ServiceEvent>> Subscribers = new List<Action<ServiceEvent>>();

            readonly object Sync = new object();

            Thread Worker;


            internal Group(eGroup name)
            {
                Queue = System.Threading.Channels.Channel.CreateUnbounded<ServiceEvent>(new UnboundedChannelOptions()
                {
                    SingleReader = true,
                    SingleWriter = false
                });
            }


            internal int SubscriberCount
            {
                get
                {
                    lock (Sync)
                    {
                        return Subscribers.Count;
                    }
                }
            }


            internal void Add(Action<ServiceEvent> handler)
            {
                lock (Sync)
                {
                    Subscribers.Add(handler);
                }
            }


            internal void Remove(Action<ServiceEvent> handler)
            {
                lock (Sync)
                {
                    Subscribers.Remove(handler);
                }
            }


            internal Action<ServiceEvent>[] Snapshot()
            {
                lock (Sync)
                {
                    return Subscribers.ToArray();
                }
            }


            /// <summary>Starts the thread of the group, once, with its first subscriber.</summary>
            internal void Start(Action<Group> body)
            {
                lock (Sync)
                {
                    if (Worker != null)
                    {
                        return;
                    }

                    Worker = new Thread(() => body(this))
                    {
                        IsBackground = true,
                        Name = "ServiceManager events"
                    };

                    Worker.Start();
                }
            }
        }


        /// <summary>The token a subscriber hands back when it stops listening.</summary>
        sealed class Subscription : IDisposable
        {
            readonly Group Group;
            readonly Action<ServiceEvent> Handler;

            bool Disposed;


            internal Subscription(Group group, Action<ServiceEvent> handler)
            {
                Group = group;
                Handler = handler;
            }


            public void Dispose()
            {
                if (Disposed)
                {
                    return;
                }

                Disposed = true;

                Group.Remove(Handler);
            }
        }
    }
}
