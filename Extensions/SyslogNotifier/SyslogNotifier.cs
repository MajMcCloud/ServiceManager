using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.SyslogNotifier.forms;
using System;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.SyslogNotifier
{
    /// <summary>
    /// The provider for an installation that already collects logs: a syslog line needs no account, no
    /// token and no certificate, and it lands in the same place as the lines of every other device.
    /// </summary>
    public class SyslogNotifier : Interfaces.INotificationMessageExtension
    {
        public Guid Id => Guid.Parse("5aa8d564-45ae-456d-9425-aaf5f69c2e9c");

        public string Name => "Syslog";

        public string Description => "Sends every event as a syslog line to a collector, over UDP or TCP and without an account.";


        private Config.Config __config;

        private SyslogSender __sender;


        public SyslogNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);

            if (__config == null)
                __config = new Config.Config();

            __sender = new SyslogSender(__config);
        }


        /// <summary>
        /// The plain text contract of the older extensions. Without an event kind the line can only be
        /// rated as informational, which is what a bare line of text deserves.
        /// </summary>
        public Task Send(string message)
        {
            return Send(new NotificationMessage() { Text = message, Timestamp = DateTime.Now });
        }


        public async Task Send(NotificationMessage message)
        {
            if (message == null)
                return;

            var error = await __sender.Deliver(message).ConfigureAwait(false);

            if (error != null)
                Console.WriteLine("SyslogNotifier: " + error);
        }


        /// <summary>
        /// Used by the settings dialog: sends one line and answers what the network said, so the user
        /// sees the result instead of a console line of the service.
        /// </summary>
        public Task<String> Test(NotificationMessage message)
        {
            return __sender.Deliver(message);
        }


        public void Configure()
        {
            using (frmConfig frm = new frmConfig())
            {
                frm.ShowDialog();
            }
        }
    }
}
