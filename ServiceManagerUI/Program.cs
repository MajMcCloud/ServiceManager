using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.Themes;

namespace ServiceManagerUI
{
    static class Program
    {
        public static String ServiceName { get { return "ServiceManager"; } }

        public static String ServiceTitle { get { return "Service Manager"; } }


        public static Dictionary<String, String> Parameters = new Dictionary<String, String>();

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            for (int i = 0; i < args.Length; i++)
            {

                switch (args[i].ToLower())
                {
                    case "-c":

                        if (args.Length <= i)
                            continue;

                        Parameters.Add("-c", "");                       

                        Parameters.Add("host", args[i + 1].Split(':')[0]);

                        Parameters.Add("port", args[i + 1].Split(':')[1]);

                        Thread.Sleep(5000);

                        break;
                }

            }

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            var th = new FluentTheme();
            Telerik.WinControls.ThemeResolutionService.ApplicationThemeName = th.ThemeName;

            Application.Run(new frmMain());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null)
                return;


            try
            {
                String exl = CollectionException(ex, 0);

                System.IO.File.AppendAllText(Application.StartupPath + "\\errors.txt", exl);

            }
            catch
            {

            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {

            Exception ex = e.Exception;
            if (ex == null)
                return;





            try
            {
                String exl = CollectionException(ex, 0);


                System.IO.File.AppendAllText(Application.StartupPath + "\\errors_threads.txt", exl);

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
