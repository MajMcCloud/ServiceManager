using System;
using System.IO;
using System.Reflection;

namespace ServiceManager.Extensions.SmtpNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        /// <summary>Host of the mail server, a bare ip address works as well.</summary>
        public String SmtpServer { get; set; }

        /// <summary>25 without encryption, 587 with STARTTLS, 465 is not supported by SmtpClient.</summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// STARTTLS: the connection starts plain and is encrypted after the server agreed to it. The
        /// implicit TLS of port 465 is not implemented by SmtpClient of .NET Framework.
        /// </summary>
        public bool UseStartTls { get; set; }

        public String Username { get; set; }

        public String Password { get; set; }

        /// <summary>The address the mail comes from, many servers refuse a foreign sender.</summary>
        public String FromAddress { get; set; }

        /// <summary>One address, or several separated by comma or semicolon.</summary>
        public String ToAddresses { get; set; }

        /// <summary>First word of the subject, so a mail rule can pick the ServiceManager out.</summary>
        public String SubjectPrefix { get; set; }

        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Seconds between two identical mails. A console line that repeats itself every second would
        /// otherwise send a mail per line. Zero switches the guard off.
        /// </summary>
        public int ThrottleSeconds { get; set; }

        /// <summary>
        /// No server involved: the mail is written as an .eml file into this folder. Handy to look at
        /// the layout without a server, and a way to feed a mail gateway that reads that folder.
        /// </summary>
        public String PickupDirectory { get; set; }

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
            this.SmtpServer = "";
            this.SmtpPort = 25;
            this.UseStartTls = false;
            this.Username = "";
            this.Password = "";
            this.FromAddress = "";
            this.ToAddresses = "";
            this.SubjectPrefix = "ServiceManager";
            this.TimeoutSeconds = 15;
            this.ThrottleSeconds = 60;
            this.PickupDirectory = "";
        }
    }
}
