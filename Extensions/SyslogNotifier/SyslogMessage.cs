using ServiceManager.Extensions.Interfaces;
using System;
using System.Globalization;
using System.Text;

namespace ServiceManager.Extensions.SyslogNotifier
{
    /// <summary>
    /// How one event looks on the wire. Kept apart from the transport so a test can read the line of
    /// an event without a collector in the way, and so both formats stay readable on their own.
    /// </summary>
    public static class SyslogMessage
    {
        /// <summary>RFC 3164 has no hard limit, but every collector cuts around this size.</summary>
        public const int DefaultMaxLength = 1024;


        /// <summary>
        /// The severity numbers of RFC 5424. Same rating as the event log, the webhook and the mail
        /// provider, so a rule in the collector agrees with a mail rule of the same installation.
        /// </summary>
        public static int SeverityOf(NotificationKind kind)
        {
            switch (kind)
            {
                case NotificationKind.ConsoleError:
                case NotificationKind.ServiceCrashed:
                    return 3;

                case NotificationKind.ServiceStop:
                case NotificationKind.ServiceRestart:
                case NotificationKind.DependencyWait:
                    return 4;

                default:
                    return 6;
            }
        }


        /// <summary>
        /// facility * 8 + severity, the number in pointy brackets in front of every syslog line. An
        /// out of range facility falls back to local0 rather than producing a line no collector reads.
        /// </summary>
        public static int PriorityOf(int facility, NotificationKind kind)
        {
            var part = facility;

            if (part < 0 || part > 23)
                part = 16;

            return part * 8 + SeverityOf(kind);
        }


        /// <summary>
        /// The line as the collector gets it, the format chosen by the settings of the provider.
        /// </summary>
        public static String Format(Config.Config config, NotificationMessage message)
        {
            var app = (config.AppName ?? "").Trim();

            if (app == "")
                app = "ServiceManager";

            var host = (config.Hostname ?? "").Trim();

            if (host == "")
                host = Environment.MachineName;

            var max = config.MaxLength > 0 ? config.MaxLength : DefaultMaxLength;

            var priority = PriorityOf(config.Facility, message.Kind);
            var text = Text(message);

            // The collector shows when the event happened, not when the provider got around to sending
            // it. Only an event without a timestamp falls back to the current time.
            var stamp = message.Timestamp == DateTime.MinValue ? DateTime.Now : message.Timestamp;

            if (config.UseRfc5424)
                return Format5424(priority, stamp, host, app, text, max);

            return Format3164(priority, stamp, host, app, text, max);
        }


        /// <summary>
        /// The old form every collector reads. The timestamp of RFC 3164 has no year and no offset, so
        /// the collector takes the local time of the sender and the current year.
        /// </summary>
        public static String Format3164(int priority, DateTime timestamp, String hostname, String appName, String text, int maxBytes)
        {
            var month = timestamp.ToString("MMM", CultureInfo.InvariantCulture);
            var day = timestamp.Day.ToString().PadLeft(2, ' ');
            var time = timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            var stamp = month + " " + day + " " + time;

            return Fit("<" + priority + ">" + stamp + " " + hostname + " " + appName + ": ", text, maxBytes);
        }


        /// <summary>
        /// The new form: version, timestamp with the offset of the sender and room for structured data.
        /// PROCID and MSGID stay empty dashes, a collector that filters by them loses nothing.
        /// </summary>
        public static String Format5424(int priority, DateTime timestamp, String hostname, String appName, String text, int maxBytes)
        {
            var stamp = timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

            return Fit("<" + priority + ">1 " + stamp + " " + hostname + " " + appName + " - - ", text, maxBytes);
        }


        /// <summary>
        /// The message part of the line. The kind is in square brackets in front, so a line of the
        /// collector still says what happened when the structured fields are thrown away.
        /// </summary>
        public static String Text(NotificationMessage message)
        {
            var builder = new StringBuilder();

            builder.Append('[').Append(message.Kind).Append(']');

            var instance = (message.InstanceName ?? "").Trim();

            if (instance != "")
                builder.Append(' ').Append(instance);

            var service = (message.ServiceTitle ?? "").Trim();

            if (service != "")
                builder.Append(" / ").Append(service);

            var text = SingleLine(message.Text);

            if (text != "")
            {
                // The colon belongs to the name in front of the message. Without instance and service
                // the line would start with a colon behind the closing bracket.
                builder.Append(instance != "" || service != "" ? ": " : " ").Append(text);
            }

            var details = SingleLine(message.Details);

            if (details != "")
                builder.Append(" | ").Append(details);

            return builder.ToString();
        }


        /// <summary>
        /// A line break inside a message is read as the next message by the collector, so a console
        /// block becomes one line with the breaks replaced by a space.
        /// </summary>
        public static String SingleLine(String value)
        {
            if (String.IsNullOrEmpty(value))
                return "";

            var builder = new StringBuilder(value.Length);

            foreach (var c in value)
            {
                if (c == '\r' || c == '\n')
                {
                    if (builder.Length > 0 && builder[builder.Length - 1] != ' ')
                        builder.Append(' ');

                    continue;
                }

                if (Char.IsControl(c))
                    continue;

                builder.Append(c);
            }

            return builder.ToString().Trim();
        }


        /// <summary>
        /// Cuts the message part, never the header: a line without its priority brackets is dropped by
        /// the collector entirely, a cut text is still a valid entry.
        /// </summary>
        private static String Fit(String prefix, String text, int maxBytes)
        {
            if (maxBytes <= 0)
                return prefix + text;

            var used = Encoding.UTF8.GetByteCount(prefix);

            if (used >= maxBytes)
                return prefix.Substring(0, Math.Min(prefix.Length, maxBytes));

            var room = maxBytes - used;

            if (Encoding.UTF8.GetByteCount(text) <= room)
                return prefix + text;

            var builder = new StringBuilder(room);

            foreach (var c in text)
            {
                var size = Encoding.UTF8.GetByteCount(c.ToString());

                if (size > room)
                    break;

                builder.Append(c);
                room -= size;
            }

            return prefix + builder;
        }
    }
}
