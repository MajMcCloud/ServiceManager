using Newtonsoft.Json;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Base.Data.Settings
{
    public  class Notifications
    {
        public String InstanceName { get; set; } = "Service Manager";


        /// <summary>
        /// The active providers, each with its own event selection. Empty means notifications are off.
        /// </summary>
        public List<NotificationTarget> Targets { get; set; } = new List<NotificationTarget>();


        // Everything below PluginId belongs to the single provider setup of older settings files. It
        // is read by Migrate() and never written again, see the test that fixes the migration.
        public Guid? PluginId { get; set; }

        public bool OnRestart { get; set; }

        public bool OnStart { get; set; }

        public bool OnStop { get; set; }

        public bool OnConsoleOutput { get; set; }

        public bool OnConsoleError { get; set; }


        public bool OnServerStart { get; set; }

        public bool OnServerShutdown { get; set; }


        /// <summary>
        /// Turns the single provider of an older settings file into a target. Runs straight after
        /// deserialization, so the UI and the service both work on Targets without knowing that an
        /// upgrade happened.
        /// </summary>
        public void Migrate()
        {
            if (Targets == null)
                Targets = new List<NotificationTarget>();

            if (Targets.Count > 0 || PluginId == null)
                return;

            Targets.Add(new NotificationTarget()
            {
                PluginId = PluginId.Value,

                OnStart = OnStart,
                OnStop = OnStop,
                OnRestart = OnRestart,
                OnConsoleOutput = OnConsoleOutput,
                OnConsoleError = OnConsoleError,
                OnServerStart = OnServerStart,
                OnServerShutdown = OnServerShutdown
            });

            PluginId = null;

            OnStart = OnStop = OnRestart = false;
            OnConsoleOutput = OnConsoleError = false;
            OnServerStart = OnServerShutdown = false;
        }


        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Migrate();
        }


        /// <summary>The providers that asked for this kind of event.</summary>
        public List<Guid> GetProviders(NotificationKind kind)
        {
            if (Targets == null)
                return new List<Guid>();

            return Targets.Where(a => a.IsEnabled(kind)).Select(a => a.PluginId).ToList();
        }


    }
}

