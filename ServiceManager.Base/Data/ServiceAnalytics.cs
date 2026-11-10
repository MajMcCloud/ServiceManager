using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServiceManager.Base.Data
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


        /// <summary>
        /// One stream never grows past this many characters.
        /// </summary>
        public const int MaxStreamLength = 256 * 1024;


        /// <summary>
        /// Adds text to the output and drops the oldest lines when the limit is reached.
        /// </summary>
        public void AppendOutput(String text)
        {
            this.Output = TrimToTail(this.Output + text);
        }


        /// <summary>
        /// Adds text to the error stream and drops the oldest lines when the limit is reached.
        /// </summary>
        public void AppendError(String text)
        {
            this.Error = TrimToTail(this.Error + text);
        }

        private static String TrimToTail(String value)
        {
            if (value.Length <= MaxStreamLength)
            {
                return value;
            }

            String tail = value.Substring(value.Length - MaxStreamLength);

            // Cut on a line boundary so the first line of the tail is not a fragment.
            int lineBreak = tail.IndexOf('\n');
            if (lineBreak >= 0)
            {
                tail = tail.Substring(lineBreak + 1);
            }

            return tail;
        }

        public int Restarts { get; set; } = 0;

        public DateTime LastSaved { get; set; }

        public DateTime LastPing { get; set; }

        public eStatus Status { get; set; }


        /// <summary>
        /// The status as English text. The enum name is not what a person should read in the service list.
        /// </summary>
        public static String StatusText(eStatus status)
        {
            switch (status)
            {
                case eStatus.starting:
                    return "starting";
                case eStatus.started:
                    return "started";
                case eStatus.running:
                    return "running";
                case eStatus.shutting_down:
                    return "shutting down";
                case eStatus.offline:
                    return "offline";
                case eStatus.start_failed:
                    return "start failed";
                case eStatus.failed:
                    return "failed";
                case eStatus.updated:
                    return "updated";
                case eStatus.waiting_for_dependency:
                    return "waiting for dependency";
                default:
                    return "unknown";
            }
        }

        public enum eStatus
        {
            unknown = -1,
            starting = 1,
            started = 2,
            running = 3,
            shutting_down = 4,
            offline = 5,
            start_failed = 6,
            failed = 7,
            updated = 8,
            waiting_for_dependency = 9
        }

    }
}
