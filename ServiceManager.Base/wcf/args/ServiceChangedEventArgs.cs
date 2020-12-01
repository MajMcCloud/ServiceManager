using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.wcf.args
{
    public class ServiceChangedEventArgs
    {
        public enum eState
        {
            started = 0,
            exited = 1,
            updated = 2,
            exiting = 3
        }

        public Guid ServiceId { get; set; }

        public eState State { get; set; }

        public int PID { get; set; }

        public int Restarts { get; set; } = 0;

        public bool AutoRestart { get; set; } = false;


    }
}
