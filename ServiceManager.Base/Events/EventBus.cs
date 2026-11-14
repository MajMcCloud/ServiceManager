using System;
using System.Collections.Generic;

namespace ServiceManager.Base.Events
{
    /// <summary>
    /// Delivers an event to every subscriber on the thread that published it. A subscriber that throws
    /// only costs itself, which is the rule the provider dispatch already follows.
    /// </summary>
    public class EventBus : IServiceEventBus
    {
        readonly List<Action<ServiceEvent>> Subscribers = new List<Action<ServiceEvent>>();


        /// <summary>Hands the event to every subscriber, in the order they subscribed.</summary>
        public void Publish(ServiceEvent e)
        {
            if (e == null)
            {
                return;
            }

            //The list may change while a subscriber unsubscribes or subscribes, the snapshot does not
            Action<ServiceEvent>[] current;

            lock (Subscribers)
            {
                current = Subscribers.ToArray();
            }

            foreach (var handler in current)
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


        /// <summary>Listens to every event until the returned token is disposed.</summary>
        public IDisposable Subscribe(Action<ServiceEvent> handler)
        {
            if (handler != null)
            {
                lock (Subscribers)
                {
                    Subscribers.Add(handler);
                }
            }

            return new Subscription(this, handler);
        }


        void Unsubscribe(Action<ServiceEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            lock (Subscribers)
            {
                Subscribers.Remove(handler);
            }
        }


        /// <summary>The token a subscriber hands back when it stops listening.</summary>
        sealed class Subscription : IDisposable
        {
            readonly EventBus Bus;
            readonly Action<ServiceEvent> Handler;

            bool Disposed;


            internal Subscription(EventBus bus, Action<ServiceEvent> handler)
            {
                Bus = bus;
                Handler = handler;
            }


            public void Dispose()
            {
                if (Disposed)
                {
                    return;
                }

                Disposed = true;

                Bus.Unsubscribe(Handler);
            }
        }
    }
}
