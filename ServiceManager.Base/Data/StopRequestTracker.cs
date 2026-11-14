using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;

namespace ServiceManager.Base.Data
{
    /// <summary>
    /// Remembers which services the manager itself asked to stop. A process that goes away after such a
    /// request ended on purpose, everything that ends without one ended on its own and counts as a crash.
    /// </summary>
    public class StopRequestTracker
    {
        readonly HashSet<Guid> Requested = new HashSet<Guid>();


        /// <summary>Notes that a stop was asked for. Asking twice is fine.</summary>
        public void Mark(Guid serviceId)
        {
            lock (Requested)
            {
                Requested.Add(serviceId);
            }
        }


        /// <summary>Forgets the request, a new process starts without one.</summary>
        public void Clear(Guid serviceId)
        {
            lock (Requested)
            {
                Requested.Remove(serviceId);
            }
        }


        /// <summary>True while a stop of this service is on its way.</summary>
        public bool Contains(Guid serviceId)
        {
            lock (Requested)
            {
                return Requested.Contains(serviceId);
            }
        }


        /// <summary>
        /// The event a process end stands for: the stop the user asked for, or a crash nobody asked for.
        /// </summary>
        public NotificationKind ReasonOf(Guid serviceId)
        {
            return Contains(serviceId) ? NotificationKind.ServiceStop : NotificationKind.ServiceCrashed;
        }
    }
}
