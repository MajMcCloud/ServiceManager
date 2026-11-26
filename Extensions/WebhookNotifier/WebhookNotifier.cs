using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.WebhookNotifier.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
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
    public class WebhookNotifier : Interfaces.INotificationMessageExtension, Interfaces.INotificationSettingsCheck
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
                SMLog.Warning("WebhookNotifier: " + error);
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
        /// What the settings dialog of this provider still shows as empty. The names are the labels of
        /// that dialog, so a warning beside the provider points at the field to fill in.
        /// </summary>
        public static IList<String> MissingSettings(Config.Config config)
        {
            var missing = new List<String>();

            if (config == null || (config.Url ?? "").Trim() == "")
                missing.Add("URL");

            return missing;
        }


        public IList<String> MissingSettings()
        {
            return MissingSettings(__config);
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
                    last = Reason(ex) + " (" + url + ")";
                }

                if (attempt >= attempts)
                    return last;

                await Task.Delay(1000 * attempt).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// One request, one answer. The answer is the status code and the first characters of the body,
        /// which is what the notification text is built from.
        /// </summary>
        private async Task<KeyValuePair<Int32, String>> Post(String url, String body, String contentType)
        {
            var seconds = __config.TimeoutSeconds <= 0 ? 10 : __config.TimeoutSeconds;

            // The handler is built for this one request: the certificate decision belongs to this
            // provider, it must neither reach into the whole process nor outlive the settings dialog.
            var handler = new HttpClientHandler();

            if (__config.IgnoreCertificateErrors)
            {
                handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => true;
            }

            // A silent endpoint must not keep the event waiting forever, so the seconds the user
            // configured bound the request and the answer of the endpoint.
            using (var deadline = new CancellationTokenSource(seconds * 1000))
            using (var client = new HttpClient(handler, true))
            using (var request = new HttpRequestMessage(new HttpMethod(Method()), url))
            {
                request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body ?? ""));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                ApplyHeaders(request);

                try
                {
                    using (var answer = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false))
                    {
                        var read = await Read(answer, deadline.Token).ConfigureAwait(false);

                        return new KeyValuePair<Int32, String>((Int32)answer.StatusCode, read);
                    }
                }
                catch (Exception ex) when (deadline.IsCancellationRequested)
                {
                    // The expiry of our own timer is the one thing the user wants to hear about by name.
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
        /// The headers the user typed. A name HttpClient keeps for itself is refused, and so is a body
        /// header on the request line: both answer with one of our own messages rather than leaving a
        /// HTTP one to the caller.
        /// </summary>
        private void ApplyHeaders(HttpRequestMessage request)
        {
            foreach (var line in HeaderLines())
            {
                var separator = line.IndexOf(':');

                if (separator <= 0)
                {
                    continue;
                }

                var name = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim();

                // The content type of the settings is the one the body is sent with.
                if (String.Equals(name, "content-type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // A body header belongs to the content of the request, everything else to the request.
                HttpHeaders headers = name.StartsWith("content-", StringComparison.OrdinalIgnoreCase)
                    ? request.Content.Headers
                    : request.Headers;

                if (!headers.TryAddWithoutValidation(name, value))
                {
                    throw new InvalidOperationException("The header \"" + name + "\" cannot be sent this way.");
                }
            }
        }


        /// <summary>
        /// The first characters of the answer. A receiver that answers with a whole page must not fill
        /// the console, and one that sends its headers and then stalls the body runs into the same
        /// deadline as the request itself.
        /// </summary>
        private static async Task<String> Read(HttpResponseMessage answer, CancellationToken deadline)
        {
            try
            {
                using (var stream = await answer.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var bytes = new Byte[AnswerLimit * 3 + 1];
                    var total = 0;

                    while (total < bytes.Length)
                    {
                        var read = await stream.ReadAsync(bytes.AsMemory(total, bytes.Length - total), deadline).ConfigureAwait(false);

                        if (read == 0)
                        {
                            break;
                        }

                        total += read;
                    }

                    return Encoding.UTF8.GetString(bytes.AsSpan(0, total));
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return "the answer could not be read: " + ex.Message;
            }
        }


        /// <summary>
        /// What the user reads when the request never arrived. HttpClient passes the wording of the
        /// socket layer on, and that one arrives in the language of the machine, so the ways an
        /// endpoint actually fails are named here instead.
        /// </summary>
        private static String Reason(Exception ex)
        {
            var request = ex as HttpRequestException;

            if (request == null)
            {
                return ex.Message;
            }

            var socket = request.InnerException as SocketException;

            if (socket == null)
            {
                // The HTTP stack explains the rest on its own: an answer that stops, a certificate
                // that is not trusted.
                if (request.InnerException == null)
                {
                    return request.Message;
                }

                return request.InnerException.Message;
            }

            switch (socket.SocketErrorCode)
            {
                case SocketError.HostNotFound:
                case SocketError.HostUnreachable:
                case SocketError.TryAgain:
                    return "the endpoint name could not be resolved";

                case SocketError.ConnectionRefused:
                    return "the endpoint refused the connection";

                case SocketError.NetworkDown:
                case SocketError.NetworkUnreachable:
                    return "the endpoint is not reachable from this machine";

                default:
                    return "the endpoint could not be reached: " + socket.SocketErrorCode;
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
            using (FrmConfig frm = new FrmConfig())
            {
                frm.ShowDialog();
            }
        }
    }
}
