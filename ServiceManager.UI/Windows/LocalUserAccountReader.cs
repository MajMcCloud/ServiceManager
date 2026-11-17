using System;
using System.Collections.Generic;
using System.Management;

namespace ServiceManager.UI.Windows
{
    /// <summary>
    /// Reads the local accounts of this machine through Windows Management Instrumentation. The query is
    /// limited to local accounts because a process is started with a local logon.
    /// </summary>
    public class LocalUserAccountReader : IUserAccountReader
    {
        /// <summary>Lists the local accounts of this machine, sorted by name.</summary>
        public List<UserAccountInfo> ListAccounts()
        {
            List<UserAccountInfo> accounts = new List<UserAccountInfo>();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE LocalAccount = TRUE"))
            using (ManagementObjectCollection found = searcher.Get())
            {
                foreach (ManagementObject account in found)
                {
                    accounts.Add(toInfo(account));
                }
            }

            accounts.Sort(delegate(UserAccountInfo a, UserAccountInfo b)
            {
                return String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            return accounts;
        }

        static UserAccountInfo toInfo(ManagementObject account)
        {
            UserAccountInfo info = new UserAccountInfo();

            info.Name = text(account, "Name");
            info.Domain = String.Empty;
            info.FullName = text(account, "FullName");
            info.Description = text(account, "Description");
            info.Disabled = flag(account, "Disabled");

            return info;
        }

        static String text(ManagementObject account, String property)
        {
            object value = account[property];

            if (value == null)
            {
                return String.Empty;
            }

            return Convert.ToString(value);
        }

        static bool flag(ManagementObject account, String property)
        {
            object value = account[property];

            if (value == null)
            {
                return false;
            }

            return Convert.ToBoolean(value);
        }
    }
}
