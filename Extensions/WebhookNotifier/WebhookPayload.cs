using ServiceManager.Extensions.Interfaces;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ServiceManager.Extensions.WebhookNotifier
{
    /// <summary>
    /// Fills the body template of the settings file. Deliberately free of network and form code so
    /// the rendering can be checked without an endpoint in front of it.
    /// </summary>
    public static class WebhookPayload
    {
        /// <summary>
        /// Only a brace followed by a name is a placeholder. That matters: a JSON body starts with a
        /// brace as well, and scanning for the next closing brace would swallow the real placeholders
        /// of "{ \"text\": \"{text}\" }".
        /// </summary>
        private static readonly Regex Placeholder = new Regex("\\{\\s*([A-Za-z][A-Za-z0-9_]*)\\s*\\}", RegexOptions.Compiled);


        /// <summary>
        /// The body to send. Values are escaped for JSON when the receiver is told the body is JSON,
        /// a plain text or form receiver gets them as they are.
        /// </summary>
        public static String Render(String template, NotificationMessage message, bool json)
        {
            if (String.IsNullOrEmpty(template))
                template = Config.Config.DefaultBody;

            if (message == null)
                return template;

            return Placeholder.Replace(template, (match) =>
            {
                var value = Value(message, match.Groups[1].Value);

                // An unknown placeholder stays in the body, so a template written for another tool
                // does not silently lose its own variables.
                if (value == null)
                    return match.Value;

                return json ? EscapeJson(value) : value;
            });
        }


        /// <summary>
        /// The value of one placeholder, null when the name is unknown. Names are compared without
        /// case, so {Text} and {text} both work.
        /// </summary>
        private static String Value(NotificationMessage message, String name)
        {
            switch (name.ToLowerInvariant())
            {
                case "text":
                    return message.Text ?? "";

                case "details":
                    return message.Details ?? "";

                case "kind":
                    return message.Kind.ToString();

                case "level":
                    return Level(message.Kind);

                case "service":
                    return message.ServiceTitle ?? "";

                case "instance":
                    return message.InstanceName ?? "";

                case "timestamp":
                    return message.Timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

                default:
                    return null;
            }
        }


        /// <summary>
        /// The level a receiver can colour its message by. Same order as the event log provider, so
        /// both providers rate an event the same way.
        /// </summary>
        public static String Level(NotificationKind kind)
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
        /// The value as it may stand inside a JSON string. Without this a console line containing a
        /// quote would break the body and the receiver would answer with a parse error.
        /// </summary>
        public static String EscapeJson(String value)
        {
            if (String.IsNullOrEmpty(value))
                return "";

            var builder = new StringBuilder(value.Length + 16);

            foreach (var c in value)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;

                    case '\\':
                        builder.Append("\\\\");
                        break;

                    case '\b':
                        builder.Append("\\b");
                        break;

                    case '\f':
                        builder.Append("\\f");
                        break;

                    case '\n':
                        builder.Append("\\n");
                        break;

                    case '\r':
                        builder.Append("\\r");
                        break;

                    case '\t':
                        builder.Append("\\t");
                        break;

                    default:
                        if (c < ' ')
                            builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
