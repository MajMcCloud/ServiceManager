using ServiceManager.Base.data;
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
using Telerik.WinControls.UI;

namespace ServiceManagerUI.forms
{
    public partial class frmNewService : RadForm
    {

        public ServiceItem Item { get; set; }

        public frmNewService()
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
    }
}
