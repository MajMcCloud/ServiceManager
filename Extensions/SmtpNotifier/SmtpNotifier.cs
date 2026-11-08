using ServiceManager.Extensions.SmtpNotifier.forms;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.SmtpNotifier
{
    /// <summary>
    /// The provider that reaches people without a chat service and without the internet: a mail goes
    /// to a mailbox on the mail server of the network and is still readable on the way home.
    /// </summary>
    public class SmtpNotifier : Interfaces.INotificationMessageExtension
    {
        public Guid Id => Guid.Parse("390d4bb4-9b40-4586-a355-b031e13d7e79");

        public string Name => "E-Mail (SMTP)";

        public string Description => "Sends one mail per event over a SMTP server, works in a network without internet access.";


        private Config.Config __config;

        private MailSender __sender;


        public SmtpNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);

            if (__config == null)
                __config = new Config.Config();

            __sender = new MailSender(__config);
        }


        /// <summary>
        /// The plain text contract of the older extensions. Without an event kind the mail can only be
        /// rated as information, which is what a bare line of text deserves.
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
                Console.WriteLine("SmtpNotifier: " + error);
        }


        /// <summary>
        /// Used by the settings dialog: sends one mail and answers what the server said, so the user
        /// sees the result instead of a console line of the service.
        /// </summary>
        public Task<String> Test(NotificationMessage message)
        {
            return __sender.Test(message);
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
