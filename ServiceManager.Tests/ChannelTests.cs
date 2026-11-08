using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceManager.Base;
using ServiceManager.Base.data;
using ServiceManager.Base.data.settings;
using ServiceManager.Base.wcf;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServiceManager.Tests
{
    /// <summary>
    /// The connection is configured in code, not in app.config: which transport a given
    /// constructor selects and which defaults end up in the service config file.
    /// </summary>
    [TestClass]
    public class ChannelTests
    {
        [TestMethod]
        public void pipeConstructor_selectsNamedPipesAndKeepsTheTcpDefaultsUnused()
        {
            var channel = new Channel(true, "serviceman1234");

            Assert.AreEqual(Runtime.eMode.NetPipes, channel.Mode);
            Assert.AreEqual("serviceman1234", channel.PipeName);
            Assert.IsTrue(channel.IsServer);

            Assert.AreEqual("localhost", channel.Hostname, "the default is only used by the tcp mode");
            Assert.AreEqual(50000, channel.Port);

            Assert.IsFalse(channel.Failed);
            Assert.IsFalse(channel.Reconnect);
        }

        [TestMethod]
        public void tcpConstructor_selectsTcpAndStoresHostAndPort()
        {
            var channel = new Channel(false, "10.0.0.5", 51000);

            Assert.AreEqual(Runtime.eMode.Tcp, channel.Mode);
            Assert.AreEqual("10.0.0.5", channel.Hostname);
            Assert.AreEqual(51000, channel.Port);
            Assert.IsFalse(channel.IsServer);
        }

        [TestMethod]
        public void runtimeDefaults_areNamedPipesOnPort50000()
        {
            var runtime = new Runtime();

            Assert.AreEqual(Runtime.eMode.NetPipes, runtime.Mode);
            Assert.AreEqual(50000, runtime.TCPPort);
            Assert.AreEqual(0, runtime.Startup_Delay);
        }

        [TestMethod]
        public void modeEnumValues_areStableBecauseTheConfigFileStoresTheNumber()
        {
            // Newtonsoft writes enums as numbers, so an existing default.cfg carries the raw
            // value. Reordering the members would silently flip every installed config.
            var runtime = new Runtime();

            StringAssert.Contains(JsonConvert.SerializeObject(runtime), "\"Mode\":0", "the default transport is named pipes");

            runtime.Mode = Runtime.eMode.Tcp;

            StringAssert.Contains(JsonConvert.SerializeObject(runtime), "\"Mode\":1", "Tcp has to stay the second member");
        }

        [TestMethod]
        public void serviceConfig_startsComplete_soNoCallerHasToNullCheck()
        {
            var cfg = new ServiceConfig();

            Assert.IsNotNull(cfg.ServiceList);
            Assert.IsNotNull(cfg.RuntimeSettings);
            Assert.IsNotNull(cfg.Notifications);

            Assert.AreEqual(0, cfg.ServiceList.Count);
            Assert.AreEqual(Runtime.eMode.NetPipes, cfg.RuntimeSettings.Mode);
            Assert.AreEqual(50000, cfg.RuntimeSettings.TCPPort);
        }

        [TestMethod]
        public void serviceConfig_roundTripsTheRuntimeSettingsThroughTheConfigFile()
        {
            var directory = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "default.cfg");

            try
            {
                var cfg = ServiceConfig.load(path);

                cfg.RuntimeSettings.Mode = Runtime.eMode.Tcp;
                cfg.RuntimeSettings.TCPPort = 50555;
                cfg.RuntimeSettings.Startup_Delay = 15;

                cfg.save(path);

                var again = ServiceConfig.load(path);

                Assert.AreEqual(Runtime.eMode.Tcp, again.RuntimeSettings.Mode);
                Assert.AreEqual(50555, again.RuntimeSettings.TCPPort);
                Assert.AreEqual(15, again.RuntimeSettings.Startup_Delay);
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void probe_pipes_keeps_the_callback_channel_of_the_connected_client()
        {
            var pipeName = ProbePipeName();

            var server = new Channel(true, pipeName);
            var client = new Channel(false, pipeName);

            try
            {
                server.Start();
                client.Start();

                var registered = WaitForCallbackChannel(server);

                Assert.IsNotNull(registered, "the client hands over its callbacks when it connects");

                Assert.IsTrue(Channel.ProbePipes("localhost", pipeName), "the running service answers the test");

                Assert.AreSame(registered, server.ServerChannel, "a connection test must not take the live updates away from the connected client");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void probe_pipes_is_false_without_a_listening_service()
        {
            Assert.IsFalse(Channel.ProbePipes("localhost", ProbePipeName()));
        }

        [TestMethod]
        public void probe_tcp_is_false_for_a_port_nobody_listens_on()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            listener.Stop();

            Assert.IsFalse(Channel.ProbeTcp("localhost", port));
        }

        // WCF allows letters and digits in a pipe name only, and every run needs its own endpoint.
        private static String ProbePipeName()
        {
            return "servicemanprobe" + Guid.NewGuid().ToString("N");
        }

        // Init is a one way call, so the service stores the channel some milliseconds later.
        private static ICallbacks WaitForCallbackChannel(Channel server, int TimeoutMs = 5000)
        {
            var watch = Stopwatch.StartNew();

            while (server.ServerChannel == null && watch.ElapsedMilliseconds < TimeoutMs)
            {
                Thread.Sleep(25);
            }

            return server.ServerChannel;
        }
    }
}
