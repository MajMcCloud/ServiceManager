using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data
{
    public class ServiceItem
    {
        public Guid ID { get; set; }

        public String Description { get; set; }

        public String Title { get; set; }

        public String Path { get; set; }

        public bool Enabled { get; set; } = true;


        public bool ForceRestart { get; set; }

        /// <summary>
        /// Windows services that have to run before this service is started.
        /// </summary>
        public List<String> WindowsDependencies { get; set; } = new List<String>();

        /// <summary>
        /// Wait for the Windows dependencies after an automatic restart as well.
        /// </summary>
        public bool Wait_On_Restart { get; set; } = true;

        public int Shutdown_Timeout { get; set; }

        public bool Shutdown_ENTER_Send { get; set; }

        public int Shutdown_ENTER_Timeout { get; set; }

        /// <summary>
        /// Logs console output to memory
        /// </summary>
        public bool LogConsoleOutput { get; set; } = true;


        /// <summary>
        /// Logs console output to disk
        /// </summary>
        public bool LogConsoleOutputToDisk { get; set; }


        /// <summary>
        /// Clears up daily log cache from memory
        /// </summary>
        public bool ResetConsoleOutputDaily { get; set; } = true;

        public ServiceItem()
        {

        }

    }
}
