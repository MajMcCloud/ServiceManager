using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.wcf.args
{
    public class LiveLogsEventArgs
    {
        public String Logs { get; set; }

        public Guid ServiceId { get; set; }


        public LiveLogsEventArgs(Guid ServiceId, String Logs)
        {
            this.ServiceId = ServiceId;
            this.Logs = Logs;
        }

    }
}
