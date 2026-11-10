using ServiceManager.Base.Data.Settings;
using ServiceManager.ConfigBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data
{
    public class ServiceConfig : ConfigBase.ConfigBase<ServiceConfig>
    {
        public List<ServiceItem> ServiceList { get; set; }

        public Runtime RuntimeSettings { get; set; }

        public WindowsService WindowsSettings { get; set; }

        public Notifications Notifications { get; set; }



        public ServiceConfig()
        {
            this.ServiceList = new List<ServiceItem>();
            this.RuntimeSettings = new Runtime();
            this.WindowsSettings = new WindowsService();
            this.Notifications = new Notifications();
        }



    }
}
