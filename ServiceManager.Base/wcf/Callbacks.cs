using ServiceManager.Base.data;
using ServiceManager.Base.wcf.args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.wcf
{
    public interface ICallbacks
    {

        [OperationContract(IsOneWay = true)]
        void ServiceExited(Guid serviceId, ServiceItem service, ServiceAnalytics analytics);

        [OperationContract(IsOneWay = true)]
        void ServiceExiting(Guid serviceId, ServiceItem service, ServiceAnalytics analytics);

        [OperationContract(IsOneWay = true)]
        void ServiceStarted(Guid serviceId, ServiceItem service, ServiceAnalytics analytics);

        [OperationContract(IsOneWay = true)]
        void ServiceUpdated(Guid serviceId, ServiceItem service, ServiceAnalytics analytics);


        [OperationContract(IsOneWay = true)]
        void ShutdownStarted();

        [OperationContract(IsOneWay = true)]
        void ShutdownCompleted();


        [OperationContract(IsOneWay = true)]
        void LiveLogsUpdate(Guid serviceId, String logs);


        [OperationContract(IsOneWay = true)]
        void ActivityPing(Guid serviceId);
    }

    [ServiceBehavior(UseSynchronizationContext = false, AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class Callbacks : ICallbacks
    {
        EventHandlerList _Events { get; set; } = new EventHandlerList();

        private static object __evLiveLogsUpdate = new object();

        private static object __evServiceChanged = new object();


        private static object __evShutdownStarted = new object();

        private static object __evShutdownCompleted = new object();

        private static object __evActivityPing = new object();

        public void LiveLogsUpdate(Guid serviceId, string logs)
        {
            var e = new LiveLogsEventArgs(serviceId, logs);
            OnLiveLogsUpdate(e);
        }

        public void ServiceExited(Guid serviceId, ServiceItem service, ServiceAnalytics analytics)
        {
            OnServiceChanged(new ServiceChangedEventArgs() { ServiceId = serviceId, Status = analytics.Status, PID = analytics.ProcessID, AutoRestart = service.ForceRestart, Restarts = analytics.Restarts });
        }

        public void ServiceExiting(Guid serviceId, ServiceItem service, ServiceAnalytics analytics)
        {
            OnServiceChanged(new ServiceChangedEventArgs() { ServiceId = serviceId, Status = analytics.Status, PID = analytics.ProcessID, AutoRestart = service.ForceRestart, Restarts = analytics.Restarts });
        }

        public void ServiceStarted(Guid serviceId, ServiceItem service, ServiceAnalytics analytics)
        {
            OnServiceChanged(new ServiceChangedEventArgs() { ServiceId = serviceId, Status = analytics.Status, PID = analytics.ProcessID, AutoRestart = service.ForceRestart, Restarts = analytics.Restarts });
        }

        public void ServiceUpdated(Guid serviceId, ServiceItem service, ServiceAnalytics analytics)
        {
            OnServiceChanged(new ServiceChangedEventArgs() { ServiceId = serviceId, Status = analytics.Status, PID = analytics.ProcessID, AutoRestart = service.ForceRestart, Restarts = analytics.Restarts });
        }

        public void ActivityPing(Guid serviceId)
        {
            OnServiceActivityPing(new ActivityPingEventArgs() { ServiceId = serviceId });
        }


        public void ShutdownStarted()
        {
            OnServerShutdownStarted(new EventArgs());
        }

        public void ShutdownCompleted()
        {
            OnServerShutdownCompleted(new EventArgs());
        }

        #region "Events"

        public event EventHandler<LiveLogsEventArgs> LiveLogs
        {
            add
            {
                _Events.AddHandler(__evLiveLogsUpdate, value);
            }
            remove
            {
                _Events.RemoveHandler(__evLiveLogsUpdate, value);
            }
        }

        public void OnLiveLogsUpdate(LiveLogsEventArgs e)
        {
            (this._Events[__evLiveLogsUpdate] as EventHandler<LiveLogsEventArgs>)?.Invoke(this, e);
        }

        public event EventHandler<ActivityPingEventArgs> ServiceActivityPing
        {
            add
            {
                _Events.AddHandler(__evActivityPing, value);
            }
            remove
            {
                _Events.RemoveHandler(__evActivityPing, value);
            }
        }

        public void OnServiceActivityPing(ActivityPingEventArgs e)
        {
            (this._Events[__evActivityPing] as EventHandler<ActivityPingEventArgs>)?.Invoke(this, e);
        }

        public event EventHandler<ServiceChangedEventArgs> ServiceChanged
        {
            add
            {
                _Events.AddHandler(__evServiceChanged, value);
            }
            remove
            {
                _Events.RemoveHandler(__evServiceChanged, value);
            }
        }

        public void OnServiceChanged(ServiceChangedEventArgs e)
        {
            (this._Events[__evServiceChanged] as EventHandler<ServiceChangedEventArgs>)?.Invoke(this, e);
        }


        public event EventHandler ServerShutdownStarted
        {
            add
            {
                _Events.AddHandler(__evShutdownStarted, value);
            }
            remove
            {
                _Events.RemoveHandler(__evShutdownStarted, value);
            }
        }

        public void OnServerShutdownStarted(EventArgs e)
        {
            (this._Events[__evShutdownStarted] as EventHandler)?.Invoke(this, e);
        }

        public event EventHandler ServerShutdownCompleted
        {
            add
            {
                _Events.AddHandler(__evShutdownCompleted, value);
            }
            remove
            {
                _Events.RemoveHandler(__evShutdownCompleted, value);
            }
        }

        public void OnServerShutdownCompleted(EventArgs e)
        {
            (this._Events[__evShutdownCompleted] as EventHandler)?.Invoke(this, e);
        }


        #endregion
    }
}
