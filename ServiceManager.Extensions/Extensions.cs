using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Threading;

namespace ServiceManager.Extensions
{
    public static class Extensions
    {

        public static List<Extension> GetNotificationExtensions()
        {
            var lst = new List<Extension>();

            var path = ExtensionFolder;

            if (!Directory.Exists(path))
                return lst;

            var extensions = Directory.GetDirectories(path);
            Assembly asm;

            foreach (var extension in extensions)
            {
                var files = Directory.GetFiles(extension, "*.dll");

                foreach (var file in files)
                {
                    if (!file.Contains("ServiceManager.Extensions.TelegramNotifier"))
                        continue;

                    try
                    {
                        asm = Assembly.LoadFrom(file);

                        var list = asm.GetTypes().ToList();

                        var nt = asm.GetTypes().Where(a => a.GetInterfaces().Where(b => b == typeof(INotificationExtension)).Count() > 0).FirstOrDefault();
                        if (nt == null)
                            break;

                        var instance = nt.GetConstructor(new Type[] { }).Invoke(new object[] { }) as INotificationExtension;
                        if (instance == null)
                            break;

                        lst.Add(new Extension() { Id = instance.Id, Name = instance.Name, Path = file, Notification = instance });

                    }
                    catch
                    {


                    }
                    finally
                    {
                        asm = null;
                    }
                }
            }



            return lst;
        }

        public static Extension GetExtension(Guid? id)
        {
            var extensions = GetNotificationExtensions();

            var ext = extensions.FirstOrDefault(a => a.Id == id);

            return ext;
        }

        public static void SendNotification(Guid pluginId, String message)
        {
            var t = new Thread(() =>
            {
                try
                {
                    var task = GetExtension(pluginId)?.Notification?.Send(message);

                    task.GetAwaiter().GetResult();
                }
                catch
                {

                }

            });

            t.Start();
        }



        public static String ExtensionFolder
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "\\extensions\\";
            }
        }


    }
}
