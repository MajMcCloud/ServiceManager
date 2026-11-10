using ServiceManager.Extensions.EventLogNotifier.Forms;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManager.Extensions.EventLogNotifier
{
    /// <summary>
    /// The provider without any network: every event lands in the Windows Event Log and, when the
    /// user asks for it, as a balloon tip on the desktop. It is the only provider that still works
    /// while the machine has no internet access.
    /// </summary>
    public class EventLogNotifier : Interfaces.INotificationMessageExtension
    {
        public Guid Id => Guid.Parse("f43139b7-7aef-4cf3-9731-b0de76e21771");

        public string Name => "Windows Event Log";

        public string Description => "Writes your service events into the Windows Event Log and can show a balloon tip on the desktop.";


        private Config.Config __config;

        /// <summary>Balloon tips are limited by the shell, longer text is simply cut off.</summary>
        private const int BalloonTextLimit = 1024;

        private const int BalloonTitleLimit = 127;


        public EventLogNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);

            if (__config == null)
                __config = new Config.Config();
        }


        /// <summary>
        /// The plain text contract of the older extensions. Without an event kind the entry can only
        /// be logged as information.
        /// </summary>
        public Task Send(string message)
        {
            return Send(new NotificationMessage() { Text = message, Timestamp = DateTime.Now });
        }


        public Task Send(NotificationMessage message)
        {
            if (message == null)
                return Task.CompletedTask;

            if (!__config.WriteToEventLog && !__config.ShowBalloonTip)
                return Task.CompletedTask;

            var text = message.Text ?? "";

            if (__config.WriteToEventLog)
            {
                try
                {
                    WriteEntry(message, text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EventLogNotifier could not write to \"{__config.EventLog}\": {ex.Message}");
                }
            }

            if (__config.ShowBalloonTip)
            {
                try
                {
                    ShowBalloon(message, text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EventLogNotifier could not show a balloon tip: {ex.Message}");
                }
            }

            return Task.CompletedTask;
        }


        private void WriteEntry(NotificationMessage message, string text)
        {
            var log = String.IsNullOrEmpty(__config.EventLog) ? "Application" : __config.EventLog;
            var source = String.IsNullOrEmpty(__config.Source) ? "ServiceManager" : __config.Source;

            if (!EventLog.SourceExists(source))
            {
                // Registering a source writes into the registry, so it needs administrator rights.
                // The service usually has them, the management tool only when it was started elevated.
                EventLog.CreateEventSource(source, log);
            }

            var kind = message.Kind;

            EventLog.WriteEntry(source, text, ToEntryType(kind), EventId(kind), (short)(int)kind);
        }


        /// <summary>
        /// A stopped or restarted service is worth a warning, everything that arrives over the error
        /// stream is an error. The rest is information.
        /// </summary>
        private static EventLogEntryType ToEntryType(NotificationKind kind)
        {
            switch (kind)
            {
                case NotificationKind.ConsoleError:
                    return EventLogEntryType.Error;

                case NotificationKind.ServiceStop:
                case NotificationKind.ServiceRestart:
                case NotificationKind.DependencyWait:
                    return EventLogEntryType.Warning;

                default:
                    return EventLogEntryType.Information;
            }
        }


        /// <summary>
        /// One id per event kind, so the event viewer can filter on e.g. every console error of every
        /// ServiceManager without knowing the ids of the other kinds.
        /// </summary>
        private int EventId(NotificationKind kind)
        {
            var baseId = __config.EventId;

            if (baseId <= 0)
                baseId = 1000;

            return baseId + (int)kind;
        }


        private void ShowBalloon(NotificationMessage message, string text)
        {
            var seconds = __config.BalloonSeconds;

            if (seconds <= 0)
                seconds = 5;

            var title = String.IsNullOrEmpty(message.InstanceName) ? "ServiceManager" : message.InstanceName;

            var icon = ToToolTipIcon(ToEntryType(message.Kind));

            var thread = new Thread(() =>
            {
                using (var tray = new NotifyIcon())
                {
                    tray.Icon = SystemIcons.Application;
                    tray.Visible = true;

                    tray.ShowBalloonTip(seconds * 1000, Trim(title, BalloonTitleLimit), Trim(text, BalloonTextLimit), icon);

                    Thread.Sleep(seconds * 1000 + 500);

                    tray.Visible = false;
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }


        private static ToolTipIcon ToToolTipIcon(EventLogEntryType type)
        {
            switch (type)
            {
                case EventLogEntryType.Error:
                    return ToolTipIcon.Error;
                case EventLogEntryType.Warning:
                    return ToolTipIcon.Warning;
                default:
                    return ToolTipIcon.Info;
            }
        }


        private static string Trim(String value, int max)
        {
            if (value == null || value.Length <= max)
                return value;

            return value.Substring(0, max);
        }


        public void Configure()
        {
            using (FrmConfig frm = new FrmConfig())
            {
                frm.ShowDialog();
            }
        }
    }
}
