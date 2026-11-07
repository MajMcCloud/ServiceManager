using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.EventLogNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        /// <summary>Write every event into the Windows Event Log.</summary>
        public bool WriteToEventLog { get; set; }

        /// <summary>
        /// Additionally show a balloon tip. Only visible in an interactive session, a service running
        /// in session zero produces no desktop output.
        /// </summary>
        public bool ShowBalloonTip { get; set; }

        public String EventLog { get; set; }

        public String Source { get; set; }

        /// <summary>
        /// First id of the range. Every event kind gets its own id (EventId + kind), so the event
        /// viewer can filter on a single kind of the ServiceManager.
        /// </summary>
        public int EventId { get; set; }

        public int BalloonSeconds { get; set; }

        public static String FilePath
        {
            get
            {
                var file = Assembly.GetExecutingAssembly().Location;
                var path = Path.GetDirectoryName(file);

                return path + "\\settings.cfg";
            }
        }



        public override void loadDefaultValues()
        {
            this.WriteToEventLog = true;
            this.ShowBalloonTip = false;
            this.EventLog = "Application";
            this.Source = "ServiceManager";
            this.EventId = 1000;
            this.BalloonSeconds = 5;
        }
    }
}
