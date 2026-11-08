using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.ScriptNotifier.Forms;
using System;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.ScriptNotifier
{
    /// <summary>
    /// The provider for everything that does not speak http: a program of your own gets the event as
    /// json and can call an sms gateway, open a ticket or start a backup.
    /// </summary>
    public class ScriptNotifier : Interfaces.INotificationMessageExtension
    {
        public Guid Id => Guid.Parse("f285237d-627e-40e7-b43a-6ba8b6c51199");

        public string Name => "Script hook";

        public string Description => "Starts a program per event and hands it the event as json, from an sms gateway to a ticket system.";


        private Config.Config __config;

        private ScriptRunner __runner;


        public ScriptNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);

            if (__config == null)
                __config = new Config.Config();

            __runner = new ScriptRunner(__config);
        }


        /// <summary>
        /// The plain text contract of the older extensions. Without an event kind the json can only
        /// carry the text itself and the level stays info.
        /// </summary>
        public Task Send(string message)
        {
            return Send(new NotificationMessage() { Text = message, Timestamp = DateTime.Now });
        }


        public async Task Send(NotificationMessage message)
        {
            if (message == null)
                return;

            var error = await __runner.Deliver(message).ConfigureAwait(false);

            if (error != null)
                Console.WriteLine("ScriptNotifier: " + error);
        }


        /// <summary>
        /// Used by the settings dialog: runs the program once and answers what it said, so the user
        /// sees the exit code instead of a console line of the service.
        /// </summary>
        public Task<String> Test(NotificationMessage message)
        {
            return __runner.Deliver(message);
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
