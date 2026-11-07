using System;

namespace ServiceManager.Extensions.Interfaces
{
    /// <summary>
    /// An event next to the plain text an extension has always received. Providers that build cards,
    /// set an event level or pick a subject need the structured fields, providers that only ever
    /// print a line can keep using Text.
    /// </summary>
    public class NotificationMessage
    {
        public NotificationKind Kind { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Name of this ServiceManager instance, as configured by the user.</summary>
        public String InstanceName { get; set; }

        /// <summary>Managed service the event belongs to, null for host wide events.</summary>
        public String ServiceTitle { get; set; }

        /// <summary>Console line or error text, null when the event carries none.</summary>
        public String Details { get; set; }

        /// <summary>
        /// The line older extensions receive. Identical to what Send(String) was handed before, so a
        /// provider keeps its output when it is moved to the structured contract.
        /// </summary>
        public String Text { get; set; }


        public bool IsError
        {
            get { return Kind == NotificationKind.ConsoleError; }
        }
    }
}
