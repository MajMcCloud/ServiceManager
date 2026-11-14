using ServiceManager.Base.Data;
using ServiceManager.Base.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    public partial class FrmNewService : Form
    {

        public ServiceItem Item { get; set; }

        /// <summary>The service names the user picked, in the order the picker returned them.</summary>
        List<String> m_Dependencies = new List<String>();

        /// <summary>The ids of the services this one waits for.</summary>
        List<Guid> m_ServiceDependencies = new List<Guid>();

        WindowsServiceLister m_ServiceLister;

        public FrmNewService()
        {
            InitializeComponent();
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            if (Item == null)
                Item = new ServiceItem();

            Item.Enabled = chkEnabled.Checked;
            Item.Description = txtDescription.Text;

            Item.Title = txtTitle.Text;
            Item.Path = txtPath.Text;
            Item.ForceRestart = chkAutoRestart.Checked;

            Item.Shutdown_Timeout = (int)nudShutdown_Timeout.Value;
            Item.Shutdown_ENTER_Send = chkShutdown_SendENTER.Checked;
            Item.Shutdown_ENTER_Timeout = (int)nudShutdown_ENTER_Timeout.Value;

            Item.LogConsoleOutput = chkLogs_ToMemory.Checked;
            Item.LogConsoleOutputToDisk = chkLogs_ToDisk.Checked;
            Item.ResetConsoleOutputDaily = chkLogs_ResetDaily.Checked;

            Item.WindowsDependencies = m_Dependencies;
            Item.Wait_On_Restart = chkDependencies_WaitOnRestart.Checked;

            var picked = checkedServiceIds();

            //A circle would leave the manager with a start order it cannot work out
            if (closesACircle(picked))
            {
                MessageBox.Show("The services would wait for each other. Take out the services that close the circle.", "Dependencies", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Item.Dependencies = picked;
            Item.DependencyRestartRule = selectedRule(cmbDependency_Restart, ServiceItem.eDependencyRestartRule.Ignore);
            Item.DependencyStartRule = selectedRule(cmbDependency_Start, ServiceItem.eDependencyStartRule.StartAnyway);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmNewService_Load(object sender, EventArgs e)
        {
            //The rule boxes and the list of the other services are filled either way. A service that is being
            //created has nothing to wait for yet, but it still has to show what can be picked.
            fillRuleBoxes();

            if (this.Item != null)
            {
                lblID.Text = this.Item.ID.ToString();
                chkEnabled.Checked = this.Item.Enabled;
                txtDescription.Text = this.Item.Description;
                txtTitle.Text = this.Item.Title;
                txtPath.Text = this.Item.Path;
                chkAutoRestart.Checked = this.Item.ForceRestart;


                nudShutdown_Timeout.Value = this.Item.Shutdown_Timeout;
                chkShutdown_SendENTER.Checked = this.Item.Shutdown_ENTER_Send;

                nudShutdown_ENTER_Timeout.Value = this.Item.Shutdown_ENTER_Timeout;


                chkLogs_ToMemory.Checked = this.Item.LogConsoleOutput;
                chkLogs_ToDisk.Checked = this.Item.LogConsoleOutputToDisk;
                chkLogs_ResetDaily.Checked = this.Item.ResetConsoleOutputDaily;

                m_Dependencies = this.Item.WindowsDependencies != null ? new List<String>(this.Item.WindowsDependencies) : new List<String>();
                chkDependencies_WaitOnRestart.Checked = this.Item.Wait_On_Restart;

                m_ServiceDependencies = this.Item.Dependencies != null ? new List<Guid>(this.Item.Dependencies) : new List<Guid>();

                selectRule(cmbDependency_Restart, this.Item.DependencyRestartRule);
                selectRule(cmbDependency_Start, this.Item.DependencyStartRule);

                fillDependencyList();
            }
            else
            {
                selectRule(cmbDependency_Restart, ServiceItem.eDependencyRestartRule.Ignore);
                selectRule(cmbDependency_Start, ServiceItem.eDependencyStartRule.StartAnyway);
            }

            fillServiceDependencies();
        }

        private void frmNewService_DragDrop(object sender, DragEventArgs e)
        {

            var formats = e.Data.GetFormats();

            if (formats.Contains("FileNameW"))
            {
                try
                {
                    var d = e.Data.GetData("FileNameW") as string[];
                    txtPath.Text = d[0];

                    FileInfo fi = new FileInfo(d[0]);
                    txtTitle.Text = Path.GetFileNameWithoutExtension(fi.Name);
                }
                catch
                {

                }
            }


        }


        private void frmNewService_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void txtPath_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ofdOpen.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofdOpen.FileName;

                txtTitle.Text = Path.GetFileNameWithoutExtension(ofdOpen.FileName);
            }
        }

        private void bnSelectPath_Click(object sender, EventArgs e)
        {
            if (ofdOpen.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofdOpen.FileName;

                txtTitle.Text = Path.GetFileNameWithoutExtension(ofdOpen.FileName);
            }
        }

        private void chkLogs_ToMemory_CheckedChanged(object sender, EventArgs e)
        {

            chkLogs_ToDisk.Enabled = chkLogs_ToMemory.Checked;
            chkLogs_ResetDaily.Enabled = chkLogs_ToMemory.Checked;

        }

        private void bnDependencies_Select_Click(object sender, EventArgs e)
        {
            using (var dialog = new FrmServicePicker(getServices(), m_Dependencies))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                m_Dependencies = dialog.SelectedNames;
                fillDependencyList();
            }
        }

        /// <summary>
        /// The services of the machine, read once and kept. The network stack is part of the list, waiting for
        /// a running network is exactly what these dependencies are used for.
        /// </summary>
        private List<WindowsServiceInfo> getServices()
        {
            if (m_ServiceLister == null)
            {
                m_ServiceLister = new WindowsServiceLister();
            }

            try
            {
                return m_ServiceLister.ListServices(true);
            }
            catch (Exception)
            {
                return new List<WindowsServiceInfo>();
            }
        }

        /// <summary>
        /// Shows the picked services with their title and their state. A service that is not installed any more
        /// stays in the list, that is something to see before saving.
        /// </summary>
        private void fillDependencyList()
        {
            var all = getServices();

            lsvDependencies.BeginUpdate();
            lsvDependencies.Items.Clear();

            foreach (String name in m_Dependencies)
            {
                var info = all.FirstOrDefault(m => String.Equals(m.ServiceName, name, StringComparison.OrdinalIgnoreCase));
                ListViewItem item = new ListViewItem(info != null ? info.DisplayName : name);

                item.SubItems.Add(name);
                item.SubItems.Add(info != null ? info.Status : "Not installed");

                lsvDependencies.Items.Add(item);
            }

            lsvDependencies.EndUpdate();
        }

        /// <summary>
        /// The other services of the settings file, the one being edited left out. A service that is being
        /// created is not in the file yet, so its id matches nothing and the list stays complete.
        /// </summary>
        private List<ServiceItem> otherServices()
        {
            var slist = ServiceConfig.load();

            var own = Item != null ? Item.ID : Guid.Empty;

            return slist.ServiceList
                .Where(a => a != null && a.ID != own)
                .OrderBy(a => a.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Lists the other services with a check mark on the ones this one waits for. A service that was
        /// deleted from the settings file leaves no row behind, its id is dropped on the next save.
        /// </summary>
        private void fillServiceDependencies()
        {
            lsvServiceDependencies.BeginUpdate();
            lsvServiceDependencies.Items.Clear();

            foreach (var service in otherServices())
            {
                ListViewItem row = new ListViewItem(service.Title);

                row.SubItems.Add(service.Path);
                row.Tag = service.ID;
                row.Checked = m_ServiceDependencies.Contains(service.ID);

                lsvServiceDependencies.Items.Add(row);
            }

            lsvServiceDependencies.EndUpdate();
        }

        /// <summary>The ids of the checked rows, in the order the list shows them.</summary>
        private List<Guid> checkedServiceIds()
        {
            var ids = new List<Guid>();

            foreach (ListViewItem row in lsvServiceDependencies.CheckedItems)
            {
                var id = row.Tag as Guid?;

                if (id == null || id == Guid.Empty)
                {
                    continue;
                }

                if (Item != null && id.Value == Item.ID)
                {
                    continue;
                }

                if (!ids.Contains(id.Value))
                {
                    ids.Add(id.Value);
                }
            }

            return ids;
        }

        /// <summary>
        /// True when the picked services would make this service wait for itself, directly or through the
        /// services in between. The service is put into the list of the settings file the way it would be
        /// saved, so a circle that is only closed by this dialog is caught here as well.
        /// </summary>
        private bool closesACircle(List<Guid> picked)
        {
            if (Item == null || picked.Count == 0)
            {
                return false;
            }

            var test = new ServiceItem();
            test.ID = Item.ID;
            test.Title = Item.Title;
            test.Dependencies = picked;

            var graph = new ServiceDependencyGraph(otherServices().Concat(new List<ServiceItem> { test }));

            return graph.ReachesItself(test.ID);
        }

        /// <summary>Fills the two rule boxes with the rules a dependent service can follow.</summary>
        private void fillRuleBoxes()
        {
            cmbDependency_Restart.Items.Clear();

            foreach (ServiceItem.eDependencyRestartRule rule in Enum.GetValues(typeof(ServiceItem.eDependencyRestartRule)))
            {
                cmbDependency_Restart.Items.Add(new RuleItem(restartRuleText(rule), rule));
            }

            cmbDependency_Start.Items.Clear();

            foreach (ServiceItem.eDependencyStartRule rule in Enum.GetValues(typeof(ServiceItem.eDependencyStartRule)))
            {
                cmbDependency_Start.Items.Add(new RuleItem(startRuleText(rule), rule));
            }
        }

        private static String restartRuleText(ServiceItem.eDependencyRestartRule rule)
        {
            switch (rule)
            {
                case ServiceItem.eDependencyRestartRule.Restart:
                    return "Restart this service";
                case ServiceItem.eDependencyRestartRule.Stop:
                    return "Stop this service";
                default:
                    return "Leave this service alone";
            }
        }

        private static String startRuleText(ServiceItem.eDependencyStartRule rule)
        {
            switch (rule)
            {
                case ServiceItem.eDependencyStartRule.DoNotStart:
                    return "Do not start this service";
                default:
                    return "Start anyway";
            }
        }

        /// <summary>Selects the entry of the box that carries the given rule.</summary>
        private static void selectRule(ComboBox box, Object rule)
        {
            foreach (RuleItem item in box.Items)
            {
                if (Equals(item.Value, rule))
                {
                    box.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>The rule picked in one of the boxes, the given default when nothing was picked.</summary>
        private static T selectedRule<T>(ComboBox box, T fallback)
        {
            var item = box.SelectedItem as RuleItem;

            if (item == null || !(item.Value is T))
            {
                return fallback;
            }

            return (T)item.Value;
        }

        /// <summary>Entry of one of the rule boxes.</summary>
        private sealed class RuleItem
        {
            public RuleItem(String text, Object value)
            {
                Text = text;
                Value = value;
            }

            public String Text { get; }

            public Object Value { get; }

            public override String ToString()
            {
                return Text;
            }
        }
    }
}
