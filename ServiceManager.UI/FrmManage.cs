using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;
using ServiceManager.Base.Progress;
using ServiceManager.Base.Windows;
using ServiceManager.Extensions;
using ServiceManager.UI.Forms;
using ServiceManager.UI.Installer;
using ServiceManager.UI.Notifications;
using ServiceManager.UI.Progress;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ServiceManager.Base.Data.Settings.Runtime;

namespace ServiceManager.UI
{
    public partial class FrmManage : Form
    {
        public String ConfigFile { get; set; }

        /// <summary>The tabs of this window, in the order they sit on the control.</summary>
        public enum eTab
        {
            Services = 0,
            Settings = 1,
            Notifications = 2,
            WindowsService = 3,
            Dependencies = 4
        }

        /// <summary>The tab the window opens on. Null leaves the first one selected.</summary>
        public eTab? OpenTab { get; set; }

        /// <summary>The providers of the settings file, edited before they are saved.</summary>
        private List<NotificationTarget> m_Targets = new List<NotificationTarget>();

        /// <summary>True while the list is filled, so the selection handler does not write back.</summary>
        private bool m_LoadingTargets;

        /// <summary>The provider the checkboxes currently show, the one <see cref="commitCheckboxes"/> writes to.</summary>
        private NotificationTarget m_DisplayedTarget;

        /// <summary>Image of a provider whose settings are still incomplete.</summary>
        private const String WarningImage = "warning";

        private List<String> m_CustomDependencies = new List<String>();
        private bool m_LoadingWindowsSettings;

        /// <summary>Reads the services of the machine once per dialog, the list and the picker share it.</summary>
        private WindowsServiceLister m_ServiceLister;

        /// <summary>Reads the rights of this process, asked once per dialog.</summary>
        private readonly IProcessElevation m_Elevation = new WindowsElevation();

        public FrmManage()
        {
            InitializeComponent();

            var providerImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };

            // The icon of the shell stays readable under any theme, and it needs no file of its own.
            providerImages.Images.Add(WarningImage, SystemIcons.Warning.ToBitmap());

            lsvNotifications_Providers.SmallImageList = providerImages;
        }

        private void frmManage_Load(object sender, EventArgs e)
        {
            if (OpenTab.HasValue)
            {
                tabMain.SelectedIndex = (int)OpenTab.Value;
            }

            loadList();

            showWindowsServiceTabShield();

            showDependencyTree();
        }

        /// <summary>
        /// The services are edited on the first tab, so the tree is built again whenever its own tab is opened
        /// rather than once for the whole dialog.
        /// </summary>
        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab != tabDependencies)
            {
                return;
            }

            showDependencyTree();
        }

        /// <summary>
        /// The tree shows what the start order is built from, so it is handed the same graph the manager works
        /// with. Nothing here decides anything about the dependencies, the tree only draws what it is given.
        /// </summary>
        private void showDependencyTree()
        {
            var slist = ServiceConfig.load();
            var windows = slist.WindowsSettings;

            var tree = new DependencyTree(new ServiceDependencyGraph(slist.ServiceList), Program.ServiceName,
                windows == null ? null : windows.GetDependencies());

            trvDependencies.BeginUpdate();
            trvDependencies.Nodes.Clear();

            foreach (var node in tree.Build())
            {
                trvDependencies.Nodes.Add(toTreeNode(node));
            }

            trvDependencies.ExpandAll();
            trvDependencies.EndUpdate();
        }

        /// <summary>
        /// A disabled service and a service that sits on a circle are marked instead of left out, otherwise the
        /// tree would hide the two things a person has to see.
        /// </summary>
        private TreeNode toTreeNode(DependencyTreeNode node)
        {
            var item = new TreeNode(node.Title);

            //The node is the key of the double click, it names the service the settings belong to.
            item.Tag = node;

            // A Windows service is always a leaf, the settings file holds no dependencies below it.
            if (node.Kind == DependencyTreeNode.eNodeKind.WindowsService)
            {
                return item;
            }

            if (!node.Enabled)
            {
                item.Text += " (disabled)";
                item.ForeColor = Color.Gray;
            }

            if (node.IsRepeat)
            {
                item.Text += " (see above)";
                item.ForeColor = Color.Gray;
            }
            else if (node.InCycle)
            {
                item.Text += " (circle)";
                item.ForeColor = Color.Firebrick;
            }

            foreach (var child in node.Children)
            {
                item.Nodes.Add(toTreeNode(child));
            }

            return item;
        }

        /// <summary>
        /// A service node opens the settings of that service, the ServiceManager node opens the tab holding its
        /// own settings. A Windows service has no settings here, and the repeat that closes a circle leads to a
        /// service that is already shown above.
        /// </summary>
        private void trvDependencies_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //The tree turns the branch of a node over on a double click. The settings are what the click is meant
            //for, so the branch is turned back and stays as the user left it.
            if (e.Node != null)
            {
                e.Node.Toggle();
            }

            var node = (e.Node as TreeNode)?.Tag as DependencyTreeNode;
            if (node == null || node.IsRepeat)
            {
                return;
            }

            if (node.Kind == DependencyTreeNode.eNodeKind.Host)
            {
                tabMain.SelectedTab = tabWindowsService;
                return;
            }

            if (node.Kind != DependencyTreeNode.eNodeKind.Service)
            {
                return;
            }

            editService(node.ID);

            showDependencyTree();
        }

        /// <summary>
        /// The settings of the Windows service are written to the service control manager, which asks for
        /// elevated rights. The shield sits on the tab so that the hint is seen before the save button of
        /// that tab is looked for.
        /// </summary>
        private void showWindowsServiceTabShield()
        {
            if (!ElevationRules.NeedsShield(ElevationRules.eOperation.WindowsServiceSettings, m_Elevation.IsElevated()))
            {
                return;
            }

            Image shield = ShieldIcon.Get();

            if (shield == null)
            {
                return;
            }

            ImageList icons = new ImageList();
            icons.ImageSize = new Size(shield.Width, shield.Height);
            icons.Images.Add(shield);

            tabMain.ImageList = icons;
            tabWindowsService.ImageIndex = 0;
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

            nudDependency_Wait_Timeout.Value = slist.RuntimeSettings.Dependency_Wait_Timeout;

            chkRequireAdministrator.Checked = slist.RuntimeSettings.RequireAdministrator;


            //Windows service settings

            loadWindowsService(slist);


            //Notification Settings
            txtNotifications_InstanceName.Text = slist.Notifications.InstanceName;

            var extensions = Extensions.Extensions.GetNotificationExtensions();

            // The list of providers grows with every extension dropped into the folder, so the choice is
            // offered in the order the names read rather than in the order the files are found.
            foreach (var extension in extensions.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
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
            public TargetItem(NotificationTarget target, Extension extension, string text)
            {
                Target = target;
                Extension = extension;
                Text = text;
            }

            public NotificationTarget Target { get; }

            /// <summary>Der geladene Provider, null wenn seine Assembly verschwunden ist.</summary>
            public Extension Extension { get; }

            public string Text { get; }

            public override string ToString()
            {
                return Text;
            }
        }

        /// <summary>
        /// Refills the provider list in alphabetical order. A provider whose assembly disappeared is still
        /// listed, otherwise saving would silently drop it and the user could no longer remove it.
        /// </summary>
        private void reloadTargets(NotificationTarget select = null)
        {
            var extensions = Extensions.Extensions.GetNotificationExtensions();

            var entries = m_Targets.Select(target =>
            {
                var extension = extensions.FirstOrDefault(a => a.Id == target.PluginId);

                var text = extension != null ? extension.Name : "unknown provider: " + target.PluginId;

                return new TargetItem(target, extension, text);
            })
            .OrderBy(a => a.Text, StringComparer.OrdinalIgnoreCase)
            .ToList();

            // The order is not the one of the settings file, so the entry to keep is the provider itself.
            var wanted = select ?? selectedTarget();

            m_LoadingTargets = true;

            lsvNotifications_Providers.BeginUpdate();
            lsvNotifications_Providers.Items.Clear();

            foreach (var entry in entries)
            {
                ListViewItem row = new ListViewItem(entry.Text);

                row.Tag = entry;

                // The icon points at the entry before anyone selects it and reads the sentence beside it.
                if (ProviderWarning.Missing(entry.Extension).Count > 0)
                    row.ImageKey = WarningImage;

                lsvNotifications_Providers.Items.Add(row);
            }

            lsvNotifications_Providers.EndUpdate();

            var index = entries.FindIndex(a => ReferenceEquals(a.Target, wanted));

            if (index < 0)
                index = entries.Count > 0 ? 0 : -1;

            if (index >= 0)
                lsvNotifications_Providers.SelectedIndices.Add(index);

            m_LoadingTargets = false;

            applySelectedTarget();
        }

        private TargetItem selectedItem()
        {
            if (lsvNotifications_Providers.SelectedItems.Count == 0)
                return null;

            return lsvNotifications_Providers.SelectedItems[0].Tag as TargetItem;
        }

        private NotificationTarget selectedTarget()
        {
            var item = selectedItem();

            return item != null ? item.Target : null;
        }

        /// <summary>Moves the checkbox state into the provider the checkboxes were loaded for.</summary>
        private void commitCheckboxes()
        {
            var target = m_DisplayedTarget;

            if (target == null)
                return;

            target.OnStart = chkNotifications_OnStart.Checked;
            target.OnStop = chkNotifications_OnStop.Checked;
            target.OnRestart = chkNotifications_OnRestart.Checked;
            target.OnConsoleOutput = chkNotifications_OnConsoleOutput.Checked;
            target.OnConsoleError = chkNotifications_OnConsoleError.Checked;
            target.OnServerStart = chkNotifications_OnServerStart.Checked;
            target.OnServerShutdown = chkNotifications_OnServerShutdown.Checked;
            target.OnDependencyWait = chkNotifications_OnDependencyWait.Checked;
            target.OnServiceCrashed = chkNotifications_OnServiceCrashed.Checked;
        }

        /// <summary>Shows the events the selected provider asked for.</summary>
        private void applySelectedTarget()
        {
            var entry = selectedItem();

            var target = entry != null ? entry.Target : null;

            m_DisplayedTarget = target;

            bnNotifications_Remove.Enabled = target != null;
            bnNotifications_Configure.Enabled = target != null;

            lblNotifications_Warning.Text = entry == null ? "" : ProviderWarning.Text(entry.Text, ProviderWarning.Missing(entry.Extension));

            if (target == null)
                return;

            chkNotifications_OnStart.Checked = target.OnStart;
            chkNotifications_OnStop.Checked = target.OnStop;
            chkNotifications_OnRestart.Checked = target.OnRestart;
            chkNotifications_OnConsoleOutput.Checked = target.OnConsoleOutput;
            chkNotifications_OnConsoleError.Checked = target.OnConsoleError;
            chkNotifications_OnServerStart.Checked = target.OnServerStart;
            chkNotifications_OnServerShutdown.Checked = target.OnServerShutdown;
            chkNotifications_OnDependencyWait.Checked = target.OnDependencyWait;
            chkNotifications_OnServiceCrashed.Checked = target.OnServiceCrashed;
        }

        private void lsvNotifications_Providers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_LoadingTargets)
                return;

            // The selection is already the new one, so the ticks still in the boxes belong to the previous entry.
            commitCheckboxes();

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

            var added = new NotificationTarget() { PluginId = id };

            m_Targets.Add(added);

            reloadTargets(added);
        }

        private void bnNotifications_Remove_Click(object sender, EventArgs e)
        {
            var target = selectedTarget();

            if (target == null)
                return;

            m_Targets.Remove(target);

            reloadTargets();
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
            FrmNewService frm = new FrmNewService();

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

            editService((Guid)item.Item.Tag);
        }

        /// <summary>Opens the settings of one service and writes the file back once the dialog was accepted.</summary>
        private void editService(Guid id)
        {
            var slist = ServiceConfig.load();
            var service = slist.ServiceList.FirstOrDefault(a => a.ID == id);
            if (service == null)
            {
                return;
            }

            FrmNewService frm = new FrmNewService();
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

            var slist = ServiceConfig.load();

            var ids = new List<Guid>();

            foreach (var si in lsvServices.SelectedItems)
            {
                var id = (si as ListViewItem)?.Tag as Guid?;
                if (id == null)
                    continue;

                if (!ids.Contains(id.Value))
                    ids.Add(id.Value);
            }

            //A service that others wait for is named together with everything that waits for it, so the
            //person deleting can see what the list entry takes with it.
            var graph = new ServiceDependencyGraph(slist.ServiceList);
            var dependents = new List<ServiceItem>();

            foreach (Guid id in ids)
            {
                foreach (var dependent in graph.DependentsRecursively(id))
                {
                    if (!dependents.Contains(dependent))
                        dependents.Add(dependent);
                }
            }

            var question = "Are you sure to remove these " + ids.Count + " services?";

            if (dependents.Count > 0)
            {
                question += "\r\n\r\nThese services depend on them and will no longer wait for them:\r\n" + String.Join("\r\n", dependents.Select(a => a.Title).OrderBy(a => a, StringComparer.OrdinalIgnoreCase));
            }

            if (MessageBox.Show(question, "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            foreach (Guid id in ids)
            {
                slist.ServiceList.RemoveAll(a => a.ID == id);

                foreach (var service in slist.ServiceList)
                {
                    if (service.Dependencies != null)
                        service.Dependencies.Remove(id);
                }
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

        /// <summary>
        /// Fills the service control manager section. The values come from the settings file and not from
        /// the installation, so opening and closing the dialog changes nothing by itself.
        /// </summary>
        private void loadWindowsService(ServiceConfig slist)
        {
            var windows = slist.WindowsSettings;

            m_LoadingWindowsSettings = true;
            cmbService_StartMode.Items.Clear();
            cmbService_Account.Items.Clear();
            cmbService_Dependencies.Items.Clear();
            m_CustomDependencies = (windows.CustomDependencies ?? new List<String>()).ToList();

            foreach (WindowsService.eStartMode mode in Enum.GetValues(typeof(WindowsService.eStartMode)))
            {
                cmbService_StartMode.Items.Add(new ChoiceItem(startModeText(mode), mode));
            }

            foreach (WindowsService.eAccount account in Enum.GetValues(typeof(WindowsService.eAccount)))
            {
                cmbService_Account.Items.Add(new ChoiceItem(accountText(account), account));
            }

            foreach (WindowsService.eDependencyMode mode in Enum.GetValues(typeof(WindowsService.eDependencyMode)))
            {
                cmbService_Dependencies.Items.Add(new ChoiceItem(dependencyText(mode), mode));
            }

            selectChoice(cmbService_StartMode, windows.StartMode);
            selectChoice(cmbService_Account, windows.Account);
            selectChoice(cmbService_Dependencies, windows.DependenciesMode);

            txtService_User.Text = windows.UserName ?? String.Empty;
            txtService_Password.Clear();
            m_LoadingWindowsSettings = false;

            applyAccountFields();
            applyDependencyFields();
        }

        /// <summary>
        /// Reads the section back into the settings. True when an edit came in, which is what decides
        /// whether the installation has to be touched.
        /// </summary>
        private bool applyWindowsSettings(ServiceConfig slist)
        {
            var windows = slist.WindowsSettings;

            var startMode = selectedChoice(cmbService_StartMode, windows.StartMode);
            var account = selectedChoice(cmbService_Account, windows.Account);
            var dependencies = selectedChoice(cmbService_Dependencies, windows.DependenciesMode);

            String userName = txtService_User.Text.Trim();
            bool passwordEntered = account == WindowsService.eAccount.User && txtService_Password.Text.Length > 0;

            if (account == WindowsService.eAccount.User && String.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("The service account needs a user name.");
            }

            if (account == WindowsService.eAccount.User && !passwordEntered
                && (windows.Account != account || !String.Equals(windows.UserName, userName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Enter a password when selecting a different user account.");
            }

            var updated = new WindowsService
            {
                StartMode = startMode,
                Account = account,
                UserName = userName,
                Password = passwordEntered ? SecretStorage.Protect(txtService_Password.Text) : windows.Password,
                DependenciesMode = dependencies,
                CustomDependencies = m_CustomDependencies.ToList()
            };

            bool changed = startMode != windows.StartMode
                || account != windows.Account
                || dependencies != windows.DependenciesMode
                || (windows.UserName ?? String.Empty) != userName
                || passwordEntered
                || !(windows.CustomDependencies ?? new List<String>()).SequenceEqual(updated.CustomDependencies, StringComparer.OrdinalIgnoreCase);

            slist.WindowsSettings = updated;

            return changed;
        }

        /// <summary>Applies settings to an installed service before persisting them.</summary>
        private bool applyWindowsService(ServiceConfig slist, bool accountChanged)
        {
            var writer = new WindowsServiceConfigWriter();

            if (!writer.IsInstalled(Program.ServiceName))
            {
                return true;
            }

            WindowsServiceWriteResult result = null;

            ProgressRunner.Run(this, "Service settings", progress =>
            {
                progress.Report(ProgressReport.Running("Applying the service settings..."));

                result = writer.Apply(Program.ServiceName, slist.WindowsSettings, accountChanged);
            });

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Service settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result.Success;
        }

        private void offerServiceRestart()
        {
            var writer = new WindowsServiceConfigWriter();

            if (!writer.IsInstalled(Program.ServiceName) || !writer.IsRunning(Program.ServiceName))
            {
                return;
            }

            var question = "The service settings were applied." + Environment.NewLine + Environment.NewLine
                + "The service has to be restarted for them to take effect. Every application it runs is stopped with it." + Environment.NewLine + Environment.NewLine
                + "Restart the service now?";

            if (MessageBox.Show(question, "Service settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            WindowsServiceWriteResult restart = null;

            ProgressRunner.Run(this, "Service settings", progress =>
            {
                progress.Report(ProgressReport.Running("Restarting the service..."));

                restart = writer.Restart(Program.ServiceName);
            });

            if (!restart.Success)
            {
                MessageBox.Show(restart.Message, "Service settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void applyAccountFields()
        {
            var user = selectedChoice(cmbService_Account, WindowsService.eAccount.LocalSystem) == WindowsService.eAccount.User;

            lblService_User.Enabled = user;
            txtService_User.Enabled = user;
            lblService_Password.Enabled = user;
            txtService_Password.Enabled = user;
        }

        /// <summary>
        /// The services of the machine, read once and kept. A machine that does not answer offers an empty
        /// list, the configured dependencies stay visible either way.
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
        /// Shows the dependencies of the selected mode with the title of the service in front of its name. A
        /// service that is not installed any more stays in the list, that is something to see before saving.
        /// </summary>
        private void fillDependencyList(WindowsService.eDependencyMode mode)
        {
            List<String> names;

            if (mode == WindowsService.eDependencyMode.StandardNetwork)
            {
                names = WindowsService.StandardNetworkDependencies.ToList();
            }
            else if (mode == WindowsService.eDependencyMode.Custom)
            {
                names = m_CustomDependencies;
            }
            else
            {
                names = new List<String>();
            }

            var all = getServices();

            lsvService_Dependencies.BeginUpdate();
            lsvService_Dependencies.Items.Clear();

            foreach (String name in names)
            {
                var info = all.FirstOrDefault(m => String.Equals(m.ServiceName, name, StringComparison.OrdinalIgnoreCase));
                ListViewItem item = new ListViewItem(info != null ? info.DisplayName : name);

                item.SubItems.Add(name);
                item.SubItems.Add(info != null ? info.Status : "Not installed");

                lsvService_Dependencies.Items.Add(item);
            }

            lsvService_Dependencies.EndUpdate();
        }

        private void applyDependencyFields()
        {
            if (m_LoadingWindowsSettings)
            {
                return;
            }

            var mode = selectedChoice(cmbService_Dependencies, WindowsService.eDependencyMode.StandardNetwork);

            bnService_Dependencies_Select.Enabled = mode == WindowsService.eDependencyMode.Custom;
            fillDependencyList(mode);
            lblService_Hint.Visible = mode != WindowsService.eDependencyMode.None;
        }

        private void cmbService_Account_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyAccountFields();
        }

        private void cmbService_Dependencies_SelectedIndexChanged(object sender, EventArgs e)
        {
            applyDependencyFields();
        }

        private void bnService_Dependencies_Select_Click(object sender, EventArgs e)
        {
            using (var dialog = new FrmServicePicker(getServices(), m_CustomDependencies))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                m_CustomDependencies = dialog.SelectedNames;
                fillDependencyList(WindowsService.eDependencyMode.Custom);
            }
        }

        private static void selectChoice(ComboBox combo, Object value)
        {
            foreach (object item in combo.Items)
            {
                var choice = item as ChoiceItem;

                if (choice != null && Equals(choice.Value, value))
                {
                    combo.SelectedItem = choice;

                    return;
                }
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private static T selectedChoice<T>(ComboBox combo, T fallback)
        {
            var choice = combo.SelectedItem as ChoiceItem;

            return choice == null ? fallback : (T)choice.Value;
        }

        private static String startModeText(WindowsService.eStartMode mode)
        {
            switch (mode)
            {
                case WindowsService.eStartMode.AutomaticDelayed:

                    return "Automatic (delayed start)";

                case WindowsService.eStartMode.Manual:

                    return "Manual";

                case WindowsService.eStartMode.Disabled:

                    return "Disabled";

                default:

                    return "Automatic";
            }
        }

        private static String accountText(WindowsService.eAccount account)
        {
            switch (account)
            {
                case WindowsService.eAccount.LocalService:

                    return "Local Service account";

                case WindowsService.eAccount.NetworkService:

                    return "Network Service account";

                case WindowsService.eAccount.User:

                    return "A named user account";

                default:

                    return "Local System account";
            }
        }

        private static String dependencyText(WindowsService.eDependencyMode mode)
        {
            switch (mode)
            {
                case WindowsService.eDependencyMode.None:

                    return "No dependencies";

                case WindowsService.eDependencyMode.Custom:

                    return "Own list";

                default:

                    return "Standard network";
            }
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

            slist.RuntimeSettings.Dependency_Wait_Timeout = (int)nudDependency_Wait_Timeout.Value;

            slist.RuntimeSettings.RequireAdministrator = chkRequireAdministrator.Checked;

            try
            {
                slist.saveChecked(ServiceConfig.ConfigPath + ServiceConfig.Filename);
                this.Close();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is Newtonsoft.Json.JsonException)
            {
                MessageBox.Show("The settings could not be saved." + Environment.NewLine + ex.Message,
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bnWindowsService_Save_Click(object sender, EventArgs e)
        {
            var slist = ServiceConfig.load();

            try
            {
                WindowsService previous = slist.WindowsSettings;
                bool serviceChanged = applyWindowsSettings(slist);
                bool accountChanged = previous.Account != slist.WindowsSettings.Account
                    || (slist.WindowsSettings.UsesPassword()
                        && (!String.Equals(previous.UserName, slist.WindowsSettings.UserName, StringComparison.OrdinalIgnoreCase)
                            || previous.Password != slist.WindowsSettings.Password));

                if (serviceChanged && !applyWindowsService(slist, accountChanged))
                {
                    return;
                }

                slist.saveChecked(ServiceConfig.ConfigPath + ServiceConfig.Filename);

                if (accountChanged && slist.WindowsSettings.StartMode != WindowsService.eStartMode.Disabled)
                {
                    offerServiceRestart();
                }

                this.Close();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is Win32Exception
                || ex is InvalidOperationException || ex is CryptographicException || ex is FormatException
                || ex is IOException || ex is UnauthorizedAccessException || ex is Newtonsoft.Json.JsonException)
            {
                MessageBox.Show("The settings could not be saved or applied. Check the Windows service configuration before retrying."
                    + Environment.NewLine + ex.Message, "Service settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        /// <summary>Entry of a drop down list.</summary>
        private sealed class ChoiceItem
        {
            public ChoiceItem(String text, Object value)
            {
                Text = text;
                Value = value;
            }

            public String Text { get; }

            public Object Value { get; }

            public override String ToString() => Text;
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
