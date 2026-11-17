using System;

namespace ServiceManager.UI.Windows
{
    /// <summary>
    /// An account of this machine that a managed process can be started under.
    /// </summary>
    public class UserAccountInfo
    {
        /// <summary>The account name the logon is done with.</summary>
        public String Name { get; set; }

        /// <summary>The domain of the account. Empty for a local account.</summary>
        public String Domain { get; set; }

        /// <summary>The name of the account holder, empty when the account does not carry one.</summary>
        public String FullName { get; set; }

        /// <summary>The description stored with the account.</summary>
        public String Description { get; set; }

        /// <summary>True when the account is disabled and cannot sign in.</summary>
        public bool Disabled { get; set; }
    }
}
