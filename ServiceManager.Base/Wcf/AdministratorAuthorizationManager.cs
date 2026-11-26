using CoreWCF;
using CoreWCF.Channels;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ServiceManager.Base.Wcf
{
    /// <summary>
    /// Allows members of the local Administrators group only. Both transports already sign in
    /// with Windows over the transport, what was missing is who may use the connection.
    /// </summary>
    public class AdministratorAuthorizationManager : ServiceAuthorizationManager
    {
        public override ValueTask<(bool, Message)> CheckAccessAsync(OperationContext operationContext, Message request)
        {
            if (operationContext == null || operationContext.ServiceSecurityContext == null)
            {
                return new ValueTask<(bool, Message)>((false, request));
            }

            bool allowed = IsAdministrator(operationContext.ServiceSecurityContext.WindowsIdentity);

            return new ValueTask<(bool, Message)>((allowed, request));
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
