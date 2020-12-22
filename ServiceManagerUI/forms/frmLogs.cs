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
            try
            {
                var logs = this.Connection.Client.GetServiceLogs(serviceId);

                txtContent.Text = "";

                txtContent.AppendText(logs.Logs);
            }
            catch
            {

            }

            txtContent.SelectionStart = txtContent.Text.Length;

            txtContent.ScrollToCaret();

            var services = this.Connection.Client.GetServicesAnalytics();

            var service = services.Analytics.FirstOrDefault(a => a.ServiceID == serviceId);

            ClientCallback_ServiceChanged(null, new ServiceManager.Base.wcf.args.ServiceChangedEventArgs() { State = (service.IsRunning ? ServiceManager.Base.wcf.args.ServiceChangedEventArgs.eState.started : ServiceManager.Base.wcf.args.ServiceChangedEventArgs.eState.exited) });

            if (LiveLogs)
            {
                this.Text += " (Live)";

                this.Connection.ClientCallback.LiveLogs += ClientCallback_LiveLogs;
                this.Connection.ClientCallback.ServiceChanged += ClientCallback_ServiceChanged;

                this.Connection.Client.BeginLiveLogs(serviceId);
            }
            else
            {
                txtInput.ReadOnly = true;
            }
        }

        private void ClientCallback_ServiceChanged(object sender, ServiceManager.Base.wcf.args.ServiceChangedEventArgs e)
        {

            if (e.ServiceId != serviceId)
                return;

            switch (e.State)
            {
                case ServiceManager.Base.wcf.args.ServiceChangedEventArgs.eState.started:

                    tsmiStatus.Image = Properties.Resources.led_on;
                    tsmiStatus.Text = "Service is running.";

                    break;
                case ServiceManager.Base.wcf.args.ServiceChangedEventArgs.eState.exited:

                    tsmiStatus.Image = Properties.Resources.led_off;
                    tsmiStatus.Text = "Service is offline.";

                    break;
            }

        }

        private void ClientCallback_LiveLogs(object sender, ServiceManager.Base.wcf.args.LiveLogsEventArgs e)
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
            this.Connection.Client.ShutdownService(this.serviceId, false);
        }

        private void tsmiService_Restart_Click(object sender, EventArgs e)
        {
            this.Connection.Client.RestartService(this.serviceId);
        }

        private void frmLogs_Activated(object sender, EventArgs e)
        {
            txtInput.Select();
        }
    }
}
