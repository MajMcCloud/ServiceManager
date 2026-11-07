using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ServiceManager.Extensions.WebhookNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        /// <summary>The endpoint every event is posted to.</summary>
        public String Url { get; set; }

        /// <summary>POST for most receivers, some expect PUT or PATCH.</summary>
        public String Method { get; set; }

        public String ContentType { get; set; }

        /// <summary>
        /// The body as the user wants it. {text}, {details}, {kind}, {level}, {service}, {instance}
        /// and {timestamp} are replaced, everything else is sent unchanged.
        /// </summary>
        public String BodyTemplate { get; set; }

        /// <summary>Additional request headers, one per line as "Name: value".</summary>
        public List<String> Headers { get; set; }

        public int TimeoutSeconds { get; set; }

        /// <summary>How often a failed attempt is repeated.</summary>
        public int Retries { get; set; }

        /// <summary>
        /// Accepts an https endpoint with a self signed certificate. Only useful inside a trusted
        /// network, it also accepts a real attacker on that path.
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; }

        public static String FilePath
        {
            get
            {
                var file = Assembly.GetExecutingAssembly().Location;
                var path = Path.GetDirectoryName(file);

                return path + "\\settings.cfg";
            }
        }


        /// <summary>
        /// The body a receiver gets when the user did not write one. Written as plain text so the
        /// settings file stays readable and editable by hand.
        /// </summary>
        public const String DefaultBody =
            "{\r\n" +
            "  \"instance\": \"{instance}\",\r\n" +
            "  \"event\": \"{kind}\",\r\n" +
            "  \"level\": \"{level}\",\r\n" +
            "  \"service\": \"{service}\",\r\n" +
            "  \"time\": \"{timestamp}\",\r\n" +
            "  \"text\": \"{text}\",\r\n" +
            "  \"details\": \"{details}\"\r\n" +
            "}";


        public override void loadDefaultValues()
        {
            this.Url = "";
            this.Method = "POST";
            this.ContentType = "application/json";
            this.BodyTemplate = DefaultBody;
            this.Headers = new List<String>();
            this.TimeoutSeconds = 10;
            this.Retries = 1;
            this.IgnoreCertificateErrors = false;
        }
    }
}
