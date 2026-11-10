using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data.Settings
{
    public class Runtime
    {
        

        public enum eMode
        {
            NetPipes = 0,
            Tcp = 1
        }


        public eMode Mode { get; set; }

        public int TCPPort { get; set; }

        /// <summary>
        /// Delay of service startup in seconds.
        /// </summary>
        public int Startup_Delay { get; set; }

        /// <summary>
        /// Only members of the Administrators group may connect to the service.
        /// </summary>
        public bool RequireAdministrator { get; set; }

        /// <summary>
        /// How long a service waits for a Windows dependency that is not running, in seconds. Zero starts
        /// nothing while a dependency is missing.
        /// </summary>
        public int Dependency_Wait_Timeout { get; set; }

        public Runtime()
        {
            this.Mode = eMode.NetPipes;
            this.TCPPort = 50000;
            this.Startup_Delay = 0;
            this.RequireAdministrator = true;
            this.Dependency_Wait_Timeout = 120;
        }

    }
}
