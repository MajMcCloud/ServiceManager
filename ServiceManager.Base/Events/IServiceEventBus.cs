using System;

namespace ServiceManager.Base.Events
{
    /// <summary>
    /// The in-process post office of the manager. Whoever notices something publishes it, whoever wants
    /// to act on it subscribes once instead of being called by hand at every place that does something.
    /// </summary>
    public interface IServiceEventBus
    {
        /// <summary>Hands the event to every subscriber, in the order they subscribed.</summary>
        void Publish(ServiceEvent e);

        /// <summary>Listens to every event until the returned token is disposed.</summary>
        IDisposable Subscribe(Action<ServiceEvent> handler);
    }
}
