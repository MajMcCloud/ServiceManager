using ServiceManager.Base.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Wcf.Models
{
    public class GetServiceAnalyticsResponse
    {
        public ServiceAnalytics Analytics { get; set; }

        public ServiceItem Service { get; set; }


        public GetServiceAnalyticsResponse()
        {

        }
    }

}
