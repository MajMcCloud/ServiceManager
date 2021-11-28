using ServiceManager.Base.data;
using ServiceManager.Base.wcf.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager.Base.wcf
{

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbacks), ProtectionLevel = System.Net.Security.ProtectionLevel.None)]
    public interface IManagerService
    {

        [OperationContract(IsOneWay = true)]
        void Init();

        [OperationContract]
        GetServiceAnalyticsResponse GetServiceAnalytics(Guid serviceId);

        [OperationContract]
        GetServicesAnalyticsResponse GetServicesAnalytics();


        [OperationContract(IsOneWay = true)]
        void StartService(Guid serviceId);


        [OperationContract(IsOneWay = true)]
        void RestartService(Guid serviceId);


        [OperationContract(IsOneWay = true)]
        void ShutdownService(Guid serviceId);

        [OperationContract(IsOneWay = true)]
        void ToggleEnable(Guid serviceId);

        [OperationContract]
        GetServiceLogsResponse GetServiceLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
        void BeginLiveLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
        void EndLiveLogs(Guid serviceId);

        [OperationContract(IsOneWay = true)]
        void SendInput(Guid serviceId, String input);

        [OperationContract(IsOneWay = true)]
        void ToggleAutoRestart(Guid serviceId);

        [OperationContract]
        void Ping();

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AutomaticSessionShutdown = false, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, UseSynchronizationContext = false)]
    public class ManagerService : IManagerService
    {
        public ICallbacks CallbackChannel { get; set; }

        public ServiceManager Manager { get; set; }

        public void Init()
        {
            this.CallbackChannel = OperationContext.Current.GetCallbackChannel<ICallbacks>();
        }

        public GetServiceAnalyticsResponse GetServiceAnalytics(Guid serviceId)
        {
            var response = new GetServiceAnalyticsResponse();

            var item = this.Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            if (item == null)
                return null;

            var sa = this.Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            response.Service = item;
            response.Analytics = sa;

            return response;
        }

        public GetServicesAnalyticsResponse GetServicesAnalytics()
        {
            var response = new GetServicesAnalyticsResponse();

            foreach (var an in this.Manager.Analytics.Services)
            {
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

            Manager.StartService(service);

            ServiceLog(serviceId, "Service has been started.");
        }

        public void RestartService(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);


            if (analytics.IsRunning)
            {
                ServiceLog(serviceId, "Service will be shutting down.");

                analytics.Exiting = DateTime.Now;
                Manager.ServiceEnd(service);
            }

            //Does not restart automatically
            if (!service.ForceRestart)
            {
                Manager.StartService(service);
            }

            ServiceLog(serviceId, "Service has been restarted.");
        }

        public void ShutdownService(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            if (!analytics.IsRunning)
            {
                ServiceLog(serviceId, "Service is already offline.");
                return;
            }

            ServiceLog(serviceId, "Service will be ended.");


            service.ForceRestart = false;


            if (analytics.IsRunning)
            {
                analytics.Exiting = DateTime.Now;
                Manager.ServiceEnd(service);
            }

            if (!analytics.IsRunning)
                ServiceLog(serviceId, "Service has been ended.");
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
            if (this.CallbackChannel == null)
                return;

            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            String s = "SERVICE MANAGER: " + log;
            analytics.Output += s + "\r\n";

            try
            {
                this.CallbackChannel.LiveLogsUpdate(serviceId, s);
            }
            catch
            {

            }
        }

        public void ToggleAutoRestart(Guid serviceId)
        {
            var service = Manager.Config.ServiceList.FirstOrDefault(a => a.ID == serviceId);
            var analytics = Manager.Analytics.Services.FirstOrDefault(a => a.ServiceID == serviceId);

            service.ForceRestart = !service.ForceRestart;

            Thread t = new Thread(new ThreadStart(() =>
            {
                this.CallbackChannel.ServiceUpdated(serviceId, service, analytics);
            }));
            t.Start();

        }

        /// <summary>
        /// Just a Ping 
        /// </summary>
        public void Ping()
        {
            
        }

    }
}
