using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.SmtpNotifier
{
    /// <summary>
    /// Talks to the mail server. A separate class because the extension itself reads its settings
    /// from the folder it was loaded from, which a test cannot change.
    /// </summary>
    public class MailSender
    {
        private readonly Config.Config __config;

        /// <summary>Last successful send per kind and text, the guard against a mail storm.</summary>
        private readonly Dictionary<String, DateTime> __lastSend = new Dictionary<String, DateTime>();

        private bool __missingServerReported;

        private bool __missingSenderReported;

        private bool __missingRecipientReported;


        public MailSender(Config.Config config)
        {
            __config = config ?? new Config.Config();
        }


        /// <summary>
        /// Sends one mail and answers null when the server accepted it, otherwise the reason. The
        /// extension writes that reason to the console, the settings dialog shows it to the user.
        /// </summary>
        public Task<String> Deliver(NotificationMessage message)
        {
            return Deliver(message, false);
        }


        /// <summary>
        /// The settings dialog asks for a mail that always goes out, otherwise the second click on the
        /// test button within the throttle window would look like a broken provider.
        /// </summary>
        public Task<String> Test(NotificationMessage message)
        {
            return Deliver(message, true);
        }


        private async Task<String> Deliver(NotificationMessage message, bool ignoreThrottle)
        {
            if (message == null)
                return null;

            var server = (__config.SmtpServer ?? "").Trim();

            if (server == "")
                return ReportMissing(ref __missingServerReported, "no SMTP server is configured, open the settings of the E-Mail provider");

            __missingServerReported = false;

            var from = (__config.FromAddress ?? "").Trim();

            if (from == "")
                return ReportMissing(ref __missingSenderReported, "no sender address is configured, open the settings of the E-Mail provider");

            __missingSenderReported = false;

            var recipients = Recipients(__config.ToAddresses);

            if (recipients.Count == 0)
                return ReportMissing(ref __missingRecipientReported, "no recipient address is configured, open the settings of the E-Mail provider");

            __missingRecipientReported = false;

            if (!ignoreThrottle && IsThrottled(message, DateTime.UtcNow))
                return null;

            var port = __config.SmtpPort;

            if (port <= 0 || port > 65535)
                port = 25;

            try
            {
                await Send(server, port, from, recipients, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ex.Message + " (" + server + ":" + port + ")";
            }

            Remember(message, DateTime.UtcNow);

            return null;
        }


        private async Task Send(String server, int port, String from, List<String> recipients, NotificationMessage message)
        {
            var seconds = __config.TimeoutSeconds;

            if (seconds <= 0)
                seconds = 15;

            using (var mail = new MailMessage())
            using (var client = new SmtpClient(server, port))
            {
                mail.From = new MailAddress(from);

                foreach (var address in recipients)
                    mail.To.Add(new MailAddress(address));

                mail.Subject = MailContent.RenderSubject(__config, message);
                mail.Body = MailContent.RenderBody(message);
                mail.IsBodyHtml = false;

                // A console line is utf8 already, and the default encoding of SmtpClient would turn a
                // umlaut into a question mark for a server that does not announce utf8 support.
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                client.EnableSsl = __config.UseStartTls;

                var user = (__config.Username ?? "").Trim();

                if (user != "")
                    client.Credentials = new NetworkCredential(user, __config.Password ?? "");

                var pickup = (__config.PickupDirectory ?? "").Trim();

                if (pickup != "")
                {
                    // No network at all: the mail is written as a file. Used for a gateway that reads
                    // that folder, and it is what the tests of this provider read.
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = pickup;
                }
                else
                {
                    client.Timeout = seconds * 1000;
                }

                var send = client.SendMailAsync(mail);

                Observe(send);

                if (await Task.WhenAny(send, Task.Delay(seconds * 1000)).ConfigureAwait(false) != send)
                    throw new TimeoutException("the SMTP server \"" + server + "\" did not answer within " + seconds + " seconds");

                await send.ConfigureAwait(false);
            }
        }


        /// <summary>
        /// A send that is given up on still faults later, and an unobserved task would surface as noise
        /// of the finalizer instead of a message of the provider.
        /// </summary>
        private static void Observe(Task task)
        {
            task.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// True when the very same event was sent a moment ago. Only identical text is suppressed, a
        /// line that differs by one character is a different problem and still goes out.
        /// </summary>
        internal bool IsThrottled(NotificationMessage message, DateTime now)
        {
            if (__config.ThrottleSeconds <= 0 || message == null)
                return false;

            var key = ThrottleKey(message);

            lock (__lastSend)
            {
                DateTime last;

                if (!__lastSend.TryGetValue(key, out last))
                    return false;

                return (now - last).TotalSeconds < __config.ThrottleSeconds;
            }
        }


        private void Remember(NotificationMessage message, DateTime now)
        {
            if (__config.ThrottleSeconds <= 0)
                return;

            var key = ThrottleKey(message);

            lock (__lastSend)
            {
                __lastSend[key] = now;

                // The service runs for weeks. Everything older than the window can never matter again.
                if (__lastSend.Count > 200)
                {
                    var cutoff = now.AddSeconds(-Math.Max(1, __config.ThrottleSeconds));
                    var stale = new List<String>();

                    foreach (var pair in __lastSend)
                    {
                        if (pair.Value < cutoff)
                            stale.Add(pair.Key);
                    }

                    foreach (var staleKey in stale)
                        __lastSend.Remove(staleKey);
                }
            }
        }


        private static String ThrottleKey(NotificationMessage message)
        {
            return message.Kind + "\n" + (message.Text ?? "");
        }


        /// <summary>
        /// Comma, semicolon or one address per line: all three are how people write several addresses
        /// into one field of a settings file.
        /// </summary>
        internal static List<String> Recipients(String value)
        {
            var list = new List<String>();

            if (String.IsNullOrEmpty(value))
                return list;

            foreach (var part in value.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var address = part.Trim();

                if (address != "")
                    list.Add(address);
            }

            return list;
        }


        /// <summary>
        /// A provider without a target is a configuration mistake, and one line per event would bury
        /// the console in it. The message appears again as soon as something was sent successfully.
        /// </summary>
        private static String ReportMissing(ref bool reported, String message)
        {
            if (reported)
                return null;

            reported = true;

            return message;
        }
    }
}
