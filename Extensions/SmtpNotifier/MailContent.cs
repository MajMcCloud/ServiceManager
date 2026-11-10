using ServiceManager.Extensions.Interfaces;
using System;
using System.Globalization;
using System.Text;

namespace ServiceManager.Extensions.SmtpNotifier
{
    /// <summary>
    /// What stands in the subject and the body of a mail. Kept apart from the sending on purpose: a
    /// test reads the mail of an event without an SMTP server in the way, and the settings dialog can
    /// show the subject before anything was sent.
    /// </summary>
    public static class MailContent
    {
        /// <summary>Mail clients wrap a subject far earlier than the protocol does, longer text is lost.</summary>
        public const int SubjectLimit = 200;

        /// <summary>How many characters of a console line still make sense in a subject.</summary>
        private const int SubjectTextLimit = 120;


        /// <summary>
        /// Same rating as the event log and the webhook provider, so a mail rule, a filter in the
        /// event viewer and a webhook receiver agree on what counts as an error.
        /// </summary>
        public static String Severity(NotificationKind kind)
        {
            switch (kind)
            {
                case NotificationKind.ConsoleError:
                    return "error";

                case NotificationKind.ServiceStop:
                case NotificationKind.ServiceRestart:
                case NotificationKind.DependencyWait:
                    return "warning";

                default:
                    return "info";
            }
        }


        /// <summary>
        /// One line, so a console line or a service name can neither break the subject nor add a
        /// second header to the mail.
        /// </summary>
        public static String RenderSubject(Config.Config config, NotificationMessage message)
        {
            var prefix = (config.SubjectPrefix ?? "").Trim();

            if (prefix == "")
                prefix = "ServiceManager";

            var instance = (message.InstanceName ?? "").Trim();

            if (instance == "")
                instance = "ServiceManager";

            var subject = "[" + Severity(message.Kind).ToUpperInvariant() + "] " + prefix + " - " + instance;

            var service = OneLine(message.ServiceTitle);

            if (service != "")
                subject += " / " + service;

            var text = OneLine(message.Text);

            if (text != "")
                subject += ": " + Cut(text, SubjectTextLimit);

            return Cut(subject, SubjectLimit);
        }


        /// <summary>
        /// The fields of the event as a small table, the text itself below it. Plain text because a
        /// console line is not html and a mail client would hide everything it considers a tag.
        /// </summary>
        public static String RenderBody(NotificationMessage message)
        {
            var builder = new StringBuilder();

            builder.AppendLine("ServiceManager notification");
            builder.AppendLine();

            Append(builder, "Event", message.Kind.ToString());
            Append(builder, "Severity", Severity(message.Kind));
            Append(builder, "Time", message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            Append(builder, "Instance", (message.InstanceName ?? "").Trim());
            Append(builder, "Service", (message.ServiceTitle ?? "").Trim());
            Append(builder, "Details", (message.Details ?? "").Trim());

            builder.AppendLine();
            builder.AppendLine(message.Text ?? "");

            return builder.ToString();
        }


        private static void Append(StringBuilder builder, String label, String value)
        {
            if (String.IsNullOrEmpty(value))
                return;

            builder.Append(label.PadRight(10, ' '));
            builder.AppendLine(value);
        }


        /// <summary>
        /// Everything up to the first line break, without the control characters behind it. A carriage
        /// return inside a header would let a service name add headers to the mail.
        /// </summary>
        private static String OneLine(String value)
        {
            if (String.IsNullOrEmpty(value))
                return "";

            var builder = new StringBuilder(value.Length);

            foreach (var c in value)
            {
                if (c == '\r' || c == '\n')
                    break;

                if (Char.IsControl(c))
                    continue;

                builder.Append(c);
            }

            return builder.ToString().Trim();
        }


        private static String Cut(String value, int max)
        {
            if (value == null || value.Length <= max)
                return value ?? "";

            return value.Substring(0, max);
        }
    }
}
