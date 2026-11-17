using System;
using System.Diagnostics;
using System.Security;

namespace ServiceManager.Base.Data
{
    /// <summary>
    /// The account a managed process is started with, read from the settings of a service. The manager and the
    /// tests look at the same values, so every mode can be checked without starting a process.
    /// </summary>
    public class RunAsAccount
    {
        private RunAsAccount()
        {
        }

        /// <summary>The mode the settings of the service ask for.</summary>
        public ServiceItem.eRunAs Mode { get; private set; }

        /// <summary>The account name without the domain, empty when the account is inherited.</summary>
        public String UserName { get; private set; }

        /// <summary>The domain of the account, empty for a local account.</summary>
        public String Domain { get; private set; }

        /// <summary>The password the logon is made with, empty when none was stored.</summary>
        public String Password { get; private set; }

        /// <summary>Why the account cannot be used, null when the settings are complete.</summary>
        public String Error { get; private set; }

        /// <summary>True when the process runs under an account of its own rather than the manager's.</summary>
        public bool UsesLogon
        {
            get { return Mode == ServiceItem.eRunAs.User; }
        }

        /// <summary>The account as a person reads it, for the log and the messages.</summary>
        public String FullName
        {
            get
            {
                if (String.IsNullOrEmpty(Domain))
                {
                    return UserName;
                }

                return Domain + "\\" + UserName;
            }
        }

        /// <summary>
        /// Reads the account of a service. The stored password is decrypted here; the value is only ever handed
        /// to the operating system and never written back.
        /// </summary>
        public static RunAsAccount Of(ServiceItem item)
        {
            RunAsAccount account = new RunAsAccount();

            account.Mode = item != null ? item.RunAs : ServiceItem.eRunAs.Inherit;
            account.UserName = trimmed(item != null ? item.RunAsUser : null);
            account.Domain = trimmed(item != null ? item.RunAsDomain : null);
            account.Password = String.Empty;

            if (!account.UsesLogon)
            {
                return account;
            }

            //A name typed as DOMAIN\user carries its own domain, the logon wants the two parts apart
            if (String.IsNullOrEmpty(account.Domain))
            {
                var parts = account.UserName.Split(new char[] { '\\' }, 2);

                if (parts.Length == 2)
                {
                    account.Domain = parts[0];
                    account.UserName = parts[1];
                }
            }

            if (String.IsNullOrEmpty(account.UserName))
            {
                account.Error = "NO ACCOUNT NAME FOR THE SERVICE";

                return account;
            }

            try
            {
                account.Password = SecretStorage.Unprotect(item.RunAsPassword) ?? String.Empty;
            }
            catch (Exception)
            {
                //A secret sealed on another machine, or one that was edited by hand, cannot be read here
                account.Error = "THE STORED PASSWORD CANNOT BE READ ON THIS MACHINE";
            }

            return account;
        }

        /// <summary>The line that lands in the error stream of the service, null when the account is usable.</summary>
        public String ErrorLine()
        {
            if (Error == null)
            {
                return null;
            }

            return "SERVICE MANAGER: " + Error + "\r\n";
        }

        /// <summary>
        /// Puts the account on the start information of the process. An inherited account leaves it alone, which
        /// is what makes the process run under the account of the ServiceManager.
        /// </summary>
        public void Apply(ProcessStartInfo startInfo)
        {
            if (startInfo == null || !UsesLogon || Error != null)
            {
                return;
            }

            startInfo.UserName = UserName;

            if (!String.IsNullOrEmpty(Domain))
            {
                startInfo.Domain = Domain;
            }

            startInfo.Password = ToSecure(Password);

            //A program started under another account expects its own profile folders and its own HKCU
            startInfo.LoadUserProfile = true;
        }

        private static String trimmed(String value)
        {
            return value != null ? value.Trim() : String.Empty;
        }

        private static SecureString ToSecure(String value)
        {
            SecureString secure = new SecureString();

            if (!String.IsNullOrEmpty(value))
            {
                foreach (char c in value)
                {
                    secure.AppendChar(c);
                }
            }

            secure.MakeReadOnly();

            return secure;
        }
    }
}
