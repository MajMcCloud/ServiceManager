using ServiceManager.Base.Data;
using ServiceManager.Extensions.Interfaces;
using System;

namespace ServiceManager.Base.Events
{
    /// <summary>
    /// One thing that happened inside the manager. The kind is what the notification providers read,
    /// the callback what the connected clients read; an event carries one of them, the other, or both.
    /// </summary>
    public class ServiceEvent
    {
        /// <summary>The event a provider can opt into. Null when no provider is to hear about it.</summary>
        public NotificationKind? Kind { get; set; }

        /// <summary>The push the connected clients get. None when nobody is to be pushed.</summary>
        public eCallback Callback { get; set; } = eCallback.none;

        public Guid ServiceId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>The service the event belongs to, null for an event about the manager itself.</summary>
        public ServiceItem Service { get; set; }

        /// <summary>The analytics of that service as they were when the event happened.</summary>
        public ServiceAnalytics Analytics { get; set; }

        /// <summary>Name of the service, for the providers that build a message out of the fields.</summary>
        public String Title { get; set; }

        /// <summary>The console line, the missing dependency, or whatever else the event is about.</summary>
        public String Details { get; set; }

        /// <summary>The wording the providers have always received for this kind of event.</summary>
        public String Text { get; set; }
    }
}
