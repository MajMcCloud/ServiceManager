using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ServiceManager.Base.Windows;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// Picks the Windows services a managed service has to wait for. The list shows the title of the service
    /// next to the name the dependency is stored under, so an entry can be recognised without knowing the
    /// internal names.
    /// </summary>
    public partial class FrmServicePicker : Form
    {
        readonly List<WindowsServiceInfo> m_Candidates;

        HashSet<String> m_Selected;

        List<WindowsServiceInfo> m_Shown;

        bool m_Populating;

        /// <summary>The services the user picked, by service name.</summary>
        public List<String> SelectedNames { get; private set; }

        public FrmServicePicker(IEnumerable<WindowsServiceInfo> candidates, IEnumerable<String> selected)
        {
            InitializeComponent();

            m_Candidates = (candidates ?? Enumerable.Empty<WindowsServiceInfo>()).ToList();
            m_Selected = new HashSet<String>(StringComparer.OrdinalIgnoreCase);
            m_Shown = new List<WindowsServiceInfo>();
            SelectedNames = new List<String>();

            if (selected != null)
            {
                foreach (String name in selected)
                {
                    AddSelected(name);
                }
            }
        }

        private void FrmServicePicker_Load(Object sender, EventArgs e)
        {
            fillList();

            txtFilter.Focus();
        }

        private void txtFilter_TextChanged(Object sender, EventArgs e)
        {
            fillList();
        }

        private void lsvServices_ItemChecked(Object sender, ItemCheckedEventArgs e)
        {
            if (m_Populating)
            {
                return;
            }

            WindowsServiceInfo info = e.Item.Tag as WindowsServiceInfo;

            if (info == null)
            {
                return;
            }

            if (e.Item.Checked)
            {
                m_Selected.Add(info.ServiceName);
            }
            else
            {
                m_Selected.Remove(info.ServiceName);
            }

            updateCount();
        }

        private void bnSelectAll_Click(Object sender, EventArgs e)
        {
            setChecked(true);
        }

        private void bnClear_Click(Object sender, EventArgs e)
        {
            setChecked(false);
        }

        private void bnOK_Click(Object sender, EventArgs e)
        {
            SelectedNames = m_Candidates.Where(m => m_Selected.Contains(m.ServiceName))
                                        .Select(m => m.ServiceName)
                                        .ToList();
        }

        /// <summary>
        /// A configured service the machine no longer offers stays in the list, marked as missing, so it can
        /// still be seen and removed instead of being dropped in silence.
        /// </summary>
        void AddSelected(String name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return;
            }

            String trimmed = name.Trim();

            m_Selected.Add(trimmed);

            if (!m_Candidates.Any(m => String.Equals(m.ServiceName, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                WindowsServiceInfo info = new WindowsServiceInfo();

                info.ServiceName = trimmed;
                info.DisplayName = trimmed;
                info.Status = "Not installed";
                info.StartType = WindowsServiceInfo.eStartType.Unknown;

                m_Candidates.Add(info);
            }
        }

        void setChecked(bool checkedState)
        {
            m_Populating = true;

            foreach (ListViewItem item in lsvServices.Items)
            {
                item.Checked = checkedState;
            }

            m_Populating = false;

            foreach (WindowsServiceInfo info in m_Shown)
            {
                if (checkedState)
                {
                    m_Selected.Add(info.ServiceName);
                }
                else
                {
                    m_Selected.Remove(info.ServiceName);
                }
            }

            updateCount();
        }

        void fillList()
        {
            m_Shown = m_Candidates.Where(m => WindowsServiceQuery.Matches(m, txtFilter.Text)).ToList();

            m_Populating = true;

            lsvServices.BeginUpdate();
            lsvServices.Items.Clear();

            foreach (WindowsServiceInfo info in m_Shown)
            {
                ListViewItem item = new ListViewItem(info.DisplayName);

                item.SubItems.Add(info.ServiceName);
                item.SubItems.Add(info.Status);
                item.SubItems.Add(WindowsServiceQuery.StartTypeText(info.StartType));
                item.Tag = info;
                item.Checked = m_Selected.Contains(info.ServiceName);

                lsvServices.Items.Add(item);
            }

            lsvServices.EndUpdate();

            m_Populating = false;

            updateCount();
        }

        void updateCount()
        {
            lblCount.Text = String.Format("{0} of {1} selected", m_Selected.Count, m_Candidates.Count);
        }
    }
}
