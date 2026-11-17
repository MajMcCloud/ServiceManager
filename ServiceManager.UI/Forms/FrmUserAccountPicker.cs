using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ServiceManager.UI.Windows;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// Picks the local account a managed process runs under. Disabled accounts stay in the list and are
    /// marked as disabled, so the reason a logon fails can be seen instead of an account quietly missing.
    /// </summary>
    public partial class FrmUserAccountPicker : Form
    {
        readonly List<UserAccountInfo> m_Accounts;

        readonly String m_PreSelected;

        /// <summary>The account the user picked. Null when the dialog was closed without a choice.</summary>
        public UserAccountInfo Selected { get; private set; }

        public FrmUserAccountPicker(IEnumerable<UserAccountInfo> accounts, String selected)
        {
            InitializeComponent();

            m_Accounts = (accounts ?? Enumerable.Empty<UserAccountInfo>()).ToList();
            m_PreSelected = (selected ?? String.Empty).Trim();
            Selected = null;
        }

        private void FrmUserAccountPicker_Load(Object sender, EventArgs e)
        {
            fillList();

            txtFilter.Focus();
        }

        private void txtFilter_TextChanged(Object sender, EventArgs e)
        {
            fillList();
        }

        private void lsvAccounts_MouseDoubleClick(Object sender, MouseEventArgs e)
        {
            if (lsvAccounts.SelectedItems.Count > 0)
            {
                bnOK_Click(sender, e);
            }
        }

        private void bnOK_Click(Object sender, EventArgs e)
        {
            UserAccountInfo info = selectedAccount();

            if (info == null)
            {
                return;
            }

            Selected = info;
            this.DialogResult = DialogResult.OK;
        }

        UserAccountInfo selectedAccount()
        {
            if (lsvAccounts.SelectedItems.Count == 0)
            {
                return null;
            }

            return lsvAccounts.SelectedItems[0].Tag as UserAccountInfo;
        }

        void fillList()
        {
            List<UserAccountInfo> shown = m_Accounts.Where(m => matches(m, txtFilter.Text)).ToList();

            lsvAccounts.BeginUpdate();
            lsvAccounts.Items.Clear();

            foreach (UserAccountInfo info in shown)
            {
                ListViewItem item = new ListViewItem(info.Name);

                item.SubItems.Add(info.FullName);
                item.SubItems.Add(info.Description);
                item.SubItems.Add(info.Disabled ? "Disabled" : String.Empty);
                item.Tag = info;

                lsvAccounts.Items.Add(item);

                if (String.Equals(info.Name, m_PreSelected, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                }
            }

            lsvAccounts.EndUpdate();

            lblCount.Text = String.Format("{0} of {1} accounts", shown.Count, m_Accounts.Count);
        }

        static bool matches(UserAccountInfo info, String filter)
        {
            if (String.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            String needle = filter.Trim();

            return info.Name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                || info.FullName.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                || info.Description.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
