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
        /// Other managed services that have to be started before this one.
        /// </summary>
        public List<Guid> Dependencies { get; set; } = new List<Guid>();

        /// <summary>
        /// What happens to this service when a service it depends on is restarted.
        /// </summary>
        public eDependencyRestartRule DependencyRestartRule { get; set; } = eDependencyRestartRule.Ignore;

        /// <summary>
        /// What happens to this service when a service it depends on did not start.
        /// </summary>
        public eDependencyStartRule DependencyStartRule { get; set; } = eDependencyStartRule.StartAnyway;

        /// <summary>
        /// What happens to this service when a service it depends on is stopped.
        /// </summary>
        public eDependencyStopRule DependencyStopRule { get; set; } = eDependencyStopRule.StayRunning;

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

        /// <summary>
        /// How a service reacts when a service it depends on is restarted. The numbers are kept because the
        /// settings file stores them.
        /// </summary>
        public enum eDependencyRestartRule
        {
            /// <summary>The dependent service is left alone.</summary>
            Ignore = 0,

            /// <summary>The dependent service is stopped and started again.</summary>
            Restart = 1,

            /// <summary>The dependent service is stopped and stays stopped.</summary>
            Stop = 2
        }

        /// <summary>
        /// How a service reacts when a service it depends on did not start. The numbers are kept because the
        /// settings file stores them.
        /// </summary>
        public enum eDependencyStartRule
        {
            /// <summary>The dependent service starts anyway, which is what the manager did before.</summary>
            StartAnyway = 0,

            /// <summary>The dependent service is taken offline and stays offline.</summary>
            DoNotStart = 1
        }

        /// <summary>
        /// How a service reacts when a service it depends on is stopped. The numbers are kept because the
        /// settings file stores them.
        /// </summary>
        public enum eDependencyStopRule
        {
            /// <summary>The dependent service keeps running, which is what the manager did before.</summary>
            StayRunning = 0,

            /// <summary>The dependent service is stopped as well.</summary>
            Stop = 1
        }

    }
}
