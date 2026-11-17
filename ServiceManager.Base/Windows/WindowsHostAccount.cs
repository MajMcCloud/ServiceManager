using System;
using System.Security.Principal;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// The account the own process runs under. The logon of a managed process behaves differently for
    /// LocalSystem, so the manager asks the token rather than guessing from the service installation.
    /// </summary>
    public static class WindowsHostAccount
    {
        private static readonly Lazy<bool> m_IsLocalSystem = new Lazy<bool>(readIsLocalSystem);

        /// <summary>
        /// True when the own process runs as LocalSystem. An account that cannot be read counts as an
        /// ordinary one, the operations of the manager do not change with the answer.
        /// </summary>
        public static bool IsLocalSystem()
        {
            return m_IsLocalSystem.Value;
        }

        private static bool readIsLocalSystem()
        {
            try
            {
                var user = WindowsIdentity.GetCurrent().User;

                if (user == null)
                {
                    return false;
                }

                var localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);

                return String.Equals(user.Value, localSystem.Value, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServiceManager: the own account could not be read: " + ex.Message);

                return false;
            }
        }
    }
}
