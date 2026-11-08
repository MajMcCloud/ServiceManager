using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManager.Extensions.EventLogNotifier.Forms
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

            chkWriteToEventLog.Checked = config.WriteToEventLog;
            chkShowBalloon.Checked = config.ShowBalloonTip;

            txtEventLog.Text = config.EventLog ?? "";
            txtSource.Text = config.Source ?? "";
            txtEventId.Text = config.EventId.ToString();
            txtBalloonSeconds.Text = config.BalloonSeconds.ToString();
        }

        /// <summary>
        /// Reads the dialog into the settings file. Returns false when a number was not a number, the
        /// settings file is untouched in that case.
        /// </summary>
        private bool save()
        {
            var config = Config.Config.load(Config.Config.FilePath);

            config.WriteToEventLog = chkWriteToEventLog.Checked;
            config.ShowBalloonTip = chkShowBalloon.Checked;

            config.EventLog = txtEventLog.Text.Trim();
            config.Source = txtSource.Text.Trim();

            int eventId;
            if (!Int32.TryParse(txtEventId.Text.Trim(), out eventId))
            {
                MessageBox.Show("The first event id has to be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            int seconds;
            if (!Int32.TryParse(txtBalloonSeconds.Text.Trim(), out seconds))
            {
                MessageBox.Show("The balloon seconds have to be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            config.EventId = eventId;
            config.BalloonSeconds = seconds;

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
        /// Sends one entry of every kind through the provider, so the log, the ids and the balloon can
        /// be checked without waiting for a service to crash.
        /// </summary>
        private void bnTest_Click(object sender, EventArgs e)
        {
            if (!save())
                return;

            var notifier = new EventLogNotifier();

            var kinds = new[]
            {
                NotificationKind.ServerStart,
                NotificationKind.ServiceStart,
                NotificationKind.ServiceRestart,
                NotificationKind.ConsoleError
            };

            foreach (var kind in kinds)
            {
                notifier.Send(new NotificationMessage()
                {
                    Kind = kind,
                    Timestamp = DateTime.Now,
                    InstanceName = "ServiceManager",
                    ServiceTitle = "Test",
                    Text = "Testentry from ServiceManager (" + kind + ")"
                });
            }

            MessageBox.Show("Testentries sent. Check the event log \"" + txtEventLog.Text.Trim() + "\" and the desktop.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
