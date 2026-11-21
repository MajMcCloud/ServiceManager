using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Wcf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ServiceManager.Base.Data.Settings.Runtime;

namespace ServiceManager.Base
{
    public class Channel
    {

        private const int MaxIncomingMessageSize = 64 * 1024;

        private const int MaxAnswerMessageSize = 2 * 1024 * 1024;

        private const int MaxConcurrentSessions = 20;

        private const int MaxConcurrentCalls = 16;

        public bool Failed { get; set; } = false;

        public bool Reconnect { get; set; } = false;

        public bool IsServer { get; set; } = false;

        public String PipeName { get; set; }

        public String Hostname { get; set; } = "localhost";

        public int Port { get; set; } = 50000;

        public eMode Mode { get; set; }

        /// <summary>
        /// Who may connect to the endpoint.
        /// </summary>
        public enum eAccess
        {
            /// <summary>Members of the local Administrators group only.</summary>
            Administrators = 0,

            /// <summary>The account that runs the service itself.</summary>
            OwnUser = 1,

            /// <summary>Every local user, the escape hatch of the settings.</summary>
            Everyone = 2,

            /// <summary>Administrators, and the accounts named in the settings with their password.</summary>
            Users = 3,

            /// <summary>Only the accounts named in the settings, an administrator needs an entry as well.</summary>
            UsersOnly = 4
        }

        /// <summary>
        /// Who may connect. Administrators is the rule of the installed service, OwnUser is what
        /// a test run inside the UI needs, Everyone lets ordinary users watch the logs as well.
        /// </summary>
        public eAccess Access { get; set; } = eAccess.Administrators;

        /// <summary>
        /// The accounts that may answer with a password. Only the server reads them, and an empty list
        /// refuses every caller that is not an administrator.
        /// </summary>
        public List<EndpointUser> Users { get; set; } = new List<EndpointUser>();

        /// <summary>
        /// The account the client names in its credential header. Without one no header is sent, and the
        /// caller is judged by its Windows identity alone.
        /// </summary>
        public String CredentialUser { get; set; }

        /// <summary>The password that belongs to <see cref="CredentialUser"/>.</summary>
        public String CredentialPassword { get; set; }

        private ServiceHost host { get; set; }

        private IManagerService m_clientChannel { get; set; }

        private Callbacks m_ClientCallbacks { get; set; }

        private ManagerService m_serverInstance { get; set; }

        private ICallbacks m_serverChannel { get; set; }

        private DuplexChannelFactory<IManagerService> channelFactory { get; set; }


        EventHandlerList _Events { get; set; } = new EventHandlerList();

        private static object __evConnected = new object();

        private static object __evDisconnected = new object();

        private static object __evFaulted = new object();

        public Channel(bool IsServer, String Pipename)
        {
            this.IsServer = IsServer;
            this.PipeName = Pipename;
            this.Mode = eMode.NetPipes;
        }

        public Channel(bool IsServer, String Hostname, int Port)
        {
            this.IsServer = IsServer;
            this.Hostname = Hostname;
            this.Port = Port;
            this.Mode = eMode.Tcp;
        }


        // A test of an endpoint must never call Init: the service keeps one callback channel for
        // the whole instance, so the throwaway channel of a test would take the live updates away
        // from the UI that is connected. Ping answers over the same session and registers nothing.
        public static bool ProbePipes(String Hostname, String PipeName)
        {
            return ProbePipes(Hostname, PipeName, null, null);
        }

        public static bool ProbePipes(String Hostname, String PipeName, String User, String Password)
        {
            return Probe(() => WithCredential(new DuplexChannelFactory<IManagerService>(new Callbacks(), CreateProbeBinding(eMode.NetPipes), new EndpointAddress($"net.pipe://{Hostname}/{PipeName}")), User, Password));
        }

        public static bool ProbeTcp(String Hostname, int Port)
        {
            return ProbeTcp(Hostname, Port, null, null);
        }

        public static bool ProbeTcp(String Hostname, int Port, String User, String Password)
        {
            return Probe(() => WithCredential(new DuplexChannelFactory<IManagerService>(new Callbacks(), CreateProbeBinding(eMode.Tcp), new EndpointAddress($"net.tcp://{Hostname}:{Port}/servicemanager")), User, Password));
        }

        // A server in the Users rule refuses a call that carries no credential, so a connection test
        // has to send one as well.
        private static DuplexChannelFactory<IManagerService> WithCredential(DuplexChannelFactory<IManagerService> Factory, String User, String Password)
        {
            if (String.IsNullOrEmpty(User))
            {
                return Factory;
            }

            Factory.Endpoint.Behaviors.Add(new EndpointCredentialBehavior(User, Password));

            return Factory;
        }

        private static bool Probe(Func<DuplexChannelFactory<IManagerService>> CreateFactory)
        {
            DuplexChannelFactory<IManagerService> factory = null;

            IManagerService client = null;

            try
            {
                factory = CreateFactory();

                client = factory.CreateChannel();

                client.Ping();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                // The test runs once per menu entry, so a channel left behind would keep a session
                // on the service for the whole lifetime of the UI.
                if (client != null)
                {
                    try
                    {
                        ((ICommunicationObject)client).Close(new TimeSpan(0, 0, 0, 2));
                    }
                    catch
                    {
                        ((ICommunicationObject)client).Abort();
                    }
                }

                if (factory != null)
                {
                    try
                    {
                        factory.Close(new TimeSpan(0, 0, 0, 2));
                    }
                    catch
                    {
                        factory.Abort();
                    }
                }
            }
        }


        private void Init()
        {
            if (IsServer)
            {
                m_serverInstance = new ManagerService();

                switch (this.Mode)
                {
                    case eMode.NetPipes:

                        host = new ServiceHost(m_serverInstance, new Uri[] { new Uri($"net.pipe://{this.Hostname}") });

                        host.OpenTimeout = new TimeSpan(0, 0, 15);
                        host.CloseTimeout = new TimeSpan(0, 0, 15);
                        

                        var endpoint = host.AddServiceEndpoint(typeof(IManagerService), CreateServerBinding(eMode.NetPipes), this.PipeName);

                        break;
                    case eMode.Tcp:

                        host = new ServiceHost(m_serverInstance);

                        host.OpenTimeout = new TimeSpan(0, 0, 15);
                        host.CloseTimeout = new TimeSpan(0, 0, 15);

                        var endpoint2 = host.AddServiceEndpoint(typeof(IManagerService), CreateServerBinding(eMode.Tcp), $"net.tcp://{this.Hostname}:{this.Port}/servicemanager");

                        break;
                    default:

                        throw new ArgumentOutOfRangeException(nameof(this.Mode));

                }

                Failed = false;

                if (this.Access != eAccess.Everyone)
                {
                    ServiceAuthorizationBehavior authorization = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
                    if (authorization != null)
                    {
                        authorization.ServiceAuthorizationManager = CreateAuthorizationManager();
                    }
                }

                if (host.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
                {
                    ServiceThrottlingBehavior throttling = new ServiceThrottlingBehavior();
                    throttling.MaxConcurrentSessions = MaxConcurrentSessions;
                    throttling.MaxConcurrentCalls = MaxConcurrentCalls;
                    host.Description.Behaviors.Add(throttling);
                }

                host.Opened += Host_Opened;
                host.Closed += Host_Closed;
                host.Faulted += Host_Faulted;

                host.Open();

                return;
            }

            m_ClientCallbacks = new Callbacks();

            switch (this.Mode)
            {
                case eMode.NetPipes:

                    channelFactory = new DuplexChannelFactory<IManagerService>(m_ClientCallbacks, CreateClientBinding(eMode.NetPipes), new EndpointAddress($"net.pipe://{this.Hostname}/" + this.PipeName));

                    break;
                case eMode.Tcp:

                    channelFactory = new DuplexChannelFactory<IManagerService>(m_ClientCallbacks, CreateClientBinding(eMode.Tcp), new EndpointAddress($"net.tcp://{this.Hostname}:{this.Port}/servicemanager"));

                    break;
                default:

                    throw new ArgumentOutOfRangeException(nameof(this.Mode));
            }

            // The credential belongs to the factory, so every later call over it carries the header too.
            WithCredential(channelFactory, this.CredentialUser, this.CredentialPassword);

            m_clientChannel = channelFactory.CreateChannel();

            try
            {
                m_clientChannel.Init();

                OnConnected(new EventArgs());
            }
            catch
            {
                OnFaulted(new EventArgs());

                return;
            }

            Failed = false;

            m_ClientCallbacks.ServerShutdownStarted += M_ClientCallbacks_ServerShutdownStarted;

            channelFactory.Faulted += ChannelFactory_Faulted;
            channelFactory.Closed += ChannelFactory_Closed;
        }

        internal static Binding CreateServerBinding(eMode mode)
        {
            Binding binding = CreateBinding(mode, MaxIncomingMessageSize);

            // A duplex session has no traffic while it is idle, so a receive
            // timeout would end the session of every idle console.
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;

            return binding;
        }

        internal static Binding CreateClientBinding(eMode mode)
        {
            Binding binding = CreateBinding(mode, MaxAnswerMessageSize);

            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.FromSeconds(10);

            return binding;
        }

        internal static Binding CreateProbeBinding(eMode mode)
        {
            Binding binding = CreateBinding(mode, MaxIncomingMessageSize);

            binding.OpenTimeout = TimeSpan.FromSeconds(5);
            binding.SendTimeout = TimeSpan.FromSeconds(10);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(10);
            binding.CloseTimeout = TimeSpan.FromSeconds(2);

            return binding;
        }

        private static Binding CreateBinding(eMode mode, int maxMessageSize)
        {
            switch (mode)
            {
                case eMode.NetPipes:
                {
                    NetNamedPipeBinding binding = new NetNamedPipeBinding();
                    binding.MaxReceivedMessageSize = maxMessageSize;
                    binding.MaxBufferSize = maxMessageSize;
                    binding.MaxBufferPoolSize = maxMessageSize;
                    ApplyReaderQuotas(binding.ReaderQuotas);

                    return binding;
                }
                case eMode.Tcp:
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.MaxReceivedMessageSize = maxMessageSize;
                    binding.MaxBufferSize = maxMessageSize;
                    binding.MaxBufferPoolSize = maxMessageSize;
                    ApplyReaderQuotas(binding.ReaderQuotas);

                    return binding;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static void ApplyReaderQuotas(System.Xml.XmlDictionaryReaderQuotas quotas)
        {
            quotas.MaxDepth = 32;
            quotas.MaxStringContentLength = 1024 * 1024;
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxBytesPerRead = 4096;
            quotas.MaxNameTableCharCount = 16384;
        }

        internal ServiceAuthorizationManager CreateAuthorizationManager()
        {
            if (this.Access == eAccess.OwnUser)
            {
                return new OwnUserAuthorizationManager();
            }

            if (this.Access == eAccess.Users)
            {
                return new UserAuthorizationManager(this.Users);
            }

            if (this.Access == eAccess.UsersOnly)
            {
                return new UserAuthorizationManager(this.Users, false);
            }

            return new AdministratorAuthorizationManager();
        }

        private void Host_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Server connected.");
        }

        private void M_ClientCallbacks_ServerShutdownStarted(object sender, EventArgs e)
        {
            this.Reconnect = false;
        }

        private void Host_Faulted(object sender, EventArgs e)
        {
            OnFaulted(e);

            Console.WriteLine("Server faulted.");

            if (Reconnect)
            {
                Init();
            }
        }

        private void Host_Closed(object sender, EventArgs e)
        {
            OnDisconnected(e);

            if (Reconnect)
            {
                Init();
            }
        }


        private void ChannelFactory_Closed(object sender, EventArgs e)
        {
            OnDisconnected(e);

            //if (Reconnect)
            //{
            //    Init();
            //}
        }

        private void ChannelFactory_Faulted(object sender, EventArgs e)
        {
            OnFaulted(e);

            Console.WriteLine("Client faulted.");

            //if (Reconnect)
            //{
            //    Init();
            //}
        }

        public void Start()
        {
            this.Init();
        }

        public void Stop()
        {
            this.Reconnect = false;

            if (IsServer)
            {
                if (host == null)
                    return;

                if (host.State != CommunicationState.Opened)
                    return;


                try
                {
                    //this?.Try(a => a.ShutdownStarted());

                    host.Abort();
                    //host.Close(new TimeSpan(0, 0, 5));
                }
                catch
                {

                }

                return;
            }

            if (channelFactory == null)
                return;

            if (channelFactory.State != CommunicationState.Opened)
                return;


            try
            {
                channelFactory.Abort();
                //channelFactory.Close(new TimeSpan(0, 0, 5));
            }
            catch
            {

            }

            channelFactory = null;

            m_clientChannel = null;

        }

        public IManagerService Client
        {
            get
            {
                return m_clientChannel;
            }
        }

        public void TryCatch(Action<IManagerService> func, Action<IManagerService, Exception> OnException = null)
        {
            IManagerService client = Client;

            if (this.channelFactory.State != CommunicationState.Opened)
            {
                return;
            }

            try
            {
                func(client);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                if (OnException != null)
                    OnException(client, ex);
            }
            catch (Exception ex)
            {
                if (OnException != null)
                    OnException(client, ex);
            }

        }

        public TResult TryCatch<TResult>(Func<IManagerService, TResult> func, Action<IManagerService, Exception> OnException = null)
        {
            IManagerService client = Client;

            if (this.channelFactory.State != CommunicationState.Opened)
            {
                return default(TResult);
            }

            TResult output = default(TResult);
            try
            {
                output = func(client);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                if (OnException != null)
                    OnException(client, ex);
            }
            catch (Exception ex)
            {
                if (OnException != null)
                    OnException(client, ex);
            }
            return output;
        }

        public void Async(Action<IManagerService> func, Action<IManagerService, Exception> OnException = null)
        {
            IManagerService client = Client;

            if (this.channelFactory.State != CommunicationState.Opened)
            {
                return;
            }

            try
            {
                Task t = new Task(delegate
                {
                    try
                    {
                        func(client);
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(Client, ex);
                    }

                });

                t.Start();
            }
            catch
            {

            }
        }

        public void Async(Action<IManagerService> func, Action OnResult, Action<IManagerService, Exception> OnException = null)
        {
            IManagerService client = Client;

            if (this.channelFactory.State != CommunicationState.Opened)
            {
                return;
            }

            try
            {
                Task t = new Task(delegate
                {
                    try
                    {
                        func(client);

                        OnResult();
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(Client, ex);
                    }

                });

                t.Start();
            }
            catch
            {

            }
        }

        public void Async<TResult>(Func<IManagerService, TResult> func, Action<TResult> OnResult = null, Action<IManagerService, Exception> OnException = null)
        {
            IManagerService client = Client;

            if (this.channelFactory.State != CommunicationState.Opened)
            {
                return;
            }

            try
            {
                Task t = new Task(delegate
                {
                    try
                    {
                        var result = func(client);

                        OnResult(result);
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                            OnException(Client, ex);
                    }

                });

                t.Start();
            }
            catch
            {

            }
        }



        public ManagerService Server
        {
            get
            {
                return m_serverInstance;
            }
        }

        public ICallbacks ServerChannel
        {
            get
            {
                return Server?.CallbackChannel;
            }
        }

        public Callbacks ClientCallback
        {
            get
            {
                return m_ClientCallbacks;
            }
        }

        public event EventHandler Connected
        {
            add
            {
                _Events.AddHandler(__evConnected, value);
            }
            remove
            {
                _Events.RemoveHandler(__evConnected, value);
            }
        }

        public void OnConnected(EventArgs e)
        {
            (this._Events[__evConnected] as EventHandler)?.Invoke(this, e);
        }

        public event EventHandler Disconnected
        {
            add
            {
                _Events.AddHandler(__evDisconnected, value);
            }
            remove
            {
                _Events.RemoveHandler(__evDisconnected, value);
            }
        }

        public void OnDisconnected(EventArgs e)
        {
            (this._Events[__evDisconnected] as EventHandler)?.Invoke(this, e);
        }

        public event EventHandler Faulted
        {
            add
            {
                _Events.AddHandler(__evFaulted, value);
            }
            remove
            {
                _Events.RemoveHandler(__evFaulted, value);
            }
        }

        public void OnFaulted(EventArgs e)
        {
            (this._Events[__evFaulted] as EventHandler)?.Invoke(this, e);
        }
    }
}
