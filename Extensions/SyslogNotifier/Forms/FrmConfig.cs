using ServiceManager.Extensions.Interfaces;
using System;
using System.Windows.Forms;

namespace ServiceManager.Extensions.SyslogNotifier.Forms
{
    public partial class FrmConfig : Form
    {
        public FrmConfig()
        {
            InitializeComponent();
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            var config = Config.Config.load(Config.Config.FilePath);

            txtHost.Text = config.Host ?? "";
            txtPort.Text = config.Port.ToString();
            chkTcp.Checked = config.UseTcp;
            chk5424.Checked = config.UseRfc5424;
            txtHostname.Text = config.Hostname ?? "";
            txtApp.Text = config.AppName ?? "";
            txtFacility.Text = config.Facility.ToString();
            txtMax.Text = config.MaxLength.ToString();
            txtTimeout.Text = config.TimeoutSeconds.ToString();
        }

        /// <summary>
        /// Reads the dialog into the settings file. Returns false when a number was not a number, the
        /// settings file is untouched in that case.
        /// </summary>
        private bool save()
        {
            var config = Config.Config.load(Config.Config.FilePath);

            config.Host = txtHost.Text.Trim();
            config.UseTcp = chkTcp.Checked;
            config.UseRfc5424 = chk5424.Checked;
            config.Hostname = txtHostname.Text.Trim();
            config.AppName = txtApp.Text.Trim();

            int port;
            if (!Int32.TryParse(txtPort.Text.Trim(), out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("The port has to be a number between 1 and 65535.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int facility;
            if (!Int32.TryParse(txtFacility.Text.Trim(), out facility) || facility < 0 || facility > 23)
            {
                MessageBox.Show("The facility has to be a number between 0 and 23.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int max;
            if (!Int32.TryParse(txtMax.Text.Trim(), out max) || max < 64)
            {
                MessageBox.Show("The maximum length has to be a number of 64 bytes or more.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int timeout;
            if (!Int32.TryParse(txtTimeout.Text.Trim(), out timeout) || timeout < 1)
            {
                MessageBox.Show("The timeout seconds have to be a number greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            config.Port = port;
            config.Facility = facility;
            config.MaxLength = max;
            config.TimeoutSeconds = timeout;

            config.save(Config.Config.FilePath);

            return true;
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            this.Close();
        }

        /// <summary>
        /// Sends one line through the provider and reports what the network said. Over UDP the answer
        /// only says that nothing complained, so the collector itself has to show the line.
        /// </summary>
        private async void bnTest_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            var notifier = new SyslogNotifier();

            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleError,
                Timestamp = DateTime.Now,
                InstanceName = "ServiceManager",
                ServiceTitle = "Test",
                Text = "Test line from ServiceManager",
                Details = "A second line with \"quotes\" and a \\ backslash"
            };

            // A connect that has to wait for its timeout would otherwise simply be started again.
            bnTest.Enabled = false;

            String error;
            try
            {
                error = await notifier.Test(message);
            }
            finally
            {
                bnTest.Enabled = true;
            }

            if (error == null)
                MessageBox.Show("The line was sent to " + txtHost.Text.Trim() + ":" + txtPort.Text.Trim() + ". Look for it in the collector.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("The line was not sent:\r\n" + error,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
