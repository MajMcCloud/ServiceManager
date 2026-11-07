using ServiceManager.Base.data;
using ServiceManager.Base.data.settings;
using ServiceManager.Extensions;
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
using static ServiceManager.Base.data.settings.Runtime;

namespace ServiceManager.UI
{
    public partial class frmManage : Form
    {
        public String ConfigFile { get; set; }

        /// <summary>The providers of the settings file, edited before they are saved.</summary>
        private List<NotificationTarget> m_Targets = new List<NotificationTarget>();

        /// <summary>True while the list is filled, so the selection handler does not write back.</summary>
        private bool m_LoadingTargets;

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

            //Runtime Settings

            switch (slist.RuntimeSettings.Mode)
            {
                case eMode.NetPipes:

                    rrbMode_NetPipes.Checked = true;

                    break;
                case eMode.Tcp:

                    rrbMode_TCP.Checked = true;

                    break;
            }


            nud_TCPPort.Value = slist.RuntimeSettings.TCPPort;

            nudStartup_Delay.Value = slist.RuntimeSettings.Startup_Delay;


            //Notification Settings
            txtNotifications_InstanceName.Text = slist.Notifications.InstanceName;

            var extensions = Extensions.Extensions.GetNotificationExtensions();

            foreach (var extension in extensions)
            {

                cmbNotification_Extensions.Items.Add(new ExtensionItem(extension.Name, extension.Id));

            }

            if (cmbNotification_Extensions.Items.Count > 0)
                cmbNotification_Extensions.SelectedIndex = 0;

            m_Targets = slist.Notifications.Targets ?? new List<NotificationTarget>();

            reloadTargets();


        }


        /// <summary>Eintrag der aktiven Provider.</summary>
        private sealed class TargetItem
        {
            public TargetItem(NotificationTarget target, string text)
            {
                Target = target;
                Text = text;
            }

            public NotificationTarget Target { get; }

            public string Text { get; }

            public override string ToString()
            {
                return Text;
            }
        }

        /// <summary>
        /// Refills the provider list. A provider whose assembly disappeared is still listed, otherwise
        /// saving would silently drop it and the user could no longer remove it.
        /// </summary>
        private void reloadTargets(int? select = null)
        {
            var extensions = Extensions.Extensions.GetNotificationExtensions();

            m_LoadingTargets = true;

            lstNotifications_Providers.BeginUpdate();
            lstNotifications_Providers.Items.Clear();

            foreach (var target in m_Targets)
            {
                var extension = extensions.FirstOrDefault(a => a.Id == target.PluginId);

                var text = extension != null ? extension.Name : "unknown provider: " + target.PluginId;

                lstNotifications_Providers.Items.Add(new TargetItem(target, text));
            }

            lstNotifications_Providers.EndUpdate();

            var index = select ?? lstNotifications_Providers.SelectedIndex;

            if (index < 0 || index >= lstNotifications_Providers.Items.Count)
                index = lstNotifications_Providers.Items.Count > 0 ? 0 : -1;

            lstNotifications_Providers.SelectedIndex = index;

            m_LoadingTargets = false;

            applySelectedTarget();
        }

        private NotificationTarget selectedTarget()
        {
            var item = lstNotifications_Providers.SelectedItem as TargetItem;

            return item != null ? item.Target : null;
        }

        /// <summary>Moves the checkbox state into the provider that is currently selected.</summary>
        private void commitCheckboxes()
        {
            var target = selectedTarget();

            if (target == null)
                return;

            target.OnStart = chkNotifications_OnStart.Checked;
            target.OnStop = chkNotifications_OnStop.Checked;
            target.OnRestart = chkNotifications_OnRestart.Checked;
            target.OnConsoleOutput = chkNotifications_OnConsoleOutput.Checked;
            target.OnConsoleError = chkNotifications_OnConsoleError.Checked;
            target.OnServerStart = chkNotifications_OnServerStart.Checked;
            target.OnServerShutdown = chkNotifications_OnServerShutdown.Checked;
        }

        /// <summary>Shows the events the selected provider asked for.</summary>
        private void applySelectedTarget()
        {
            var target = selectedTarget();

            bnNotifications_Remove.Enabled = target != null;
            bnNotifications_Configure.Enabled = target != null;

            if (target == null)
                return;

            chkNotifications_OnStart.Checked = target.OnStart;
            chkNotifications_OnStop.Checked = target.OnStop;
            chkNotifications_OnRestart.Checked = target.OnRestart;
            chkNotifications_OnConsoleOutput.Checked = target.OnConsoleOutput;
            chkNotifications_OnConsoleError.Checked = target.OnConsoleError;
            chkNotifications_OnServerStart.Checked = target.OnServerStart;
            chkNotifications_OnServerShutdown.Checked = target.OnServerShutdown;
        }

        private void lstNotifications_Providers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_LoadingTargets)
                return;

            applySelectedTarget();
        }

        private void bnNotifications_Add_Click(object sender, EventArgs e)
        {
            var selected = cmbNotification_Extensions.SelectedItem as ExtensionItem;

            if (selected == null || selected.Value == null)
                return;

            var id = (Guid)selected.Value;

            if (m_Targets.Any(a => a.PluginId == id))
            {
                MessageBox.Show("This provider is already active, select it in the list below.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            commitCheckboxes();

            m_Targets.Add(new NotificationTarget() { PluginId = id });

            reloadTargets(m_Targets.Count - 1);
        }

        private void bnNotifications_Remove_Click(object sender, EventArgs e)
        {
            var target = selectedTarget();

            if (target == null)
                return;

            m_Targets.Remove(target);

            reloadTargets(0);
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

        private void rrbMode_TCP_CheckedChanged(object sender, EventArgs e)
        {
            var on = rrbMode_TCP.Checked;

            lblPort.Enabled = on;
            nud_TCPPort.Enabled = on;

        }

        private void bnSettings_Save_Click(object sender, EventArgs e)
        {
            var slist = ServiceConfig.load();

            if (rrbMode_TCP.Checked)
            {
                slist.RuntimeSettings.Mode = eMode.Tcp;

                slist.RuntimeSettings.TCPPort = (int)nud_TCPPort.Value;
            }

            if (rrbMode_NetPipes.Checked)
            {
                slist.RuntimeSettings.Mode = eMode.NetPipes;

            }

            slist.RuntimeSettings.Startup_Delay = (int)nudStartup_Delay.Value;

            slist.save();

            this.Close();
        }

        private void rbnNotifications_Save_Click(object sender, EventArgs e)
        {
            var slist = ServiceConfig.load();

            slist.Notifications.InstanceName = txtNotifications_InstanceName.Text;

            commitCheckboxes();

            slist.Notifications.Targets = m_Targets;

            // An older file keeps its single provider in PluginId. Migrate() moved it into Targets on
            // load, so what is written here never carries both.
            slist.Notifications.PluginId = null;

            slist.save();

            this.Close();
        }

        private void bnNotifications_Configure_Click(object sender, EventArgs e)
        {
            Guid pluginId;

            var target = selectedTarget();

            if (target != null)
            {
                pluginId = target.PluginId;
            }
            else
            {
                var selected = cmbNotification_Extensions.SelectedItem as ExtensionItem;
                if (selected == null || selected.Value == null)
                {
                    return;
                }

                pluginId = (Guid)selected.Value;
            }

            var extensions = Extensions.Extensions.GetNotificationExtensions();

            var plugin = extensions.FirstOrDefault(a => a.Id == pluginId);
            if (plugin == null)
            {
                MessageBox.Show("The settings name a provider that is not installed in the extensions folder.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            plugin.Notification.Configure();

            // The dialog writes the settings file, but the cached instance keeps its settings in memory.
            Extensions.Extensions.Reload();

            reloadTargets();
        }

        /// <summary>Eintrag der Extensions-Auswahl.</summary>
        private sealed class ExtensionItem
        {
            public ExtensionItem(string text, object value)
            {
                Text = text;
                Value = value;
            }

            public string Text { get; }

            public object Value { get; }

            public override string ToString() => Text;
        }
    }
}
