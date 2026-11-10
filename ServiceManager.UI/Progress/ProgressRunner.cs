using ServiceManager.Base.Progress;
using ServiceManager.UI.Forms;
using System;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace ServiceManager.UI.Progress
{
    /// <summary>
    /// Shows the progress dialog, runs the work behind it and hands the error of that work back to the
    /// caller. The error keeps its original type, so the messages and the catch filters written for the
    /// operation still apply once the dialog is gone.
    /// </summary>
    public static class ProgressRunner
    {
        /// <summary>
        /// Runs work on a worker thread behind a modal dialog. The call returns when the work is through.
        /// </summary>
        public static void Run(Form owner, String title, Action<IProgress<ProgressReport>> work)
        {
            using (FrmProgress dialog = new FrmProgress(title, work))
            {
                dialog.ShowDialog(owner);

                if (dialog.Error != null)
                {
                    ExceptionDispatchInfo.Capture(dialog.Error).Throw();
                }
            }
        }
    }
}
