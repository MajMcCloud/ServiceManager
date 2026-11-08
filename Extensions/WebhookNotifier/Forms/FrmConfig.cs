using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ServiceManager.Extensions.WebhookNotifier.Forms
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

            if (config == null)
                config = new Config.Config();

            txtUrl.Text = config.Url ?? "";

            SelectMethod(config.Method);

            txtContentType.Text = config.ContentType ?? "";
            txtTimeout.Text = config.TimeoutSeconds.ToString();
            txtRetries.Text = config.Retries.ToString();
            chkIgnoreCertificateErrors.Checked = config.IgnoreCertificateErrors;

            txtHeaders.Text = config.Headers == null ? "" : String.Join("\r\n", config.Headers);

            txtBody.Text = String.IsNullOrEmpty(config.BodyTemplate) ? Config.Config.DefaultBody : config.BodyTemplate;
        }

        /// <summary>
        /// A settings file may name a method the combo does not offer. It is added instead of quietly
        /// switching the user to POST.
        /// </summary>
        private void SelectMethod(String method)
        {
            var name = (method ?? "").Trim().ToUpperInvariant();

            if (name == "")
                name = "POST";

            for (var index = 0; index < cmbMethod.Items.Count; index++)
            {
                if (String.Equals((String)cmbMethod.Items[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    cmbMethod.SelectedIndex = index;
                    return;
                }
            }

            cmbMethod.Items.Add(name);
            cmbMethod.SelectedIndex = cmbMethod.Items.Count - 1;
        }


        /// <summary>
        /// Reads the dialog into the settings file. Returns false when a value was not usable, the
        /// settings file is untouched in that case.
        /// </summary>
        private bool save()
        {
            var config = Config.Config.load(Config.Config.FilePath);

            if (config == null)
                config = new Config.Config();

            var url = txtUrl.Text.Trim();

            if (url != "" &&
                !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The url has to start with http:// or https://.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int timeout;
            if (!Int32.TryParse(txtTimeout.Text.Trim(), out timeout) || timeout <= 0)
            {
                MessageBox.Show("The timeout has to be a number of seconds greater than zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int retries;
            if (!Int32.TryParse(txtRetries.Text.Trim(), out retries) || retries < 0)
            {
                MessageBox.Show("The retries have to be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var headers = ReadHeaders();

            if (headers == null)
                return false;

            config.Url = url;
            config.Method = (String)cmbMethod.SelectedItem;
            config.ContentType = txtContentType.Text.Trim();
            config.TimeoutSeconds = timeout;
            config.Retries = retries;
            config.IgnoreCertificateErrors = chkIgnoreCertificateErrors.Checked;
            config.Headers = headers;
            config.BodyTemplate = txtBody.Text;

            config.save(Config.Config.FilePath);

            return true;
        }


        private List<String> ReadHeaders()
        {
            var lines = new List<String>();

            foreach (var line in txtHeaders.Text.Split('\n'))
            {
                var value = line.Trim();

                if (value == "")
                    continue;

                if (value.IndexOf(':') <= 0)
                {
                    MessageBox.Show("The header \"" + value + "\" has to look like \"Name: value\".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                lines.Add(value);
            }

            return lines;
        }


        private void bnSave_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            this.Close();
        }

        /// <summary>
        /// Sends one event through the provider and reports what the endpoint answered. The answer of
        /// the endpoint is the fastest way to find a wrong body or a rejected header.
        /// </summary>
        private async void bnTest_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            var notifier = new WebhookNotifier();

            var message = new NotificationMessage()
            {
                Kind = NotificationKind.ConsoleError,
                Timestamp = DateTime.Now,
                InstanceName = "ServiceManager",
                ServiceTitle = "Test",
                Text = "Test message from ServiceManager",
                Details = "A second line with \"quotes\" and a \\ backslash"
            };

            // A request that has to wait for its timeout would otherwise simply be started again.
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
                MessageBox.Show("The endpoint accepted the message.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("The endpoint did not accept the message:\r\n" + error,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void bnDefaultBody_Click(object sender, EventArgs e)
        {
            txtBody.Text = Config.Config.DefaultBody;
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
