using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Globalization;

namespace ServiceManager.Extensions.ScriptNotifier
{
    /// <summary>
    /// The event as one json object. The field names are the ones of the standard body of the webhook
    /// provider, so a script that reads one of them reads the other without a second mapping.
    /// </summary>
    public static class ScriptPayload
    {
        /// <summary>
        /// Same rating as the event log, the webhook and the mail provider, so a script does not have
        /// to know which provider handed it the event.
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


        public static String ToJson(NotificationMessage message)
        {
            var body = new JObject
            {
                ["instance"] = message.InstanceName ?? "",
                ["event"] = message.Kind.ToString(),
                ["level"] = Level(message.Kind),
                ["service"] = message.ServiceTitle ?? "",
                ["time"] = message.Timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                ["text"] = message.Text ?? "",
                ["details"] = message.Details ?? ""
            };

            return body.ToString(Formatting.None);
        }
    }
}
