using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.TelegramNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        public String APIKey { get; set; }


        public long ChatId { get; set; }

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
            this.APIKey = "";
            this.ChatId = 0;
        }
    }
}
