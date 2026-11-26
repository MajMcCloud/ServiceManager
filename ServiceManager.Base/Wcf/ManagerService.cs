using ServiceManager.Base.Data;
using ServiceManager.Base.Events;
using ServiceManager.Base.Wcf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CWC = CoreWCF;

namespace ServiceManager.Base.Wcf
{

    // The contract carries both attribute sets: CoreWCF on the server only reads its own attributes,
    // the System.ServiceModel channel of the UI only reads the ones it knows.
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbacks))]
    [CWC.ServiceContract(SessionMode = CWC.SessionMode.Required, CallbackContract = typeof(ICallbacks))]
    public interface IManagerService
    {

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void Init();

        [OperationContract]
[CWC.OperationContract]
        GetServiceAnalyticsResponse GetServiceAnalytics(Guid serviceId);

        [OperationContract]
[CWC.OperationContract]
        GetServicesAnalyticsResponse GetServicesAnalytics();


        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void StartService(Guid serviceId);


        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void RestartService(Guid serviceId);


        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void ShutdownService(Guid serviceId);

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void ToggleEnable(Guid serviceId);

        [OperationContract]
[CWC.OperationContract]
        GetServiceLogsResponse GetServiceLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void BeginLiveLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void EndLiveLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void SendInput(Guid serviceId, String input);

        [OperationContract(IsOneWay = true)]
[CWC.OperationContract(IsOneWay = true)]
        void ToggleAutoRestart(Guid serviceId);

        [OperationContract]
[CWC.OperationContract]
        void Ping();

    }

    [CWC.ServiceBehavior(InstanceContextMode = CWC.InstanceContextMode.Single, ConcurrencyMode = CWC.ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = false, UseSynchronizationContext = false)]
    public class ManagerService : IManagerService
    {
        public ICallbacks CallbackChannel { get; set; }

        public ServiceManager Manager { get; set; }

        public void Init()
        {
            this.CallbackChannel = CWC.OperationContext.Current.GetCallbackChannel<ICallbacks>();
        }

        public GetServiceAnalyticsResponse GetServiceAnalytics(Guid serviceId)
        {
            var response = new GetServiceAnalyticsResponse();

            var item = this.Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            if (item == null)
                return null;

            var sa = this.Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            if (sa != null)
            {
                this.Manager.Sampler.Apply(sa);
            }

            response.Service = item;
            response.Analytics = sa;

            return response;
        }

        public GetServicesAnalyticsResponse GetServicesAnalytics()
        {
            var response = new GetServicesAnalyticsResponse();

            foreach (var an in this.Manager.Analytics.Services)
            {
                //The list is polled, so every poll is a new measurement and the one before it is still known
                this.Manager.Sampler.Apply(an);

                var sa = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceAnalytics>(Newtonsoft.Json.JsonConvert.SerializeObject(an));
                if (sa == null)
                    continue;

                sa.Output = "";
                sa.Error = "";

                response.Analytics.Add(sa);
            }


            response.Services = this.Manager.Config.ServiceList;

            return response;
        }

        public void StartService(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);


            if (analytics.IsRunning)
            {
                ServiceLog(serviceId, "Service is already running.");
                return;
            }

            if (analytics.Status == ServiceAnalytics.eStatus.waiting_for_dependency)
            {
                ServiceLog(serviceId, "Service is already waiting for a dependency.");
                return;
            }

            Manager.StartService(service);

            ServiceLog(serviceId, "Service has been started.");
        }

        public void RestartService(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);

            if (service == null)
            {
                return;
            }

            ServiceLog(serviceId, "Service will be restarted.");

            Manager.RestartService(service);

            ServiceLog(serviceId, "Service has been restarted.");
        }

        public void ShutdownService(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);

            if (service == null)
            {
                return;
            }

            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            if (!analytics.IsRunning && analytics.Status != ServiceAnalytics.eStatus.waiting_for_dependency)
            {
                ServiceLog(serviceId, "Service is already offline.");
                return;
            }

            ServiceLog(serviceId, "Service will be ended.");

            //Without the flag the own restart path of the service would bring it straight back up
            service.ForceRestart = false;

            //The rules of the services that wait for this one are applied by the manager
            Manager.StopService(service);

            if (!analytics.IsRunning)
            {
                ServiceLog(serviceId, "Service has been ended.");
            }
        }

        public void ToggleEnable(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);

            service.Enabled = !service.Enabled;
        }

        public GetServiceLogsResponse GetServiceLogs(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            if(service.LogConsoleOutput)
            {
                GetServiceLogsResponse logs = new GetServiceLogsResponse();

                logs.Logs = analytics.Output;
                logs.Errors = analytics.Error;

                return logs;
            }
            else
            {
                GetServiceLogsResponse logs = new GetServiceLogsResponse();

                logs.Logs = "SYSTEM: Output history logging is currently disabled.\r\n";
                logs.Errors = "SYSTEM: Error history logging is currently disabled.\r\n";

                return logs;
            }



        }

        public void BeginLiveLogs(Guid serviceId)
        {
            if (!Manager.LiveLogs.Contains(serviceId))
                Manager.LiveLogs.Add(serviceId);
        }

        public void EndLiveLogs(Guid serviceId)
        {
            if (Manager.LiveLogs.Contains(serviceId))
                Manager.LiveLogs.Remove(serviceId);
        }

        public void SendInput(Guid serviceId, String input)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);


            Manager.SendInput(service, input);

        }

        private void ServiceLog(Guid serviceId, String log)
        {
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);
            if (analytics == null)
            {
                return;
            }

            String s = "SERVICE MANAGER: " + log;
            analytics.AppendOutput(s + "\r\n");

            //A manager message is shown whether or not the client asked for the live log of the service,
            //so the live log gate sits with the console output and not here.
            Manager.Raise(eCallback.logs, serviceId, s);
        }

        public void ToggleAutoRestart(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            service.ForceRestart = !service.ForceRestart;

            Manager.Raise(eCallback.updated, service, analytics);
        }

        /// <summary>
        /// Just a Ping 
        /// </summary>
        public void Ping()
        {
            
        }

    }
}
