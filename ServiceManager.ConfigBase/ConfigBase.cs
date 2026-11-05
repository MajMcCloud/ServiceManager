using Newtonsoft.Json;
using System;
using System.IO;

namespace ServiceManager.ConfigBase
{
    public class ConfigBase<T>
        where T : ConfigBase.ConfigBase<T>, new()
    {
        public static bool BeautifyOnSave = true;

        public static bool AutoReloadOnChange = false;

        public static String Filename { get; set; } = "default.cfg";

        public static String ConfigPath { get; set; } = AppContext.BaseDirectory + "config\\";

        [JsonIgnore]
        public FileSystemWatcher ConfigWatcher { get; set; }

        public ConfigBase()
        {
            this.loadDefaultValues();
        }

        public virtual void loadDefaultValues()
        {

        }

        public void Init()
        {
            if (AutoReloadOnChange)
            {
                ConfigWatcher = new FileSystemWatcher(ConfigPath);
                ConfigWatcher.Changed += ConfigWatcher_Changed;
                ConfigWatcher.EnableRaisingEvents = true;
            }
        }

        private void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != ConfigPath + Filename)
                return;

            Console.WriteLine("Config file changed");

            reload();
        }

        public static T load()
        {
            try
            {
                return load(ConfigPath + Filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if !DEBUG
                throw;
#endif
            }
            return null;
        }

        public static T load(String path)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<T>(ReadFile(path)) as T;

                cfg.Init();
                return cfg;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                DirectoryInfo di = new DirectoryInfo(path);

                if (!Directory.Exists(di.Parent.FullName))
                {
                    Directory.CreateDirectory(di.Parent.FullName);
                }

                var cfg = new T();
                cfg.loadDefaultValues();
                cfg.Init();
                cfg.save(path);
                return cfg as T;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                var cfg = new T();
                cfg.loadDefaultValues();
                cfg.Init();
                cfg.save(path);
                return cfg as T;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if !DEBUG
                throw;
#endif
            }
            return null;
        }

        public static T loadFromFile(String filename)
        {
            Filename = filename;

            return load();
        }

        private void reload()
        {
            try
            {
                String s = ReadFile(ConfigPath + Filename);

                // Integrate the new values into the current object
                var newCfg = JsonConvert.DeserializeObject<T>(s) as T;

                if (newCfg != null)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(this, property.GetValue(newCfg));
                        }
                    }
                }

                Console.WriteLine("reloaded!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static String ReadFile(String path)
        {
            String s = "";

            using (var inStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(inStream))
                {
                    s = sr.ReadToEnd();
                }
            }

            return s;
        }

        public void save()
        {
            try
            {
                save(ConfigPath + Filename);
            }
            catch
            {

            }
        }

        public void save(String path)
        {
            try
            {
                //Include all properties even when they are null
                var s = JsonConvert.SerializeObject(this, BeautifyOnSave ? Formatting.Indented : Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });

                File.WriteAllText(path, s);
            }
            catch
            {

            }
        }

        ~ConfigBase()
        {
            if (ConfigWatcher != null)
            {
                ConfigWatcher.EnableRaisingEvents = false;
                ConfigWatcher = null;
            }
        }
    }
}
