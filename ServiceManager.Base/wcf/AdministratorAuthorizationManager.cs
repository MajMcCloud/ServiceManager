using System;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ServiceManager.Base.wcf
{
    /// <summary>
    /// Allows members of the local Administrators group only. Both transports already sign in
    /// with Windows over the transport, what was missing is who may use the connection.
    /// </summary>
    public class AdministratorAuthorizationManager : ServiceAuthorizationManager
    {
        public override bool CheckAccess(OperationContext operationContext, ref Message request)
        {
            if (operationContext == null || operationContext.ServiceSecurityContext == null)
            {
                return false;
            }

            return IsAdministrator(operationContext.ServiceSecurityContext.WindowsIdentity);
        }

        /// <summary>
        /// Tells whether the signed in caller is an administrator.
        /// </summary>
        public static bool IsAdministrator(WindowsIdentity Identity)
        {
            if (Identity == null)
            {
                return false;
            }

            try
            {
                return new WindowsPrincipal(Identity).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                // An identity that cannot be examined is not an administrator.
                return false;
            }
        }
    }
}
