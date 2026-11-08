using System;
using System.IO;
using System.Reflection;

namespace ServiceManager.Extensions.ScriptNotifier.Config
{
    public class Config : ConfigBase.ConfigBase<Config>
    {
        /// <summary>
        /// The program to start. A full path is read as it is, a bare name is looked up in the path of
        /// the service, which is how "cmd.exe" or a tool of the network works.
        /// </summary>
        public String ScriptPath { get; set; }

        /// <summary>
        /// Written behind the program exactly as it stands here. The event itself is never part of it,
        /// so a line printed by a service cannot turn into an extra parameter.
        /// </summary>
        public String Arguments { get; set; }

        public String WorkingDirectory { get; set; }

        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// The event as one json object on the standard input of the program. The only way a console
        /// line of any length arrives whole and unchanged.
        /// </summary>
        public bool WriteJsonToStdin { get; set; }

        /// <summary>
        /// The event as one extra argument for a program that reads its arguments instead of its input.
        /// Quoted, but a batch file still cuts it at the first space, so stdin stays the default.
        /// </summary>
        public bool PassJsonAsArgument { get; set; }

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
            this.ScriptPath = "";
            this.Arguments = "";
            this.WorkingDirectory = "";
            this.TimeoutSeconds = 30;
            this.WriteJsonToStdin = true;
            this.PassJsonAsArgument = false;
        }
    }
}
