using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceManager.Base.data
{

    public class ServiceAnalytics
    {

        public Guid ServiceID { get; set; }


        public int ProcessID { get; set; }


        public DateTime? Started { get; set; }


        public DateTime? Exiting { get; set; }

        public DateTime? Exited { get; set; }


        public bool IsRunning
        {
            get
            {
                try
                {
                    var proc = Process.GetProcessById(this.ProcessID);

                    return !proc.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }


        public String Output { get; set; } = "";


        public String Error { get; set; } = "";

        public int Restarts { get; set; } = 0;

        public DateTime LastSaved { get; set; }

        public DateTime LastPing { get; set; }

    }
}
