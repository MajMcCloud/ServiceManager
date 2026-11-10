using ServiceManager.Base.Progress;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// The dialog every longer operation runs behind. The work itself runs on a worker thread, so the
    /// bar keeps moving and the window stays responsive. An operation cannot be interrupted: the
    /// service or the connection would be left half changed, so the dialog offers no way out.
    /// </summary>
    public partial class FrmProgress : Form
    {
        private readonly Action<IProgress<ProgressReport>> m_Work;

        private readonly IProgress<ProgressReport> m_Report;

        private bool m_Finished;

        /// <summary>The error the work threw, null when it went through.</summary>
        public Exception Error { get; private set; }

        public FrmProgress(String title, Action<IProgress<ProgressReport>> work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            m_Work = work;

            InitializeComponent();

            this.Text = title;

            // Progress reports on the context of the thread that created it, which is the one showing the dialog.
            m_Report = new System.Progress<ProgressReport>(apply);
        }

        private async void FrmProgress_Load(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => m_Work(m_Report));
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                m_Finished = true;

                Close();
            }
        }

        private void apply(ProgressReport report)
        {
            if (report == null)
            {
                return;
            }

            lblMessage.Text = report.Message;

            if (report.IsIndeterminate)
            {
                pbProgress.Style = ProgressBarStyle.Marquee;
                lblPercent.Text = String.Empty;

                return;
            }

            pbProgress.Style = ProgressBarStyle.Continuous;
            pbProgress.Value = report.Percent;

            lblPercent.Text = report.Percent + "%";
        }

        private void FrmProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Neither the close box nor Escape may end the dialog while the operation is still running.
            if (!m_Finished)
            {
                e.Cancel = true;
            }
        }
    }
}
