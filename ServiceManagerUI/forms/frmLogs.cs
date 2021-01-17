using ServiceManager.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace ServiceManager.UI.forms
{
    public partial class frmLogs : RadForm
    {
        const int EM_LINESCROLL = 0x00B6;


        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar,
                               int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg,
                                       int wParam, int lParam);

        public Guid serviceId { get; set; }

        public bool LiveLogs = false;

        public int SelectionCursor = 0;


        public Channel Connection { get; set; }

        public frmLogs()
        {
            InitializeComponent();
        }

        private void frmLogs_Load(object sender, EventArgs e)
        {


            this.Text += "Service: " + serviceId.ToString();

            txtContent.Text = "LOADING...";

        }

        private void frmLogs_Shown(object sender, EventArgs e)
        {
            StartLogging();
        }

        private void StartLogging()
        {

            var logs = this.Connection.TryCatch(a => a.GetServiceLogs(serviceId));

            if (logs != null)
            {
                txtContent.Text = "";

                txtContent.AppendText(logs.Logs);
            }


            txtContent.SelectionStart = txtContent.Text.Length;

            txtContent.ScrollToCaret();

            this.Connection.Async(a => a.GetServicesAnalytics(), b =>
            {
                Invoke((Action)(() =>
                {
                    var service = b.Analytics.FirstOrDefault(a => a.ServiceID == serviceId);

                    ClientCallback_ServiceChanged(null, new Base.wcf.args.ServiceChangedEventArgs() { Status = service.Status, ServiceId = service.ServiceID });
                }));
            });

            if (LiveLogs)
            {
                this.Text += " (Live)";

                this.Connection.ClientCallback.LiveLogs += ClientCallback_LiveLogs;
                this.Connection.ClientCallback.ServiceChanged += ClientCallback_ServiceChanged;

                this.Connection.Async(a => a.BeginLiveLogs(serviceId));
            }
            else
            {
                txtInput.ReadOnly = true;
            }
        }

        private void ClientCallback_ServiceChanged(object sender, Base.wcf.args.ServiceChangedEventArgs e)
        {

            if (e.ServiceId != serviceId)
                return;

            switch (e.Status)
            {
                case Base.data.ServiceAnalytics.eStatus.running:

                    tsmiStatus.Image = Properties.Resources.led_on;
                    tsmiStatus.Text = "Service is running.";

                    tsmiService_Restart.Text = "Restart service";
                    tsmiService_Shutdown.Enabled = true;

                    break;
                case Base.data.ServiceAnalytics.eStatus.offline:
                case Base.data.ServiceAnalytics.eStatus.start_failed:
                case Base.data.ServiceAnalytics.eStatus.failed:

                    tsmiStatus.Image = Properties.Resources.led_off;
                    tsmiStatus.Text = "Service is offline.";

                    tsmiService_Restart.Text = "Start service";
                    tsmiService_Shutdown.Enabled = false;

                    break;

            }

        }

        private void ClientCallback_LiveLogs(object sender, Base.wcf.args.LiveLogsEventArgs e)
        {
            if (e.ServiceId != serviceId)
                return;

            int scroll = GetScrollPos(txtContent.Handle, 1);

            if (tsmiLogs_Autoscroll.Checked)
            {
                txtContent.AppendText(e.Logs + "\r\n");
            }
            else
            {
                txtContent.Text += (e.Logs + "\r\n");

                SetScrollPos(txtContent.Handle, 1, scroll, true);
                SendMessage(txtContent.Handle, EM_LINESCROLL, 0, scroll);
            }

        }

        private void frmLogs_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (LiveLogs)
            {
                this.Connection.ClientCallback.LiveLogs -= ClientCallback_LiveLogs;
                this.Connection.ClientCallback.ServiceChanged -= ClientCallback_ServiceChanged;

                var t = new Task(() =>
                {
                    this.Connection.TryCatch(a => a.EndLiveLogs(serviceId));
                });

            }

        }

        private void tsmiLogs_Autoscroll_Click(object sender, EventArgs e)
        {

        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            txtContent.AppendText(txtInput.Text + "\r\n");

            this.Connection.TryCatch(a => { a.SendInput(serviceId, txtInput.Text); return true; }, (b, c) => { MessageBox.Show("Timeout", "Timeout"); });

            txtInput.Text = "";

        }

        private void tsmiService_Shutdown_Click(object sender, EventArgs e)
        {
            this.Connection.Client.ShutdownService(this.serviceId);
        }

        private void tsmiService_Restart_Click(object sender, EventArgs e)
        {
            this.Connection.Client.RestartService(this.serviceId);
        }

        private void frmLogs_Activated(object sender, EventArgs e)
        {
            txtInput.Select();
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Return)
            {
                SelectionCursor = 0;
                return;
            }


            int index = 0;

            int currentIndex = 0;

            txtContent.SelectAll();
            txtContent.SelectionBackColor = Color.Black;

            if (txtSearch.Text.Trim() == "")
                return;

            int lastIndex = txtContent.Text.LastIndexOf(txtSearch.Text);

            while (index < lastIndex)
            {
                txtContent.Find(txtSearch.Text, index, txtContent.TextLength, RichTextBoxFinds.None);

                txtContent.SelectionBackColor = Color.Red;


                index = txtContent.Text.IndexOf(txtSearch.Text, index) + 1;

                if (currentIndex == 0 && index >= SelectionCursor)
                {
                    if (index > SelectionCursor)
                    {
                        txtContent.SelectionStart = index;
                        txtContent.ScrollToCaret();
                        currentIndex = index;
                        SelectionCursor = index;
                    }

                    if (index == lastIndex + 1) //Reset
                    {
                        SelectionCursor = txtContent.Text.IndexOf(txtSearch.Text, 0);
                    }


                }
            }


        }
    }
}
