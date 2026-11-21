using ServiceManager.Base.Data.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// Allows the accounts named in the settings, and while the rule is Users the administrators as well.
    /// Windows has already authenticated the account of the connection before this manager is asked, so
    /// the list only decides which of the authenticated accounts may use the endpoint.
    /// </summary>
    public class UserAuthorizationManager : ServiceAuthorizationManager
    {
        /// <summary>The accounts that may connect, read from the settings file.</summary>
        public List<EndpointUser> Users { get; private set; }

        /// <summary>
        /// True when an administrator is let in by its rights alone. The UsersOnly rule turns it off, so
        /// every account has to be listed.
        /// </summary>
        public bool AllowAdministrators { get; private set; }

        public UserAuthorizationManager()
            : this(null, true)
        {
        }

        public UserAuthorizationManager(IEnumerable<EndpointUser> Users)
            : this(Users, true)
        {
        }

        public UserAuthorizationManager(IEnumerable<EndpointUser> Users, bool AllowAdministrators)
        {
            // The manager lives as long as the host, so it keeps its own copy of the list.
            this.Users = Users == null ? new List<EndpointUser>() : new List<EndpointUser>(Users);

            this.AllowAdministrators = AllowAdministrators;
        }

        public override bool CheckAccess(OperationContext operationContext, ref Message request)
        {
            if (operationContext == null || operationContext.ServiceSecurityContext == null)
            {
                return false;
            }

            return IsAllowed(operationContext.ServiceSecurityContext.WindowsIdentity, this.Users, this.AllowAdministrators);
        }

        /// <summary>
        /// Tells whether the signed in account may use the endpoint, with its administrator rights or by
        /// being one of the configured accounts.
        /// </summary>
        public static bool IsAllowed(WindowsIdentity Identity, IEnumerable<EndpointUser> Users)
        {
            return IsAllowed(Identity, Users, true);
        }

        /// <summary>
        /// Tells whether the signed in account may use the endpoint. <paramref name="AllowAdministrators"/>
        /// decides whether an administrator is let in without ever naming an account.
        /// </summary>
        public static bool IsAllowed(WindowsIdentity Identity, IEnumerable<EndpointUser> Users, bool AllowAdministrators)
        {
            if (AllowAdministrators && AdministratorAuthorizationManager.IsAdministrator(Identity))
            {
                return true;
            }

            if (Identity == null || Users == null)
            {
                return false;
            }

            // A caller that is not on the list gets in under a rule with administrators at most. The name
            // comes from Windows, so nobody can claim to be somebody else, and a name written without a
            // domain is read as the account of this machine.
            return Users.Any(a => a != null && a.MatchesName(Identity.Name));
        }
    }
}
