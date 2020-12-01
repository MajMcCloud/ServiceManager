using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.data
{
    public class ServiceItem
    {
        public Guid ID { get; set; }

        public String Description { get; set; }

        public String Title { get; set; }

        public String Path { get; set; }

        public bool Enabled { get; set; } = true;


        public bool ForceRestart { get; set; }

        public int Shutdown_Timeout { get; set; }

        public bool Shutdown_ENTER_Send { get; set; }

        public int Shutdown_ENTER_Timeout { get; set; }

        public ServiceItem()
        {

        }

    }
}
