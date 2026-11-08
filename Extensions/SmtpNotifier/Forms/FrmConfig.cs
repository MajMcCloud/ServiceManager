using ServiceManager.Extensions.Interfaces;
using System;
using System.Windows.Forms;

namespace ServiceManager.Extensions.SmtpNotifier.Forms
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

            txtServer.Text = config.SmtpServer ?? "";
            txtPort.Text = config.SmtpPort.ToString();
            chkStartTls.Checked = config.UseStartTls;
            txtUser.Text = config.Username ?? "";
            txtPassword.Text = config.Password ?? "";
            txtFrom.Text = config.FromAddress ?? "";
            txtTo.Text = config.ToAddresses ?? "";
            txtPrefix.Text = config.SubjectPrefix ?? "";
            txtTimeout.Text = config.TimeoutSeconds.ToString();
            txtThrottle.Text = config.ThrottleSeconds.ToString();
            txtPickup.Text = config.PickupDirectory ?? "";
        }

        /// <summary>
        /// Reads the dialog into the settings file. Returns false when a number was not a number, the
        /// settings file is untouched in that case.
        /// </summary>
        private bool save()
        {
            var config = Config.Config.load(Config.Config.FilePath);

            config.SmtpServer = txtServer.Text.Trim();
            config.UseStartTls = chkStartTls.Checked;
            config.Username = txtUser.Text.Trim();
            config.Password = txtPassword.Text;
            config.FromAddress = txtFrom.Text.Trim();
            config.ToAddresses = txtTo.Text.Trim();
            config.SubjectPrefix = txtPrefix.Text.Trim();
            config.PickupDirectory = txtPickup.Text.Trim();

            int port;
            if (!Int32.TryParse(txtPort.Text.Trim(), out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("The port has to be a number between 1 and 65535.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int timeout;
            if (!Int32.TryParse(txtTimeout.Text.Trim(), out timeout) || timeout < 1)
            {
                MessageBox.Show("The timeout seconds have to be a number greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int throttle;
            if (!Int32.TryParse(txtThrottle.Text.Trim(), out throttle) || throttle < 0)
            {
                MessageBox.Show("The throttle seconds have to be a number of zero or greater.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            config.SmtpPort = port;
            config.TimeoutSeconds = timeout;
            config.ThrottleSeconds = throttle;

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
        /// Sends one mail through the provider and reports what the server answered. A mail that never
        /// arrives is usually a rejected sender or a server that waits for credentials, both of them
        /// only visible in the answer of the server.
        /// </summary>
        private async void bnTest_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            var notifier = new SmtpNotifier();

            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleError,
                Timestamp = DateTime.Now,
                InstanceName = "ServiceManager",
                ServiceTitle = "Test",
                Text = "Test mail from ServiceManager",
                Details = "A second line with \"quotes\" and a \\ backslash"
            };

            // A send that has to wait for its timeout would otherwise simply be started again.
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
                MessageBox.Show("The mail was sent to " + txtTo.Text.Trim() + ".", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("The mail was not sent:\r\n" + error,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
