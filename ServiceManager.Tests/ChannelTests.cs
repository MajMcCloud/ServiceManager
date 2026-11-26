using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceManager.Base;
using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Wcf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
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
            Assert.AreEqual(120, runtime.Dependency_Wait_Timeout, "a service holds back for two minutes by default");

            Assert.IsTrue(runtime.RequireAdministrator, "a fresh installation is closed for ordinary users");
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
                cfg.RuntimeSettings.RequireAdministrator = false;
                cfg.RuntimeSettings.Dependency_Wait_Timeout = 90;

                cfg.save(path);

                var again = ServiceConfig.load(path);

                Assert.AreEqual(Runtime.eMode.Tcp, again.RuntimeSettings.Mode);
                Assert.AreEqual(50555, again.RuntimeSettings.TCPPort);
                Assert.AreEqual(15, again.RuntimeSettings.Startup_Delay);
                Assert.AreEqual(90, again.RuntimeSettings.Dependency_Wait_Timeout);

                Assert.IsFalse(again.RuntimeSettings.RequireAdministrator, "the setting is read back the way it was written");
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void settings_file_without_the_administrator_property_keeps_the_service_closed()
        {
            var directory = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "default.cfg");

            try
            {
                Directory.CreateDirectory(directory);

                File.WriteAllText(path, "{\"RuntimeSettings\":{\"Mode\":0,\"TCPPort\":50000,\"Startup_Delay\":0}}");

                var cfg = ServiceConfig.load(path);

                Assert.IsTrue(cfg.RuntimeSettings.RequireAdministrator, "an older file leaves the service closed for ordinary users");
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void settings_file_without_the_dependency_property_keeps_the_default_wait()
        {
            var directory = Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "default.cfg");

            try
            {
                Directory.CreateDirectory(directory);

                File.WriteAllText(path, "{\"RuntimeSettings\":{\"Mode\":0,\"TCPPort\":50000,\"Startup_Delay\":0}}");

                var cfg = ServiceConfig.load(path);

                Assert.AreEqual(120, cfg.RuntimeSettings.Dependency_Wait_Timeout, "an older file does not turn the wait off");
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

            // The test host runs unelevated, so the endpoint accepts the account that runs it.
            server.Access = Channel.eAccess.OwnUser;

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
            Assert.IsFalse(Channel.ProbeTcp("localhost", FreeTcpPort()));
        }

        [TestMethod]
        public void channel_requires_an_administrator_by_default()
        {
            var channel = new Channel(true, "serviceman1234");

            Assert.AreEqual(Channel.eAccess.Administrators, channel.Access);
        }

        [TestMethod]
        public void server_refuses_a_caller_that_is_not_an_administrator()
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Assert.Inconclusive("an elevated test runner passes the policy, so nothing can be refused here");
            }

            var pipeName = ProbePipeName();

            var server = new Channel(true, pipeName);
            var client = new Channel(false, pipeName);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNull(WaitForCallbackChannel(server, 2000), "the service never stores the callbacks of a refused caller");

                Assert.IsFalse(Channel.ProbePipes("localhost", pipeName), "the test call is refused as well");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void administrator_policy_rejects_a_missing_identity()
        {
            Assert.IsFalse(AdministratorAuthorizationManager.IsAdministrator(null));
        }

        [TestMethod]
        public void administrator_policy_rejects_an_anonymous_caller()
        {
            Assert.IsFalse(AdministratorAuthorizationManager.IsAdministrator(WindowsIdentity.GetAnonymous()));
        }

        [TestMethod]
        public void own_user_policy_accepts_the_account_of_the_service()
        {
            var policy = new OwnUserAuthorizationManager();

            Assert.IsTrue(policy.IsOwnUser(WindowsIdentity.GetCurrent()));
        }

        [TestMethod]
        public void own_user_policy_rejects_an_anonymous_caller()
        {
            var policy = new OwnUserAuthorizationManager();

            Assert.IsFalse(policy.IsOwnUser(WindowsIdentity.GetAnonymous()));
        }

        [TestMethod]
        public void own_user_policy_rejects_a_missing_identity()
        {
            var policy = new OwnUserAuthorizationManager();

            Assert.IsFalse(policy.IsOwnUser(null));
        }

        [TestMethod]
        public void administrators_access_selects_the_administrator_policy()
        {
            var channel = new Channel(true, "serviceman1234");

            Assert.IsInstanceOfType(channel.CreateAuthorizationManager(), typeof(AdministratorAuthorizationManager));
        }

        [TestMethod]
        public void own_user_access_selects_the_own_user_policy()
        {
            var channel = new Channel(true, "serviceman1234");
            channel.Access = Channel.eAccess.OwnUser;

            Assert.IsInstanceOfType(channel.CreateAuthorizationManager(), typeof(OwnUserAuthorizationManager));
        }

        [TestMethod]
        public void own_user_policy_lets_the_account_of_the_service_connect()
        {
            var pipeName = ProbePipeName();

            var server = new Channel(true, pipeName);
            server.Access = Channel.eAccess.OwnUser;

            var client = new Channel(false, pipeName);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNotNull(WaitForCallbackChannel(server), "the test run of the UI reaches its own service");

                Assert.IsTrue(Channel.ProbePipes("localhost", pipeName), "the test call is accepted as well");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void a_fresh_settings_file_answers_the_rule_through_the_old_property()
        {
            var runtime = new Runtime();

            Assert.IsNull(runtime.Access, "a new file carries no rule of its own yet");
            Assert.AreEqual(Channel.eAccess.Administrators, runtime.ResolveAccess());
        }

        [TestMethod]
        public void the_access_rule_is_written_with_the_old_property()
        {
            var runtime = new Runtime();

            runtime.ApplyAccess(Channel.eAccess.Users);

            Assert.AreEqual(Channel.eAccess.Users, runtime.ResolveAccess());

            Assert.IsTrue(runtime.RequireAdministrator, "an older version still reads the file as closed for ordinary users");

                StringAssert.Contains(JsonConvert.SerializeObject(runtime), "\"Access\":3", "the config file stores the number");
            }

            [TestMethod]
            public void the_users_only_rule_is_written_as_the_number_four()
            {
                var runtime = new Runtime();

                runtime.ApplyAccess(Channel.eAccess.UsersOnly);

                Assert.AreEqual(Channel.eAccess.UsersOnly, runtime.ResolveAccess());

                Assert.IsTrue(runtime.RequireAdministrator, "an older version still reads the file as closed for ordinary users");

                StringAssert.Contains(JsonConvert.SerializeObject(runtime), "\"Access\":4", "the config file stores the number");
            }

        [TestMethod]
        public void settings_round_trip_the_access_rule_and_its_users()
        {
            var path = TestConfigPath();

            try
            {
                var cfg = ServiceConfig.load(path);

                cfg.RuntimeSettings.ApplyAccess(Channel.eAccess.Users);
                cfg.RuntimeSettings.Users.Add(new EndpointUser(@"CORP\service.watcher"));

                cfg.save(path);

                var again = ServiceConfig.load(path);

                Assert.AreEqual(Channel.eAccess.Users, again.RuntimeSettings.ResolveAccess());
                Assert.AreEqual(1, again.RuntimeSettings.Users.Count);
                Assert.IsTrue(again.RuntimeSettings.Users[0].MatchesName(@"CORP\service.watcher"));
            }
            finally
            {
                DeleteConfigDirectory(path);
            }
        }

        [TestMethod]
        public void settings_file_without_the_access_rule_keeps_the_rule_of_the_old_property()
        {
            var path = TestConfigPath();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, "{\"RuntimeSettings\":{\"Mode\":0,\"TCPPort\":50000,\"Startup_Delay\":0,\"RequireAdministrator\":false}}");

                var cfg = ServiceConfig.load(path);

                Assert.AreEqual(Channel.eAccess.Everyone, cfg.RuntimeSettings.ResolveAccess(), "an older file that opened the endpoint stays open");

                Assert.IsNotNull(cfg.RuntimeSettings.Users, "the user list is never missing");
                Assert.AreEqual(0, cfg.RuntimeSettings.Users.Count);
            }
            finally
            {
                DeleteConfigDirectory(path);
            }
        }

        [TestMethod]
        public void users_access_selects_the_user_policy()
        {
            var channel = new Channel(true, "serviceman1234");

            channel.Access = Channel.eAccess.Users;

            Assert.IsInstanceOfType(channel.CreateAuthorizationManager(), typeof(UserAuthorizationManager));
        }

        [TestMethod]
        public void the_users_rule_keeps_the_administrator_shortcut()
        {
            var channel = new Channel(true, "serviceman1234");

            channel.Access = Channel.eAccess.Users;

            var manager = channel.CreateAuthorizationManager() as UserAuthorizationManager;

            Assert.IsNotNull(manager, "the users rule reads the configured accounts");

            Assert.IsTrue(manager.AllowAdministrators, "an administrator is let in by its rights alone");
        }

        [TestMethod]
        public void users_only_access_selects_the_user_policy_without_the_administrator_shortcut()
        {
            var channel = new Channel(true, "serviceman1234");

            channel.Access = Channel.eAccess.UsersOnly;

            var manager = channel.CreateAuthorizationManager() as UserAuthorizationManager;

            Assert.IsNotNull(manager, "the users only rule reads the configured accounts");

            Assert.IsFalse(manager.AllowAdministrators, "an administrator needs an entry of its own under this rule");
        }

        [TestMethod]
        public void a_channel_starts_with_an_empty_account_list()
        {
            var channel = new Channel(false, "10.0.0.5", 51000);

            Assert.IsNotNull(channel.Users);
            Assert.AreEqual(0, channel.Users.Count);
        }

        [TestMethod]
        public void users_rule_refuses_a_caller_that_is_not_listed()
        {
            if (AdministratorAuthorizationManager.IsAdministrator(WindowsIdentity.GetCurrent()))
            {
                Assert.Inconclusive("an elevated test runner is let in by its rights, so nothing can be refused here");
            }

            var port = FreeTcpPort();

            var server = new Channel(true, "localhost", port);
            server.Access = Channel.eAccess.Users;
            server.Users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            var client = new Channel(false, "localhost", port);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNull(WaitForCallbackChannel(server, 2000), "the service never stores the callbacks of an unlisted caller");

                Assert.IsFalse(Channel.ProbeTcp("localhost", port), "the test call is refused as well");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void users_rule_lets_a_listed_account_connect()
        {
            var port = FreeTcpPort();

            var account = WindowsIdentity.GetCurrent().Name;

            var server = new Channel(true, "localhost", port);
            server.Access = Channel.eAccess.Users;
            server.Users = new List<EndpointUser> { new EndpointUser(account) };

            var client = new Channel(false, "localhost", port);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNotNull(WaitForCallbackChannel(server), "a listed account reaches the endpoint, signed in over the transport");

                Assert.IsTrue(Channel.ProbeTcp("localhost", port), "the test call reaches it the same way");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void users_only_rule_lets_a_listed_account_connect()
        {
            var port = FreeTcpPort();

            var account = WindowsIdentity.GetCurrent().Name;

            var server = new Channel(true, "localhost", port);
            server.Access = Channel.eAccess.UsersOnly;
            server.Users = new List<EndpointUser> { new EndpointUser(account) };

            var client = new Channel(false, "localhost", port);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNotNull(WaitForCallbackChannel(server), "the rule reads the accounts, and the caller signs in over the transport");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void users_only_rule_refuses_a_caller_that_is_not_listed()
        {
            var port = FreeTcpPort();

            var server = new Channel(true, "localhost", port);
            server.Access = Channel.eAccess.UsersOnly;
            server.Users = new List<EndpointUser> { new EndpointUser("service.watcher") };

            var client = new Channel(false, "localhost", port);

            try
            {
                server.Start();
                client.Start();

                Assert.IsNull(WaitForCallbackChannel(server, 2000), "the rule reads no rights, only the accounts it was given");

                Assert.IsFalse(Channel.ProbeTcp("localhost", port), "the test call is refused as well");
            }
            finally
            {
                client.Stop();
                server.Stop();
            }
        }

        [TestMethod]
        public void server_binding_caps_the_incoming_named_pipe_message()
        {
            var binding = (CoreWCF.NetNamedPipeBinding)Channel.CreateServerBinding(Runtime.eMode.NetPipes);

            Assert.AreEqual(64 * 1024, binding.MaxReceivedMessageSize, "nobody sends more than a command to the service");
            Assert.AreEqual(binding.MaxReceivedMessageSize, binding.MaxBufferSize);
        }

        [TestMethod]
        public void server_binding_caps_the_incoming_tcp_message()
        {
            var binding = (CoreWCF.NetTcpBinding)Channel.CreateServerBinding(Runtime.eMode.Tcp);

            Assert.AreEqual(64 * 1024, binding.MaxReceivedMessageSize);
        }

        [TestMethod]
        public void client_binding_leaves_room_for_the_answers_of_the_service()
        {
            var binding = (NetNamedPipeBinding)Channel.CreateClientBinding(Runtime.eMode.NetPipes);

            Assert.AreEqual(2 * 1024 * 1024, binding.MaxReceivedMessageSize, "the service sends back the whole console log");
            Assert.AreEqual(binding.MaxReceivedMessageSize, binding.MaxBufferPoolSize, "the pool has to hold one whole answer");
        }

        [TestMethod]
        public void server_binding_keeps_the_receive_timeout_open_for_an_idle_console()
        {
            var binding = Channel.CreateServerBinding(Runtime.eMode.NetPipes);

            Assert.AreEqual(TimeSpan.MaxValue, binding.ReceiveTimeout, "a receive timeout would end the session of a console that does nothing");
        }

        [TestMethod]
        public void probe_binding_gives_up_on_a_dead_endpoint_quickly()
        {
            var binding = Channel.CreateProbeBinding(Runtime.eMode.NetPipes);

            Assert.AreEqual(TimeSpan.FromSeconds(5), binding.OpenTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(10), binding.ReceiveTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(2), binding.CloseTimeout);
        }

        [TestMethod]
        public void server_binding_limits_the_reader_quotas()
        {
            var binding = (CoreWCF.NetNamedPipeBinding)Channel.CreateServerBinding(Runtime.eMode.NetPipes);

            Assert.AreEqual(32, binding.ReaderQuotas.MaxDepth);
            Assert.AreEqual(1024 * 1024, binding.ReaderQuotas.MaxStringContentLength);
            Assert.AreEqual(1024 * 1024, binding.ReaderQuotas.MaxArrayLength);
        }

        // WCF allows letters and digits in a pipe name only, and every run needs its own endpoint.
        private static String ProbePipeName()
        {
            return "servicemanprobe" + Guid.NewGuid().ToString("N");
        }

        // A tcp endpoint needs a port of its own, and a listener is the only way to know one is free.
        private static int FreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;

            listener.Stop();

            return port;
        }

        private static String TestConfigPath()
        {
            return Path.Combine(Path.GetTempPath(), "servicemanager-tests", Guid.NewGuid().ToString("N"), "default.cfg");
        }

        private static void DeleteConfigDirectory(String ConfigFile)
        {
            var directory = Path.GetDirectoryName(ConfigFile);

            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
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
