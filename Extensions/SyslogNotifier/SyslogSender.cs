using ServiceManager.Extensions.Interfaces;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.SyslogNotifier
{
    /// <summary>
    /// Puts the line on the network. A separate class because the extension itself reads its settings
    /// from the folder it was loaded from, which a test cannot change.
    /// </summary>
    public class SyslogSender
    {
        private readonly Config.Config __config;

        private bool __missingHostReported;


        public SyslogSender(Config.Config config)
        {
            __config = config ?? new Config.Config();
        }


        /// <summary>
        /// Sends one line and answers null when it left the machine, otherwise the reason. Over UDP
        /// "left the machine" is all anyone can know, a collector that is down answers nothing at all.
        /// </summary>
        public async Task<String> Deliver(NotificationMessage message)
        {
            if (message == null)
                return null;

            var host = (__config.Host ?? "").Trim();

            if (host == "")
                return ReportMissing(ref __missingHostReported, "no syslog host is configured, open the settings of the Syslog provider");

            __missingHostReported = false;

            var port = __config.Port;

            if (port <= 0 || port > 65535)
                port = 514;

            var seconds = __config.TimeoutSeconds;

            if (seconds <= 0)
                seconds = 5;

            var bytes = Encoding.UTF8.GetBytes(SyslogMessage.Format(__config, message));

            try
            {
                if (__config.UseTcp)
                    await SendTcp(bytes, host, port, seconds).ConfigureAwait(false);
                else
                    await SendUdp(bytes, host, port).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ex.Message + " (" + host + ":" + port + ")";
            }

            return null;
        }


        private async Task SendUdp(byte[] bytes, String host, int port)
        {
            using (var client = new UdpClient())
            {
                await client.SendAsync(bytes, bytes.Length, host, port).ConfigureAwait(false);
            }
        }


        private async Task SendTcp(byte[] bytes, String host, int port, int seconds)
        {
            using (var client = new TcpClient())
            {
                var connect = client.ConnectAsync(host, port);

                Observe(connect);

                if (await Task.WhenAny(connect, Task.Delay(seconds * 1000)).ConfigureAwait(false) != connect)
                    throw new TimeoutException("the syslog server \"" + host + "\" did not answer within " + seconds + " seconds");

                await connect.ConfigureAwait(false);

                using (var stream = client.GetStream())
                {
                    // A collector needs to know where a message ends. The line break is the framing
                    // almost every collector reads, the octet counting of RFC 6587 hardly any of them.
                    var payload = new byte[bytes.Length + 1];

                    Buffer.BlockCopy(bytes, 0, payload, 0, bytes.Length);

                    payload[bytes.Length] = (byte)'\n';

                    var write = stream.WriteAsync(payload, 0, payload.Length);

                    Observe(write);

                    if (await Task.WhenAny(write, Task.Delay(seconds * 1000)).ConfigureAwait(false) != write)
                        throw new TimeoutException("the syslog server \"" + host + "\" did not read the message within " + seconds + " seconds");

                    await write.ConfigureAwait(false);

                    await stream.FlushAsync().ConfigureAwait(false);
                }
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
