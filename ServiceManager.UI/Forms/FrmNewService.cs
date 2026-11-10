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

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmNewService_Load(object sender, EventArgs e)
        {
            if (this.Item == null)
                return;

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

            fillDependencyList();
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
    }
}
