using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data
{
    public class Analytics
    {
        public List<ServiceAnalytics> Services { get; set; }

        public Analytics()
        {
            this.Services = new List<ServiceAnalytics>();
        }

    }
}
