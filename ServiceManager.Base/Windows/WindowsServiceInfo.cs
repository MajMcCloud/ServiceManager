using System;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// One Windows service as a list shows it. The display name is the title a person reads, the service
    /// name is what the service control manager stores in a dependency list.
    /// </summary>
    public class WindowsServiceInfo
    {
        /// <summary>How the service control manager starts the service.</summary>
        public enum eStartType
        {
            /// <summary>The machine did not answer, so nothing is known about it.</summary>
            Unknown = 0,

            /// <summary>Loaded by the kernel while the machine boots, the manager never starts it.</summary>
            Boot = 1,

            /// <summary>Loaded by the kernel during the boot, still not something the manager starts.</summary>
            System = 2,

            Automatic = 3,

            Manual = 4,

            Disabled = 5
        }

        /// <summary>The name the service control manager works with.</summary>
        public String ServiceName { get; set; }

        /// <summary>The title of the service, the service name when the machine has no title.</summary>
        public String DisplayName { get; set; }

        /// <summary>The state as English text, empty when the state is not known.</summary>
        public String Status { get; set; }

        public eStartType StartType { get; set; }

        public WindowsServiceInfo()
        {

        }
    }
}
