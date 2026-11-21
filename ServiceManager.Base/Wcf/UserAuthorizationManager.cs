using ServiceManager.Base.Data.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// Allows the users named in the settings, and while the rule is Users the administrators as well. An
    /// administrator never needs a password under that rule, so the local user interface and the installed
    /// service keep working over a named pipe, where nobody types one.
    /// </summary>
    public class UserAuthorizationManager : ServiceAuthorizationManager
    {
        /// <summary>The accounts that may use a password, read from the settings file.</summary>
        public List<EndpointUser> Users { get; private set; }

        /// <summary>
        /// True when an administrator is let in by its rights alone. The UsersOnly rule turns it off, so
        /// every account has to be listed and to name its password.
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

            return IsAllowed(operationContext.ServiceSecurityContext.WindowsIdentity, this.Users, EndpointCredential.ReadFrom(request), this.AllowAdministrators);
        }

        /// <summary>
        /// Tells whether the signed in account may use the endpoint, with its administrator rights or
        /// with the password of a configured user.
        /// </summary>
        public static bool IsAllowed(WindowsIdentity Identity, IEnumerable<EndpointUser> Users, EndpointCredential Credential)
        {
            return IsAllowed(Identity, Users, Credential, true);
        }

        /// <summary>
        /// Tells whether the signed in account may use the endpoint. <paramref name="AllowAdministrators"/>
        /// decides whether an administrator is let in without ever naming an account.
        /// </summary>
        public static bool IsAllowed(WindowsIdentity Identity, IEnumerable<EndpointUser> Users, EndpointCredential Credential, bool AllowAdministrators)
        {
            if (AllowAdministrators && AdministratorAuthorizationManager.IsAdministrator(Identity))
            {
                return true;
            }

            if (Credential == null || Users == null || String.IsNullOrEmpty(Credential.User))
            {
                return false;
            }

            EndpointUser user = Users.FirstOrDefault(a => a != null && a.MatchesName(Credential.User));

            if (user == null)
            {
                return false;
            }

            // The header names the account the password belongs to, and that account has to be the one
            // that signed in over the transport. Otherwise one password would open the endpoint for
            // every local user.
            if (Identity == null || !user.MatchesName(Identity.Name))
            {
                return false;
            }

            return user.CheckPassword(Credential.Password);
        }
    }
}
