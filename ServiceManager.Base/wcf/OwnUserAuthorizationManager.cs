using System;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceManager.Base.wcf
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

        public override bool CheckAccess(OperationContext operationContext, ref Message request)
        {
            if (operationContext == null || operationContext.ServiceSecurityContext == null)
            {
                return false;
            }

            return IsOwnUser(operationContext.ServiceSecurityContext.WindowsIdentity);
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
