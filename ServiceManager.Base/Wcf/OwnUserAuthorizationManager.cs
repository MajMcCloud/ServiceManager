using CoreWCF;
using CoreWCF.Channels;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// Allows the account that runs the service and nobody else. A test run inside the UI hosts
    /// the service in its own process, and that process is usually not elevated, so the
    /// Administrators rule would refuse the very application that started the service.
    /// </summary>
    public class OwnUserAuthorizationManager : ServiceAuthorizationManager
    {
        private SecurityIdentifier OwnSid { get; set; }

        public OwnUserAuthorizationManager()
        {
            using (WindowsIdentity current = WindowsIdentity.GetCurrent())
            {
                OwnSid = current.User;
            }
        }

        public override ValueTask<(bool, Message)> CheckAccessAsync(OperationContext operationContext, Message request)
        {
            if (operationContext == null || operationContext.ServiceSecurityContext == null)
            {
                return new ValueTask<(bool, Message)>((false, request));
            }

            bool allowed = IsOwnUser(operationContext.ServiceSecurityContext.WindowsIdentity);

            return new ValueTask<(bool, Message)>((allowed, request));
        }

        /// <summary>
        /// Tells whether the signed in caller is the account that runs the service.
        /// </summary>
        public bool IsOwnUser(WindowsIdentity Identity)
        {
            if (Identity == null || this.OwnSid == null)
            {
                return false;
            }

            try
            {
                return Identity.User != null && OwnSid.Equals(Identity.User);
            }
            catch (Exception)
            {
                // An identity that cannot be examined is not the own account.
                return false;
            }
        }
    }
}
