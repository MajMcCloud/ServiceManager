using ServiceManager.Base.Data;
using ServiceManager.Base.Windows;
using ServiceManager.UI.Windows;
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

        /// <summary>Reads the accounts of this machine for the account picker.</summary>
        IUserAccountReader m_AccountReader = new LocalUserAccountReader();

        /// <summary>The accounts of this machine, read once the first picker is opened.</summary>
        List<UserAccountInfo> m_RunAsAccounts;

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
            Item.DependencyStopRule = selectedRule(cmbDependency_Stop, ServiceItem.eDependencyStopRule.StayRunning);

            ServiceItem.eRunAs mode = selectedRule(cmbRunAs, ServiceItem.eRunAs.Inherit);
            String accountName = txtRunAs_User.Text.Trim();

            if (mode == ServiceItem.eRunAs.User && accountName.Length == 0)
            {
                MessageBox.Show("The service needs a user name to run under another account.", "Run as", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Item.RunAs = mode;

            //A built-in account is named by the system, the boxes of the dialog stay empty for it
            Item.RunAsUser = mode == ServiceItem.eRunAs.User ? accountName : String.Empty;
            Item.RunAsDomain = mode == ServiceItem.eRunAs.User ? txtRunAs_Domain.Text.Trim() : String.Empty;

            //The stored password is never shown in the dialog, so it stays unless a new one was typed
            if (txtRunAs_Password.Text.Length > 0)
            {
                Item.RunAsPassword = SecretStorage.Protect(txtRunAs_Password.Text);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmNewService_Load(object sender, EventArgs e)
        {
            //The rule boxes and the list of the other services are filled either way. A service that is being
            //created has nothing to wait for yet, but it still has to show what can be picked.
            fillRuleBoxes();
            fillRunAsBoxes();

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
                selectRule(cmbDependency_Stop, this.Item.DependencyStopRule);

                selectRule(cmbRunAs, this.Item.RunAs);
                txtRunAs_User.Text = this.Item.RunAsUser;
                txtRunAs_Domain.Text = this.Item.RunAsDomain;

                fillDependencyList();
            }
            else
            {
                selectRule(cmbDependency_Restart, ServiceItem.eDependencyRestartRule.Ignore);
                selectRule(cmbDependency_Start, ServiceItem.eDependencyStartRule.StartAnyway);
                selectRule(cmbDependency_Stop, ServiceItem.eDependencyStopRule.StayRunning);

                selectRule(cmbRunAs, ServiceItem.eRunAs.Inherit);
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

        private void cmbRunAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyRunAsFields();
        }

        private void bnRunAs_Select_Click(object sender, EventArgs e)
        {
            if (m_RunAsAccounts == null)
            {
                try
                {
                    m_RunAsAccounts = m_AccountReader.ListAccounts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The accounts of this machine cannot be read: " + ex.Message, "Run as", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            using (var dialog = new FrmUserAccountPicker(m_RunAsAccounts, txtRunAs_User.Text))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                txtRunAs_User.Text = dialog.Selected.Name;
                txtRunAs_Domain.Text = dialog.Selected.Domain;

                //The password is what the user has to add after picking an account
                txtRunAs_Password.Focus();
            }
        }

        /// <summary>Fills the account box with the ways a process can be started.</summary>
        private void fillRunAsBoxes()
        {
            cmbRunAs.Items.Clear();

            foreach (ServiceItem.eRunAs mode in Enum.GetValues(typeof(ServiceItem.eRunAs)))
            {
                cmbRunAs.Items.Add(new RuleItem(runAsText(mode), mode));
            }

            applyRunAsFields();
        }

        private static String runAsText(ServiceItem.eRunAs mode)
        {
            switch (mode)
            {
                case ServiceItem.eRunAs.User:
                    return "Another account";
                case ServiceItem.eRunAs.LocalSystem:
                    return "LocalSystem";
                case ServiceItem.eRunAs.LocalService:
                    return "LocalService";
                case ServiceItem.eRunAs.NetworkService:
                    return "NetworkService";
                default:
                    return "Account of the ServiceManager";
            }
        }

        /// <summary>The account fields only make sense once an account was picked.</summary>
        private void applyRunAsFields()
        {
            ServiceItem.eRunAs mode = selectedRule(cmbRunAs, ServiceItem.eRunAs.Inherit);

            bool user = mode == ServiceItem.eRunAs.User;
            bool builtIn = RunAsAccount.IsBuiltIn(mode);

            txtRunAs_User.Enabled = user;
            txtRunAs_Domain.Enabled = user;
            txtRunAs_Password.Enabled = user;
            bnRunAs_Select.Enabled = user;

            //A built-in account has no password to type, and its logon is only granted to an installed service
            lblRunAs_BuiltIn.Visible = builtIn;
            lblRunAs_BuiltIn.Text = builtInText(mode);
            lblRunAs_BuiltIn.ForeColor = builtIn && !WindowsHostAccount.IsLocalSystem() ? Color.Firebrick : SystemColors.ControlText;
        }

        /// <summary>What the dialog says about a built-in account, the reason out loud when this instance cannot run it.</summary>
        private static String builtInText(ServiceItem.eRunAs mode)
        {
            if (WindowsHostAccount.IsLocalSystem())
            {
                return runAsText(mode) + " is an account Windows builds in. It has no password and no profile of its own, the process is started through its token.";
            }

            return runAsText(mode) + " is an account Windows builds in. It has no password, so it only works while ServiceManager runs as a Windows service under LocalSystem.";
        }

        /// <summary>Fills the rule boxes with the rules a dependent service can follow.</summary>
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

            cmbDependency_Stop.Items.Clear();

            foreach (ServiceItem.eDependencyStopRule rule in Enum.GetValues(typeof(ServiceItem.eDependencyStopRule)))
            {
                cmbDependency_Stop.Items.Add(new RuleItem(stopRuleText(rule), rule));
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

        private static String stopRuleText(ServiceItem.eDependencyStopRule rule)
        {
            switch (rule)
            {
                case ServiceItem.eDependencyStopRule.Stop:
                    return "Stop this service";
                default:
                    return "Leave this service running";
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
