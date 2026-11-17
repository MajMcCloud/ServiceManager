using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions
{
    public  class Extension
    {
        public String Name { get; set; }

        public Guid Id { get; set; }

        public String Path { get; set; }    

        public INotificationExtension Notification { get; set; }

        /// <summary>
        /// Set when the assembly also offers INotificationMessageExtension. Older extensions leave it
        /// null and keep receiving the plain text.
        /// </summary>
        public INotificationMessageExtension MessageNotification { get; set; }

        /// <summary>
        /// Set when the assembly also offers INotificationSettingsCheck. Extensions without it are
        /// treated as configured.
        /// </summary>
        public INotificationSettingsCheck SettingsCheck { get; set; }

    }
}
