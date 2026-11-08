using System;
using System.IO;
using System.Reflection;

namespace ServiceManager.Extensions.SyslogNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        /// <summary>Host or ip of the collector, syslog listens on 514 by default.</summary>
        public String Host { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// UDP is the usual transport and needs no handshake, TCP reaches a collector that answers and
        /// costs nothing more than a connection per message.
        /// </summary>
        public bool UseTcp { get; set; }

        /// <summary>
        /// RFC 5424 carries a structured timestamp with the offset of the sender. Older collectors and
        /// most appliances only understand the 3164 form, so it stays the default.
        /// </summary>
        public bool UseRfc5424 { get; set; }

        /// <summary>
        /// The name the collector sees as sender. Empty takes the name of the machine, which is what a
        /// collector expects when one machine reports several ServiceManager instances.
        /// </summary>
        public String Hostname { get; set; }

        /// <summary>First field after the timestamp, the collector groups by it.</summary>
        public String AppName { get; set; }

        /// <summary>
        /// 0 to 23, 16 is local0 and the range a home or small business collector is free to use.
        /// </summary>
        public int Facility { get; set; }

        /// <summary>
        /// A collector cuts a line around 1024 bytes, so the provider cuts it itself and the last part
        /// of a message is never lost silently.
        /// </summary>
        public int MaxLength { get; set; }

        public int TimeoutSeconds { get; set; }

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
            this.Host = "";
            this.Port = 514;
            this.UseTcp = false;
            this.UseRfc5424 = false;
            this.Hostname = "";
            this.AppName = "ServiceManager";
            this.Facility = 16;
            this.MaxLength = 1024;
            this.TimeoutSeconds = 5;
        }
    }
}
