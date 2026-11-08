using ServiceManager.Extensions.Interfaces;
using System;
using System.Windows.Forms;

namespace ServiceManager.Extensions.ScriptNotifier.Forms
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

            txtPath.Text = config.ScriptPath ?? "";
            txtArgs.Text = config.Arguments ?? "";
            txtWorkdir.Text = config.WorkingDirectory ?? "";
            chkStdin.Checked = config.WriteJsonToStdin;
            chkArgument.Checked = config.PassJsonAsArgument;
            txtTimeout.Text = config.TimeoutSeconds.ToString();
        }

        /// <summary>
        /// Reads the dialog into the settings file. Returns false when a number was not a number, the
        /// settings file is untouched in that case.
        /// </summary>
        private bool save()
        {
            var config = Config.Config.load(Config.Config.FilePath);

            config.ScriptPath = txtPath.Text.Trim();
            config.Arguments = txtArgs.Text.Trim();
            config.WorkingDirectory = txtWorkdir.Text.Trim();
            config.WriteJsonToStdin = chkStdin.Checked;
            config.PassJsonAsArgument = chkArgument.Checked;

            int timeout;
            if (!Int32.TryParse(txtTimeout.Text.Trim(), out timeout) || timeout < 1)
            {
                MessageBox.Show("The timeout seconds have to be a number greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

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
        /// Runs the program once with an event of its own and reports the exit code. A script that is
        /// written for the json of the provider answers here, not in the console of the service.
        /// </summary>
        private async void bnTest_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            var notifier = new ScriptNotifier();

            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleError,
                Timestamp = DateTime.Now,
                InstanceName = "ServiceManager",
                ServiceTitle = "Test",
                Text = "Test event from ServiceManager",
                Details = "A second line with \"quotes\" and a \\ backslash"
            };

            // A program that has to wait for its timeout would otherwise simply be started again.
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
                MessageBox.Show("The program finished without an error.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("The program did not accept the event:\r\n" + error,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
