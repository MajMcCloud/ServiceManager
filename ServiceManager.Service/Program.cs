using ServiceManager.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager
{
    class Program
    {
        public static String ServiceName { get { return "ServiceManager"; } }

        public static String ServiceTitle { get { return "Service Manager"; } }

        public static Dictionary<String, String> Parameters = new Dictionary<String, String>();

        public static ManagerServiceBase Service { get; set; }

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {

                switch (args[i].ToLower())
                {
                    case "-s":

                        Parameters.Add("-s", "");



                        break;
                }

            }


            //Normale Windows Anwendung
            if (Parameters.ContainsKey("-s"))
            {
                ServiceBase[] ServicesToRun;
                Service = new ManagerServiceBase();
                ServicesToRun = new ServiceBase[]
                {
                    Service
                };

                ServiceBase.Run(ServicesToRun);

                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null)
                return;





            try
            {
                String exl = CollectionException(ex, 0);

                System.IO.File.AppendAllText(AppContext.BaseDirectory + "\\errors.txt", exl);

            }
            catch
            {

            }
        }

       

        private static String CollectionException(Exception ex, int level)
        {
            String s = "";

            s += $"Date: {DateTime.Now.ToString()}\r\n";
            s += $"Level: {level}\r\n\r\n";
            s += "Message: " + ex.Message + "\r\n\r\n";
            s += "Stacktrace:\r\n\r\n" + ex.StackTrace + "\r\n\r\n\r\n";

            if (ex.InnerException != null)
                s += CollectionException(ex.InnerException, level + 1);

            s += "".PadRight(30, '#');

            return s;
        }
    }
}
