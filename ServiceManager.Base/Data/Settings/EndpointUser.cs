using System;
using System.Security.Cryptography;

namespace ServiceManager.Base.Data.Settings
{
    /// <summary>
    /// A user that may connect to the endpoint with a password. The account has to exist on the
    /// machine of the service, the password is the extra secret ServiceManager asks for.
    /// </summary>
    public class EndpointUser
    {
        /// <summary>
        /// The Windows account of the user, with or without its domain.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The password as it sits in the settings file: sealed with this machine's DPAPI key.
        /// </summary>
        public String Password { get; set; }

        public EndpointUser()
        {
        }

        public EndpointUser(String Name, String Password)
        {
            this.Name = Name;
            this.Password = Password;
        }

        /// <summary>
        /// True when the name means the same account as this user, so DOMAIN\user, user@domain and
        /// plain user all match each other.
        /// </summary>
        public bool MatchesName(String Account)
        {
            return MatchesName(this.Name, Account);
        }

        /// <summary>
        /// True when both names mean the same account.
        /// </summary>
        public static bool MatchesName(String Left, String Right)
        {
            if (String.IsNullOrEmpty(Left) || String.IsNullOrEmpty(Right))
            {
                return false;
            }

            if (String.Equals(Left, Right, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return String.Equals(AccountName(Left), AccountName(Right), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// True when the typed password is the one stored for this user.
        /// </summary>
        public bool CheckPassword(String Password)
        {
            if (String.IsNullOrEmpty(Password) || String.IsNullOrEmpty(this.Password))
            {
                return false;
            }

            try
            {
                return String.Equals(SecretStorage.Unprotect(this.Password), Password, StringComparison.Ordinal);
            }
            catch (Exception)
            {
                // A secret that cannot be read on this machine never matches.
                return false;
            }
        }

        // The account part of DOMAIN\user or user@domain.example, which is what both spellings share.
        private static String AccountName(String Account)
        {
            int backslash = Account.IndexOf('\\');

            if (backslash >= 0)
            {
                return Account.Substring(backslash + 1);
            }

            int at = Account.IndexOf('@');

            if (at >= 0)
            {
                return Account.Substring(0, at);
            }

            return Account;
        }
    }
}
