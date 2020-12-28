using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceManager.Base.data.ServiceAnalytics;

namespace ServiceManager.Base.wcf.args
{
    public class ServiceChangedEventArgs
    {

        public Guid ServiceId { get; set; }

        public int PID { get; set; }

        public int Restarts { get; set; } = 0;

        public bool AutoRestart { get; set; } = false;

        public eStatus Status { get; set; }

    }
}
