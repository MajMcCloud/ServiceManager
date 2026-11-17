using System.Collections.Generic;

namespace ServiceManager.UI.Windows
{
    /// <summary>
    /// Reads the accounts a managed process can be started under. The interface keeps the machine query
    /// out of the forms, so the dialogs do not need a machine to be filled.
    /// </summary>
    public interface IUserAccountReader
    {
        /// <summary>Lists the accounts of this machine, sorted by name.</summary>
        List<UserAccountInfo> ListAccounts();
    }
}
