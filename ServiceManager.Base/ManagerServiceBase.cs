using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base
{
    public class ManagerServiceBase : ServiceBase
    {
        ServiceManager Manager { get; set; }

        public ManagerServiceBase()
        {
            this.Manager = new ServiceManager();
        }

        protected override void OnStart(string[] args)
        {
            Manager.Start();

            base.OnStart(args);
        }

        protected override void OnShutdown()
        {
            Manager.Shutdown();

            base.OnShutdown();
        }

        protected override void OnStop()
        {
            Manager.Stop();

            base.OnStop();
        }

    }
}
