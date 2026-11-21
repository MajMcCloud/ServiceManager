using System;

namespace ServiceManager.Base.Data.Settings
{
    /// <summary>
    /// A Windows account that may connect to the endpoint. The account has to exist on the machine of
    /// the service: the endpoint judges its caller by the identity Windows authenticates over the
    /// transport, so a listed account needs no secret of its own.
    /// </summary>
    public class EndpointUser
    {
        /// <summary>
        /// The Windows account of the user. Without a domain it stands for the local account of this
        /// machine, with one it stands for that domain or machine and nothing else.
        /// </summary>
        public String Name { get; set; }

        public EndpointUser()
        {
        }

        public EndpointUser(String Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// True when the name means the same account as this user.
        /// </summary>
        public bool MatchesName(String Account)
        {
            return MatchesName(this.Name, Account);
        }

        /// <summary>
        /// True when both names mean the same account. DOMAIN\user and user@DOMAIN are the two
        /// spellings of one account, but the domain has to be the one Windows names that account under:
        /// the like-named account of another machine or domain stays outside the list. A name without a
        /// domain is the account of this machine.
        /// </summary>
        public static bool MatchesName(String Left, String Right)
        {
            if (String.IsNullOrEmpty(Left) || String.IsNullOrEmpty(Right))
            {
                return false;
            }

            Left = Left.Trim();
            Right = Right.Trim();

            if (String.Equals(Left, Right, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!String.Equals(AccountOf(Left), AccountOf(Right), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            String leftDomain = DomainOf(Left);
            String rightDomain = DomainOf(Right);

            if (leftDomain == null || rightDomain == null)
            {
                // The side that names no domain is this machine, so the other one has to be it
                return String.Equals(leftDomain ?? rightDomain, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
            }

            return String.Equals(leftDomain, rightDomain, StringComparison.OrdinalIgnoreCase);
        }

        // The account of DOMAIN\user or user@DOMAIN, which is what both spellings share.
        private static String AccountOf(String Account)
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

        // The machine or the domain in front of the account, null when the name carries neither.
        private static String DomainOf(String Account)
        {
            int backslash = Account.IndexOf('\\');

            if (backslash >= 0)
            {
                return Account.Substring(0, backslash);
            }

            int at = Account.IndexOf('@');

            if (at >= 0)
            {
                return Account.Substring(at + 1);
            }

            return null;
        }
    }
}
