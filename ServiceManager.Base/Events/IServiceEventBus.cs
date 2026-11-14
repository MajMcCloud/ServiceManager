using System;

namespace ServiceManager.Base.Events
{
    /// <summary>
    /// The in-process post office of the manager. Whoever notices something publishes it and returns,
    /// whoever wants to act on it subscribes once instead of being called by hand at every place that does
    /// something.
    /// </summary>
    public interface IServiceEventBus
    {
        /// <summary>Hands the event to the groups it belongs to without waiting for them.</summary>
        void Publish(ServiceEvent e);

        /// <summary>Listens to the events of one group until the returned token is disposed.</summary>
        IDisposable Subscribe(eGroup group, Action<ServiceEvent> handler);

        /// <summary>
        /// Waits until the queued events have been delivered. False when the wait ran out.
        /// </summary>
        bool Flush();

        /// <summary>Ends the queues. What is published afterwards goes nowhere.</summary>
        void Complete();

        /// <summary>How many events were dropped because too many log lines were waiting.</summary>
        int Dropped { get; }
    }
}
