using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.WebhookNotifier.forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.WebhookNotifier
{
    /// <summary>
    /// Posts every event to a HTTP endpoint. The body is written by the user, so the same provider
    /// serves a Slack incoming webhook, a Discord webhook, an n8n or Node-RED flow and a plain
    /// endpoint of our own.
    /// </summary>
    public class WebhookNotifier : Interfaces.INotificationMessageExtension
    {
        public Guid Id => Guid.Parse("65c574ee-cc0b-4f15-89c4-7efdb052218a");

        public string Name => "Webhook";

        public string Description => "Posts your service events as JSON, or in any body you write, to a HTTP endpoint.";


        private Config.Config __config;

        /// <summary>Stops an unconfigured provider from writing one console line per event.</summary>
        private static bool __missingUrlReported;

        /// <summary>How much of the answer is quoted back when the endpoint complains.</summary>
        private const int AnswerLimit = 300;


        public WebhookNotifier()
        {
            __config = Config.Config.load(Config.Config.FilePath);

            if (__config == null)
                __config = new Config.Config();
        }


        /// <summary>
        /// The plain text contract of the older extensions. Without an event kind the body can only
        /// carry the text itself.
        /// </summary>
        public Task Send(string message)
        {
            return Send(new NotificationMessage() { Text = message, Timestamp = DateTime.Now });
        }


        public Task Send(NotificationMessage message)
        {
            if (message == null)
                return Task.CompletedTask;

            return Report(message);
        }


        /// <summary>
        /// A provider that fails must not take the others with it, and it should say why once instead
        /// of throwing the event away in silence.
        /// </summary>
        private async Task Report(NotificationMessage message)
        {
            var error = await Deliver(message).ConfigureAwait(false);

            if (error != null)
                Console.WriteLine("WebhookNotifier: " + error);
        }


        /// <summary>
        /// Used by the settings dialog: sends one message and answers what the endpoint said, so the
        /// user sees the result instead of a console line of the service.
        /// </summary>
        public Task<String> Test(NotificationMessage message)
        {
            return Deliver(message);
        }


        /// <summary>
        /// Posts the message as often as configured. Returns null when the endpoint accepted it,
        /// otherwise the reason of the last attempt.
        /// </summary>
        private async Task<String> Deliver(NotificationMessage message)
        {
            var url = (__config.Url ?? "").Trim();

            if (url == "")
            {
                if (__missingUrlReported)
                    return null;

                __missingUrlReported = true;

                return "no target url is configured, open the settings of the Webhook provider";
            }

            __missingUrlReported = false;

            var contentType = (__config.ContentType ?? "").Trim();

            if (contentType == "")
                contentType = "application/json";

            var body = WebhookPayload.Render(__config.BodyTemplate, message, IsJson(contentType));

            var attempts = 1 + Math.Max(0, __config.Retries);
            var last = "the endpoint did not answer";

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    var answer = await Post(url, body, contentType).ConfigureAwait(false);

                    if (answer.Key >= 200 && answer.Key < 300)
                        return null;

                    last = "HTTP " + answer.Key + " from " + url;

                    if (!String.IsNullOrEmpty(answer.Value))
                        last += ": " + Cut(answer.Value);
                }
                catch (Exception ex)
                {
                    last = ex.Message + " (" + url + ")";
                }

                if (attempt >= attempts)
                    return last;

                await Task.Delay(1000 * attempt).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// One request, one answer. HttpWebRequest rather than HttpClient: .NET Framework only offers
        /// the certificate callback per request here, while the one of ServicePointManager is process
        /// wide and would also switch off the checks of every other provider.
        /// </summary>
        private async Task<KeyValuePair<Int32, String>> Post(String url, String body, String contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = Method();
            request.ContentType = contentType;

            var seconds = __config.TimeoutSeconds <= 0 ? 10 : __config.TimeoutSeconds;

            request.Timeout = seconds * 1000;
            request.ReadWriteTimeout = seconds * 1000;

            if (__config.IgnoreCertificateErrors)
                request.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            ApplyHeaders(request);

            var bytes = Encoding.UTF8.GetBytes(body ?? "");

            request.ContentLength = bytes.Length;

            // Timeout and ReadWriteTimeout bound the synchronous path only. The asynchronous one of
            // .NET Framework runs on regardless, so a timer aborts the request once the seconds the
            // user configured are over. Without it a silent endpoint keeps the event waiting forever.
            using (var deadline = new CancellationTokenSource(seconds * 1000))
            using (deadline.Token.Register(() =>
            {
                try { request.Abort(); }
                catch { } // an abort of a request that finished in the meantime means nothing
            }))
            {
                try
                {
                    using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    }

                    try
                    {
                        using (var answer = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                        {
                            return new KeyValuePair<Int32, String>((Int32)answer.StatusCode, Read(answer));
                        }
                    }
                    catch (WebException ex)
                    {
                        var answer = ex.Response as HttpWebResponse;

                        if (answer == null)
                            throw; // nothing answered at all: timeout, unknown host, connection refused

                        using (answer)
                        {
                            return new KeyValuePair<Int32, String>((Int32)answer.StatusCode, Read(answer));
                        }
                    }
                }
                catch (Exception ex) when (deadline.IsCancellationRequested)
                {
                    // The abort of our own timer is the one thing the user wants to hear about by name.
                    throw new TimeoutException("no answer within " + seconds + " seconds", ex);
                }
            }
        }


        private String Method()
        {
            var method = (__config.Method ?? "").Trim().ToUpperInvariant();

            return method == "" ? "POST" : method;
        }


        /// <summary>
        /// The headers the user typed. HttpWebRequest keeps a few names for itself, those belong in
        /// the property made for them instead of the header table.
        /// </summary>
        private void ApplyHeaders(HttpWebRequest request)
        {
            foreach (var line in HeaderLines())
            {
                var separator = line.IndexOf(':');

                if (separator <= 0)
                    continue;

                var name = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim();

                // The content type of the settings is the one the body is sent with.
                if (String.Equals(name, "content-type", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    request.Headers[name] = value;
                }
                catch (ArgumentException)
                {
                    switch (name.ToLowerInvariant())
                    {
                        case "accept":
                            request.Accept = value;
                            break;

                        case "user-agent":
                            request.UserAgent = value;
                            break;

                        case "referer":
                            request.Referer = value;
                            break;

                        case "connection":
                            request.Connection = value;
                            break;

                        default:
                            throw new InvalidOperationException("The header \"" + name + "\" cannot be sent this way.");
                    }
                }
            }
        }


        private static String Read(HttpWebResponse answer)
        {
            try
            {
                using (var reader = new StreamReader(answer.GetResponseStream(), Encoding.UTF8))
                {
                    // A receiver that answers with a whole page must not fill the console.
                    var buffer = new char[AnswerLimit + 1];
                    var read = reader.ReadBlock(buffer, 0, buffer.Length);

                    return new String(buffer, 0, read);
                }
            }
            catch (Exception ex)
            {
                return "the answer could not be read: " + ex.Message;
            }
        }


        private IEnumerable<String> HeaderLines()
        {
            if (__config.Headers == null)
                return Enumerable.Empty<String>();

            return __config.Headers.Where(a => !String.IsNullOrWhiteSpace(a));
        }


        private static bool IsJson(String contentType)
        {
            return contentType.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private static String Cut(String value)
        {
            value = value.Replace("\r", " ").Replace("\n", " ").Trim();

            if (value.Length <= AnswerLimit)
                return value;

            return value.Substring(0, AnswerLimit) + "...";
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
