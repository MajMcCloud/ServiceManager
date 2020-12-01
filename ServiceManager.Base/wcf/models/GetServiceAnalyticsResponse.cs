using ServiceManager.Base.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.wcf.models
{
    public class GetServiceAnalyticsResponse
    {
        public List<ServiceAnalytics> Analytics { get; set; }

        public List<ServiceItem> Services { get; set; }


        public GetServiceAnalyticsResponse()
        {
            this.Analytics = new List<ServiceAnalytics>();
            this.Services = new List<ServiceItem>();
        }
    }

}
