using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Base;
using ServiceManager.Base.data;
using ServiceManager.Base.data.settings;
using System;
using System.IO;

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
            Assert.AreEqual(0, (int)Runtime.eMode.NetPipes);
            Assert.AreEqual(1, (int)Runtime.eMode.Tcp);
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
    }
}
