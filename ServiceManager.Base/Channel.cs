using ServiceManager.Base.wcf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ServiceManager.Base.data.settings.Runtime;

namespace ServiceManager.Base
{
    public class Channel
    {

        public bool Failed { get; set; } = false;

        public bool Reconnect { get; set; } = false;

        public bool IsServer { get; set; } = false;

        public String PipeName { get; set; }

        public String Hostname { get; set; } = "localhost";

        public int Port { get; set; } = 50000;

        public eMode Mode { get; set; }

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
                        

                        var endpoint = host.AddServiceEndpoint(typeof(IManagerService), new NetNamedPipeBinding() { ReceiveTimeout = TimeSpan.MaxValue, SendTimeout = TimeSpan.MaxValue, MaxReceivedMessageSize = Int32.MaxValue, MaxBufferSize = Int32.MaxValue }, this.PipeName);

                        break;
                    case eMode.Tcp:

                        host = new ServiceHost(m_serverInstance);

                        host.OpenTimeout = new TimeSpan(0, 0, 15);
                        host.CloseTimeout = new TimeSpan(0, 0, 15);

                        var endpoint2 = host.AddServiceEndpoint(typeof(IManagerService), new NetTcpBinding() { ReceiveTimeout = TimeSpan.MaxValue, SendTimeout = TimeSpan.MaxValue, MaxReceivedMessageSize = Int32.MaxValue, MaxBufferSize = Int32.MaxValue }, $"net.tcp://{this.Hostname}:{this.Port}/servicemanager");

                        break;
                    default:

                        throw new ArgumentOutOfRangeException(nameof(this.Mode));

                }

                Failed = false;

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

                    channelFactory = new DuplexChannelFactory<IManagerService>(m_ClientCallbacks, new NetNamedPipeBinding() { ReceiveTimeout = TimeSpan.MaxValue, SendTimeout = new TimeSpan(0, 0, 10), MaxReceivedMessageSize = Int32.MaxValue, MaxBufferSize = Int32.MaxValue }, new EndpointAddress($"net.pipe://{this.Hostname}/" + this.PipeName));

                    break;
                case eMode.Tcp:

                    channelFactory = new DuplexChannelFactory<IManagerService>(m_ClientCallbacks, new NetTcpBinding() { ReceiveTimeout = TimeSpan.MaxValue, SendTimeout = new TimeSpan(0, 0, 10), MaxReceivedMessageSize = Int32.MaxValue, MaxBufferSize = Int32.MaxValue }, new EndpointAddress($"net.tcp://{this.Hostname}:{this.Port}/servicemanager"));

                    break;
                default:

                    throw new ArgumentOutOfRangeException(nameof(this.Mode));
            }



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
