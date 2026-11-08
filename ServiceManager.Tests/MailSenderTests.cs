using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.SmtpNotifier;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MailConfig = ServiceManager.Extensions.SmtpNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The sending side of the mail provider. A real server is never contacted: the pickup folder of
    /// Windows writes the mail as a file, which is both a feature of the provider and the place a test
    /// can read the same mail from.
    /// </summary>
    [TestClass]
    public class MailSenderTests
    {
        private String __folder;


        [TestInitialize]
        public void CreatePickupFolder()
        {
            __folder = Path.Combine(Path.GetTempPath(), "smtpnotifier-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(__folder);
        }


        [TestCleanup]
        public void RemovePickupFolder()
        {
            try
            {
                if (Directory.Exists(__folder))
                    Directory.Delete(__folder, true);
            }
            catch (IOException)
            {
            }
        }


        private static NotificationMessage Event(NotificationKind kind, String text = "Service stopped")
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Timestamp = new DateTime(2026, 3, 4, 5, 6, 7),
                InstanceName = "HomeLab",
                ServiceTitle = "Backup",
                Text = text,
                Details = "exit code 3"
            };
        }


        /// <summary>A sender that is complete except for the folder the mail is written to.</summary>
        private MailConfig Ready()
        {
            return new MailConfig()
            {
                SmtpServer = "localhost",
                SmtpPort = 25,
                FromAddress = "service@example.com",
                ToAddresses = "ops@example.com",
                SubjectPrefix = "ServiceManager",
                TimeoutSeconds = 5,
                ThrottleSeconds = 60,
                PickupDirectory = __folder
            };
        }


        /// <summary>The mail the provider wrote into the pickup folder, empty when nothing was written.</summary>
        private String WrittenMail()
        {
            var file = Directory.GetFiles(__folder, "*.eml").FirstOrDefault();

            return file == null ? null : File.ReadAllText(file);
        }


        /// <summary>
        /// A mail with a utf8 body is written base64 encoded, so the table of the event has to be read
        /// behind the headers of the file rather than out of the raw text.
        /// </summary>
        private String WrittenBody()
        {
            var mail = WrittenMail();

            if (mail == null)
                return null;

            var start = mail.IndexOf(Environment.NewLine + Environment.NewLine, StringComparison.Ordinal);

            if (start < 0)
                return mail;

            var body = mail.Substring(start + 2 * Environment.NewLine.Length).Where(c => !Char.IsWhiteSpace(c));

            return Encoding.UTF8.GetString(Convert.FromBase64String(new String(body.ToArray())));
        }


        /// <summary>
        /// A provider that was set up but never filled in must say so once instead of failing on every
        /// single event of the service.
        /// </summary>
        [TestMethod]
        public async Task a_sender_without_server_names_the_settings_of_the_provider()
        {
            var sender = new MailSender(new MailConfig() { FromAddress = "a@example.com", ToAddresses = "b@example.com" });

            var first = await sender.Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsTrue(first.Contains("no SMTP server is configured"), "said: " + first);

            Assert.IsNull(await sender.Deliver(Event(NotificationKind.ConsoleError)), "the same mistake is not repeated per event");
        }


        [TestMethod]
        public async Task a_sender_without_a_sender_address_names_the_settings_of_the_provider()
        {
            var sender = new MailSender(new MailConfig() { SmtpServer = "localhost", ToAddresses = "b@example.com" });

            var answer = await sender.Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsTrue(answer.Contains("no sender address is configured"), "said: " + answer);
        }


        [TestMethod]
        public async Task a_sender_without_recipients_names_the_settings_of_the_provider()
        {
            var sender = new MailSender(new MailConfig() { SmtpServer = "localhost", FromAddress = "a@example.com" });

            var answer = await sender.Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsTrue(answer.Contains("no recipient address is configured"), "said: " + answer);
        }


        /// <summary>
        /// The pickup folder is how a gateway that reads mail off the disk is fed, and it is the only
        /// way to see a mail of this provider without a server in front.
        /// </summary>
        [TestMethod]
        public async Task a_mail_is_written_to_the_pickup_folder_of_the_installation()
        {
            var sender = new MailSender(Ready());

            Assert.IsNull(await sender.Deliver(Event(NotificationKind.ServiceStop)), "the provider reports an error although the mail was written");

            var mail = WrittenMail();

            Assert.IsNotNull(mail, "nothing landed in the pickup folder");

            Assert.IsTrue(mail.Contains("Subject: [WARNING] ServiceManager - HomeLab / Backup: Service stopped"), mail);
            Assert.IsTrue(mail.Contains("To: ops@example.com"), mail);

            var body = WrittenBody();

            Assert.IsTrue(body.Contains("Event     ServiceStop"), body);
            Assert.IsTrue(body.Contains("Details   exit code 3"), body);
            Assert.IsTrue(body.Contains("Service stopped"), body);
        }


        /// <summary>
        /// The same service that stops and starts every minute would otherwise send a mail per attempt
        /// until the inbox of the operator is useless.
        /// </summary>
        [TestMethod]
        public async Task the_same_event_is_only_mailed_once_inside_the_window()
        {
            var sender = new MailSender(Ready());

            var message = Event(NotificationKind.ServiceStop);

            Assert.IsNull(await sender.Deliver(message));

            Assert.IsTrue(sender.IsThrottled(message, DateTime.UtcNow), "the second mail of the same event should be held back");

            Assert.AreEqual(1, Directory.GetFiles(__folder, "*.eml").Length, "the second event was mailed anyway");

            Assert.IsNull(await sender.Deliver(Event(NotificationKind.ServiceStop, "another service stopped")));

            Assert.AreEqual(2, Directory.GetFiles(__folder, "*.eml").Length, "a line that differs by one character is a different problem");
        }


        /// <summary>
        /// The window belongs to the settings, so a mail that is held back has to be sent again as soon
        /// as it runs out.
        /// </summary>
        [TestMethod]
        public async Task the_window_of_the_throttle_is_the_configured_one()
        {
            var sender = new MailSender(Ready());

            var message = Event(NotificationKind.ServiceStop);

            await sender.Deliver(message);

            Assert.IsTrue(sender.IsThrottled(message, DateTime.UtcNow));
            Assert.IsTrue(sender.IsThrottled(message, DateTime.UtcNow.AddSeconds(59)));
            Assert.IsFalse(sender.IsThrottled(message, DateTime.UtcNow.AddSeconds(61)), "the mail has to go out again once the window ran out");
        }


        [TestMethod]
        public async Task a_throttle_of_zero_sends_every_event()
        {
            var config = Ready();

            config.ThrottleSeconds = 0;

            var sender = new MailSender(config);

            var message = Event(NotificationKind.ServiceStop);

            await sender.Deliver(message);

            Assert.IsFalse(sender.IsThrottled(message, DateTime.UtcNow));
        }


        /// <summary>
        /// The test button of the settings dialog has to answer even a minute after the last real mail,
        /// otherwise the provider looks broken while it is only holding the second one back.
        /// </summary>
        [TestMethod]
        public async Task the_test_button_always_sends()
        {
            var sender = new MailSender(Ready());

            var message = Event(NotificationKind.ServiceStop);

            Assert.IsNull(await sender.Deliver(message));
            Assert.IsNull(await sender.Test(message));

            Assert.AreEqual(2, Directory.GetFiles(__folder, "*.eml").Length);
        }


        [TestMethod]
        public void several_addresses_are_read_from_commas_semicolons_and_lines()
        {
            var addresses = MailSender.Recipients("a@example.com, b@example.com;c@example.com\rd@example.com\r\ne@example.com");

            Assert.AreEqual(5, addresses.Count);
            Assert.AreEqual("a@example.com", addresses[0]);
            Assert.AreEqual("e@example.com", addresses[4]);
        }


        [TestMethod]
        public void an_empty_field_stays_a_list_without_addresses()
        {
            Assert.AreEqual(0, MailSender.Recipients(null).Count);
            Assert.AreEqual(0, MailSender.Recipients("  ;  , ").Count);
        }


        /// <summary>
        /// Nothing answers on that port, so the provider has to hand the reason to the console instead
        /// of throwing over the service that is already failing.
        /// </summary>
        [TestMethod]
        public async Task a_server_that_refuses_the_connection_is_reported()
        {
            var port = ClosedPort();

            var config = Ready();

            config.SmtpServer = "127.0.0.1";
            config.SmtpPort = port;
            config.PickupDirectory = "";
            config.TimeoutSeconds = 5;

            var answer = await new MailSender(config).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "a refused connection was reported as success");

            Assert.IsTrue(answer.Contains("127.0.0.1:" + port), "said: " + answer);
        }


        /// <summary>A port on the loopback adapter that nothing listens on.</summary>
        private static int ClosedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            listener.Stop();

            return port;
        }
    }
}
