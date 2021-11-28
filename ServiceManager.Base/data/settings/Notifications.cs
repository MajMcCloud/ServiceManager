using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.data.settings
{
    public  class Notifications
    {
        public String InstanceName { get; set; } = "Service Manager";


        public Guid? PluginId { get; set; }

        public bool OnRestart { get; set; }

        public bool OnStart { get; set; }

        public bool OnStop { get; set; }

        public bool OnConsoleOutput { get; set; }

        public bool OnConsoleError { get; set; }


        public bool OnServerStart { get; set; }

        public bool OnServerShutdown { get; set; }



    }
}
