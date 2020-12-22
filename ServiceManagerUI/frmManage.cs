using ServiceManager.Base.data;
using ServiceManager.UI.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using static ServiceManager.Base.data.settings.Runtime;

namespace ServiceManager.UI
{
    public partial class frmManage : RadForm
    {
        public String ConfigFile { get; set; }

        public frmManage()
        {
            InitializeComponent();
        }

        private void frmManage_Load(object sender, EventArgs e)
        {
            loadList();
        }

        private void loadList()
        {
            var slist = ServiceConfig.load();

            lsvServices.BeginUpdate();
            lsvServices.Items.Clear();

            var running = ServiceManager.Base.ServiceManager.IsServiceRunning;

            
            foreach (var i in slist.ServiceList.OrderBy(a => a.Title))
            {

                var itm = new ListViewItem(i.ID.ToString());
                itm.SubItems.Add(i.Enabled ? "True" : "False");
                itm.SubItems.Add(i.Description);
                itm.SubItems.Add(i.Title);
                itm.SubItems.Add(i.Path);
                //itm.SubItems.Add((i.Analytics?.IsRunning ?? false).ToString());
                itm.Tag = i.ID;
                lsvServices.Items.Add(itm);
            }

            lsvServices.EndUpdate();

            switch (slist.RuntimeSettings.Mode)
            {
                case eMode.NetPipes:

                    rrbMode_NetPipes.CheckState = CheckState.Checked;

                    break;
                case eMode.Tcp:

                    rrbMode_TCP.CheckState = CheckState.Checked;

                    break;
            }


            nud_TCPPort.Value = slist.RuntimeSettings.TCPPort;

        }

        private void lsvServices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var item = lsvServices.HitTest(e.Location);
            if (item == null)
            {

                return;
            }

            cmsMenu.Show(lsvServices.PointToScreen(e.Location));
        }

        private void bnAddServer_Click(object sender, EventArgs e)
        {
            frmNewService frm = new frmNewService();

            if (frm.ShowDialog() == DialogResult.OK)
            {
                var slist = ServiceConfig.load();

                do
                {
                    frm.Item.ID = Guid.NewGuid();
                } while (slist.ServiceList.Count(a => a.ID == frm.Item.ID) > 0);

                slist.ServiceList.Add(frm.Item);

                slist.save();
            }

            loadList();
        }

        private void lsvServices_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = lsvServices.HitTest(e.Location);
            if (item == null)
            {
                return;
            }

            var slist = ServiceConfig.load();
            var service = slist.ServiceList.FirstOrDefault(a => a.ID == (Guid)item.Item.Tag);
            if (service == null)
            {
                return;
            }

            frmNewService frm = new frmNewService();
            frm.Item = service;

            if (frm.ShowDialog() == DialogResult.OK)
            {
                slist.ServiceList.RemoveAll(a => a.ID == service.ID);

                slist.ServiceList.Add(frm.Item);

                slist.save();
            }

            loadList();
        }

        private void tsmiMenu_Delete_Click(object sender, EventArgs e)
        {
            if (lsvServices.SelectedItems.Count == 0)
                return;

            if (MessageBox.Show("Are you sure to remove these " + lsvServices.SelectedItems.Count + " services?", "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var slist = ServiceConfig.load();

            foreach (var si in lsvServices.SelectedItems)
            {
                var id = (si as ListViewItem)?.Tag as Guid?;
                if (id == null)
                    continue;

                slist.ServiceList.RemoveAll(a => a.ID == id.Value);
            }

            slist.save();

            loadList();
        }

        private void rrbMode_TCP_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            var on = args.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On;

            lblPort.Enabled = on;
            nud_TCPPort.Enabled = on;

        }

        private void rrbMode_NetPipes_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {



        }

        private void bnSettings_Save_Click(object sender, EventArgs e)
        {
            var slist = ServiceConfig.load();

            if(rrbMode_TCP.CheckState == CheckState.Checked)
            {
                slist.RuntimeSettings.Mode = eMode.Tcp;

                slist.RuntimeSettings.TCPPort = (int)nud_TCPPort.Value;
            }

            if(rrbMode_NetPipes.CheckState == CheckState.Checked)
            {
                slist.RuntimeSettings.Mode = eMode.NetPipes;

            }

            slist.save();

            this.Close();
        }
    }
}
