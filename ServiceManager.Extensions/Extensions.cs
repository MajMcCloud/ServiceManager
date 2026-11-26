using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;
using System.Threading;

namespace ServiceManager.Extensions
{
    public static class Extensions
    {

        private static readonly Object m_Sync = new Object();

        private static readonly Dictionary<String, Extension> m_Cache = new Dictionary<String, Extension>(StringComparer.OrdinalIgnoreCase);

        private static String m_ExtensionFolder;


        /// <summary>
        /// The instances are kept, so an extension is built once per process instead of once per call:
        /// GetExtension runs on every notification and TelegramNotifier reads and writes its settings file
        /// in its constructor. Added and removed files are still picked up on the next call.
        /// </summary>
        public static List<Extension> GetNotificationExtensions()
        {
            var lst = new List<Extension>();

            var path = ExtensionFolder;

            if (!Directory.Exists(path))
                return lst;

            lock (m_Sync)
            {
                var present = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
                {
                    var full = Path.GetFullPath(file);

                    present.Add(full);

                    Extension cached;

                    if (m_Cache.TryGetValue(full, out cached))
                    {
                        lst.Add(cached);
                        continue;
                    }

                    var extension = LoadExtension(full);

                    if (extension == null)
                        continue;

                    if (lst.Any(a => a.Id == extension.Id))
                    {
                        SMLog.Warning($"Extension \"{extension.Name}\" skipped: the Id {extension.Id} is already registered");
                        continue;
                    }

                    m_Cache[full] = extension;

                    lst.Add(extension);
                }

                foreach (var missing in m_Cache.Keys.Where(a => !present.Contains(a)).ToList())
                    m_Cache.Remove(missing);
            }

            return lst;
        }


        /// <summary>
        /// Forgets every cached instance. An extension keeps its settings in memory, so this has to run
        /// after its settings were changed. A replaced assembly is not picked up either way, a loaded
        /// assembly stays in its load context for the lifetime of the process.
        /// </summary>
        public static void Reload()
        {
            lock (m_Sync)
            {
                m_Cache.Clear();
            }
        }


        /// <summary>
        /// Loads a single assembly and returns the extension it offers, or null when the assembly is
        /// not an extension. A broken assembly only costs itself, never the assemblies behind it.
        /// </summary>
        private static Extension LoadExtension(String file)
        {
            AssemblyName name;

            try
            {
                name = AssemblyName.GetAssemblyName(file);
            }
            catch (BadImageFormatException)
            {
                return null;
            }
            catch (Exception ex)
            {
                SMLog.Error($"Extension \"{file}\" could not be read", ex);
                return null;
            }

            // An extension folder ships copies of the host assemblies. Loading one of those copies a
            // second time makes the extension bind its INotificationExtension to the copy instead of
            // to the running host, and the extension can no longer be used. The extension itself is
            // loaded on every scan, so only a copy under a different path may be skipped here.
            var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == name.FullName);

            if (loaded != null && !IsSameFile(loaded.Location, file))
                return null;

            Assembly asm;

            try
            {
                asm = new ExtensionLoadContext(Path.GetDirectoryName(file)).LoadFromAssemblyPath(file);
            }
            catch (Exception ex)
            {
                SMLog.Error($"Extension \"{file}\" could not be loaded", ex);
                return null;
            }

            try
            {
                var type = GetTypes(asm).FirstOrDefault(IsNotificationExtension);
                if (type == null)
                    return null;

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    SMLog.Warning($"Extension \"{file}\" skipped: {type.FullName} has no public parameterless constructor");
                    return null;
                }

                var instance = Activator.CreateInstance(type) as INotificationExtension;
                if (instance == null)
                {
                    SMLog.Warning($"Extension \"{file}\" skipped: {type.FullName} implements the INotificationExtension of another ServiceManager.Extensions.dll");
                    return null;
                }

                return new Extension()
                {
                    Id = instance.Id,
                    Name = instance.Name,
                    Path = file,
                    Notification = instance,
                    MessageNotification = instance as INotificationMessageExtension,
                    SettingsCheck = instance as INotificationSettingsCheck
                };
            }
            catch (Exception ex)
            {
                SMLog.Error($"Extension \"{file}\" skipped", ex);
                return null;
            }
        }


        private static IEnumerable<Type> GetTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(a => a != null);
            }
        }


        /// <summary>
        /// Matched on the full name on purpose: an extension folder is allowed to ship its own copy of
        /// ServiceManager.Extensions.dll, and a reference comparison would fail for exactly that copy.
        /// </summary>
        private static bool IsNotificationExtension(Type type)
        {
            return type != null
                && type.IsClass
                && !type.IsAbstract
                && type.IsPublic
                && type.GetInterfaces().Any(a => a.FullName == typeof(INotificationExtension).FullName);
        }


        /// <summary>
        /// Compares two assembly paths. ExtensionFolder carries a doubled separator and GetFiles keeps
        /// it, while Assembly.Location is normalized, so both sides have to be normalized first.
        /// </summary>
        private static bool IsSameFile(String location, String file)
        {
            if (String.IsNullOrEmpty(location) || String.IsNullOrEmpty(file))
                return false;

            try
            {
                return String.Equals(Path.GetFullPath(location), Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// A provider ships its dependencies inside its own folder under extensions\, while the default
        /// loader only knows the folder of the host. Every extension is therefore read through a context
        /// of its own that answers from the folder of the provider.
        /// </summary>
        private sealed class ExtensionLoadContext : AssemblyLoadContext
        {
            private readonly String m_Folder;


            public ExtensionLoadContext(String folder)
                : base("extensions:" + Path.GetFileName(folder))
            {
                m_Folder = folder;
            }


            /// <summary>
            /// Returning null hands the request to the default context, which is what the host assemblies
            /// and the runtime need: an extension has to implement the interfaces of the running process
            /// rather than a copy of ServiceManager.Extensions.dll from its own folder.
            /// </summary>
            protected override Assembly Load(AssemblyName assemblyName)
            {
                var name = assemblyName?.Name;

                if (String.IsNullOrEmpty(name))
                {
                    return null;
                }

                if (name.StartsWith("ServiceManager", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(name, "mscorlib", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(name, "netstandard", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var candidate = Path.Combine(m_Folder, name + ".dll");

                if (!File.Exists(candidate))
                {
                    return null;
                }

                return LoadFromAssemblyPath(candidate);
            }
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
                catch (Exception ex)
                {
                    SMLog.Error($"Notification \"{pluginId}\" failed", ex);
                }

            });

            t.Start();
        }


        /// <summary>
        /// Hands one event to every provider that was selected for it. The extensions are resolved
        /// once per event instead of once per provider, and a provider that fails only costs itself.
        ///
        /// The call runs on the thread of the caller: the event bus keeps a thread of its own for the
        /// providers, so a slow provider holds up the other providers but never the producer.
        ///
        /// A provider compiled against the old contract never sees the structured message, it keeps
        /// receiving exactly the text it used to get.
        /// </summary>
        public static void SendNotification(IEnumerable<Guid> pluginIds, NotificationMessage message)
        {
            if (message == null || pluginIds == null)
                return;

            var ids = pluginIds.Distinct().ToList();

            if (ids.Count == 0)
                return;

            var extensions = GetNotificationExtensions();

            foreach (var id in ids)
            {
                Extension extension = null;

                try
                {
                    extension = extensions.FirstOrDefault(a => a.Id == id);

                    if (extension == null)
                        continue;

                    var task = extension.MessageNotification != null
                        ? extension.MessageNotification.Send(message)
                        : extension.Notification?.Send(message.Text);

                    task?.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var name = extension != null ? extension.Name : id.ToString();

                    SMLog.Error($"Notification \"{name}\" failed", ex);
                }
            }
        }



        /// <summary>
        /// Where the extensions sit without any override: next to the executable, which is why the
        /// service is installed as one folder.
        /// </summary>
        public static String DefaultExtensionFolder
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions");
            }
        }

        /// <summary>
        /// The folder the loader reads. A test run points it at a temporary directory, because the
        /// folder of the test host is write protected and the fixture assemblies are written into it.
        /// </summary>
        public static String ExtensionFolder
        {
            get
            {
                return m_ExtensionFolder ?? DefaultExtensionFolder;
            }
            set
            {
                lock (m_Sync)
                {
                    m_ExtensionFolder = value;

                    m_Cache.Clear();
                }
            }
        }


    }
}
