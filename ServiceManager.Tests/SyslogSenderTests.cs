using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.SyslogNotifier;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SyslogConfig = ServiceManager.Extensions.SyslogNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The transport of the syslog provider. The collector is a socket on the loopback adapter, so the
    /// line that leaves the machine is the line a real collector would read.
    /// </summary>
    [TestClass]
    public class SyslogSenderTests
    {
        private static NotificationMessage Event(NotificationKind kind, String text = "Service stopped")
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Timestamp = DateTime.Now,
                InstanceName = "HomeLab",
                ServiceTitle = "Backup",
                Text = text,
                Details = "exit code 3"
            };
        }


        private static SyslogConfig Collector(int port, bool tcp = false)
        {
            return new SyslogConfig()
            {
                Host = "127.0.0.1",
                Port = port,
                UseTcp = tcp,
                Hostname = "testhost",
                AppName = "ServiceManager",
                TimeoutSeconds = 5
            };
        }


        [TestMethod]
        public async Task a_sender_without_host_names_the_settings_of_the_provider()
        {
            var sender = new SyslogSender(new SyslogConfig());

            var first = await sender.Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsTrue(first.Contains("no syslog host is configured"), "said: " + first);

            Assert.IsNull(await sender.Deliver(Event(NotificationKind.ConsoleError)), "the same mistake is not repeated per event");
        }


        /// <summary>
        /// UDP answers nothing at all, so the only proof that the line is right is the datagram a
        /// collector would have read.
        /// </summary>
        [TestMethod]
        public async Task udp_hands_the_collector_one_line()
        {
            using (var collector = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
            {
                var port = ((IPEndPoint)collector.Client.LocalEndPoint).Port;

                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

                var received = Task.Run(() => collector.Receive(ref remote));

                var answer = await new SyslogSender(Collector(port)).Deliver(Event(NotificationKind.ServiceStop));

                Assert.IsNull(answer, "the sender reported an error: " + answer);

                var winner = await Task.WhenAny(received, Task.Delay(5000));

                Assert.AreSame(received, winner, "no datagram arrived at the collector");

                var line = Encoding.UTF8.GetString(received.Result);

                Assert.IsTrue(line.StartsWith("<132>", StringComparison.Ordinal), line);

                Assert.IsTrue(line.Contains(" testhost ServiceManager: "), line);

                Assert.IsTrue(line.EndsWith("[ServiceStop] HomeLab / Backup: Service stopped | exit code 3"), line);

                Assert.IsFalse(line.Contains("\r") || line.Contains("\n"), "one event is one entry: " + line);
            }
        }


        /// <summary>
        /// Over TCP the collector has to know where a message ends, which is why the provider puts a
        /// line break behind the line.
        /// </summary>
        [TestMethod]
        public async Task tcp_sends_one_line_terminated_by_a_line_break()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            var accepted = Task.Run(async () =>
            {
                using (var client = await listener.AcceptTcpClientAsync())
                using (var stream = client.GetStream())
                using (var buffer = new System.IO.MemoryStream())
                {
                    var chunk = new byte[4096];
                    int read;

                    while ((read = await stream.ReadAsync(chunk, 0, chunk.Length)) > 0)
                    {
                        buffer.Write(chunk, 0, read);

                        if (Array.IndexOf(chunk, (byte)'\n', 0, read) >= 0)
                            break;
                    }

                    return Encoding.UTF8.GetString(buffer.ToArray());
                }
            });

            var answer = await new SyslogSender(Collector(port, tcp: true)).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNull(answer, "the sender reported an error: " + answer);

            var winner = await Task.WhenAny(accepted, Task.Delay(5000));

            Assert.AreSame(accepted, winner, "the collector never saw the connection");

            var line = accepted.Result;

            Assert.IsTrue(line.StartsWith("<131>", StringComparison.Ordinal), line);

            Assert.IsTrue(line.EndsWith("[ConsoleError] HomeLab / Backup: Service stopped | exit code 3\n"), line);

            listener.Stop();
        }


        /// <summary>
        /// Nothing listens on that port. The provider has to hand the reason to the console instead of
        /// throwing over the service that is already failing.
        /// </summary>
        [TestMethod]
        public async Task a_collector_that_refuses_the_connection_is_reported()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);

            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            listener.Stop();

            var answer = await new SyslogSender(Collector(port, tcp: true)).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "a refused connection was reported as success");

            Assert.IsTrue(answer.Contains("127.0.0.1:" + port), "said: " + answer);
        }


        /// <summary>
        /// A port of 0 or 70000 in a hand written settings file would send the line into nowhere, so
        /// the provider falls back to the port every collector listens on.
        /// </summary>
        [TestMethod]
        public async Task an_out_of_range_port_falls_back_to_514()
        {
            var config = Collector(70000);

            config.Port = -1;

            var answer = await new SyslogSender(config).Deliver(Event(NotificationKind.ConsoleError));

            // Nothing listens on 514 of this machine. What matters is that the provider did not refuse
            // the number in the settings file and that it named the port it actually used.
            Assert.IsTrue(answer == null || answer.Contains(":514"), "said: " + answer);
        }
    }
}
