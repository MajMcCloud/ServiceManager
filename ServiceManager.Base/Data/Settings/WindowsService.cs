using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceManager.Base.Data.Settings
{
    /// <summary>
    /// How the ServiceManager itself is registered in the service control manager. The installer
    /// writes these values, and a running installation is changed to them again after an edit.
    /// </summary>
    public class WindowsService
    {
        /// <summary>The startup type, the four choices services.msc offers.</summary>
        public enum eStartMode
        {
            Automatic = 0,
            AutomaticDelayed = 1,
            Manual = 2,
            Disabled = 3
        }

        /// <summary>The account the service process logs on with.</summary>
        public enum eAccount
        {
            LocalSystem = 0,
            LocalService = 1,
            NetworkService = 2,

            /// <summary>A named account, its password sits protected in the settings file.</summary>
            User = 3
        }

        /// <summary>Where the list of service dependencies comes from.</summary>
        public enum eDependencyMode
        {
            None = 0,

            /// <summary>The network stack, the set every installation so far has used.</summary>
            StandardNetwork = 1,

            Custom = 2
        }

        /// <summary>
        /// The dependencies of every release up to the settings dialog. They are named here so the
        /// dialog can show them instead of hiding the set behind a preset.
        /// </summary>
        public static readonly String[] StandardNetworkDependencies = new String[] { "Tcpip", "Dhcp", "Dnscache" };

        public eStartMode StartMode { get; set; } = eStartMode.Automatic;

        public eAccount Account { get; set; } = eAccount.LocalSystem;

        /// <summary>The account name as the service control manager expects it, only used for User.</summary>
        public String UserName { get; set; }

        /// <summary>The password of the named account, protected by <see cref="SecretStorage"/>.</summary>
        public String Password { get; set; }

        public eDependencyMode DependenciesMode { get; set; } = eDependencyMode.StandardNetwork;

        public List<String> CustomDependencies { get; set; } = new List<String>();

        public WindowsService()
        {

        }

        /// <summary>
        /// The services the service control manager has to start first. Empty means the service comes
        /// up on its own, which is what a machine without a network stack needs.
        /// </summary>
        public String[] GetDependencies()
        {
            switch (DependenciesMode)
            {
                case eDependencyMode.None:

                    return new String[0];

                case eDependencyMode.Custom:

                    if (CustomDependencies == null)
                    {
                        return new String[0];
                    }

                    return CustomDependencies
                        .Where(a => !String.IsNullOrWhiteSpace(a))
                        .Select(a => a.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                case eDependencyMode.StandardNetwork:

                    return StandardNetworkDependencies.ToArray();

                default:

                    throw new ArgumentOutOfRangeException(nameof(DependenciesMode));
            }
        }

        /// <summary>
        /// The name the service control manager logs the service on with. Null for a named account
        /// without a name, so a half filled form is never handed to the service control manager.
        /// </summary>
        public String GetAccountName()
        {
            switch (Account)
            {
                case eAccount.LocalService:

                    return "NT AUTHORITY\\LocalService";

                case eAccount.NetworkService:

                    return "NT AUTHORITY\\NetworkService";

                case eAccount.User:

                    if (String.IsNullOrWhiteSpace(UserName))
                    {
                        return null;
                    }

                    return UserName.Trim();

                case eAccount.LocalSystem:

                    return "LocalSystem";

                default:

                    throw new ArgumentOutOfRangeException(nameof(Account));
            }
        }

        /// <summary>True when the account needs a password the settings file has to carry.</summary>
        public bool UsesPassword()
        {
            return Account == eAccount.User;
        }
    }
}
