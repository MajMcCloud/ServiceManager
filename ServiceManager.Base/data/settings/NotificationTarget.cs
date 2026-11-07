using ServiceManager.Extensions.Interfaces;
using System;

namespace ServiceManager.Base.data.settings
{
    /// <summary>
    /// One notification provider plus the events it should react to. Every provider gets its own
    /// selection, so a console output can go to the event log while the errors of the same service
    /// go to a chat provider.
    /// </summary>
    public class NotificationTarget
    {
        public Guid PluginId { get; set; }

        public bool OnStart { get; set; } = true;

        public bool OnStop { get; set; } = true;

        public bool OnRestart { get; set; } = true;

        public bool OnConsoleOutput { get; set; } = true;

        public bool OnConsoleError { get; set; } = true;

        public bool OnServerStart { get; set; } = true;

        public bool OnServerShutdown { get; set; } = true;


        public bool IsEnabled(NotificationKind kind)
        {
            switch (kind)
            {
                case NotificationKind.ServerStart:
                    return OnServerStart;
                case NotificationKind.ServerShutdown:
                    return OnServerShutdown;
                case NotificationKind.ServiceStart:
                    return OnStart;
                case NotificationKind.ServiceStop:
                    return OnStop;
                case NotificationKind.ServiceRestart:
                    return OnRestart;
                case NotificationKind.ConsoleOutput:
                    return OnConsoleOutput;
                case NotificationKind.ConsoleError:
                    return OnConsoleError;
                default:
                    return false;
            }
        }
    }
}
