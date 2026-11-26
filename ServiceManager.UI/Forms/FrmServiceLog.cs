using ServiceManager.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// Shows the log files the service writes. A service has no console, so this window is where the
    /// lines of a run are read without opening the folder by hand.
    /// </summary>
    public partial class FrmServiceLog : Form
    {
        // A day file grows while the service runs, only its newest lines are worth showing.
        private const int MaxLines = 2000;

        private static readonly Color InfoColor = Color.Silver;

        private static readonly Color WarningColor = Color.FromArgb(255, 210, 120);

        private static readonly Color ErrorColor = Color.FromArgb(255, 128, 128);

        private Timer m_RefreshTimer;

        public FrmServiceLog()
        {
            InitializeComponent();
        }

        private void FrmServiceLog_Load(object sender, EventArgs e)
        {
            m_RefreshTimer = new Timer();
            m_RefreshTimer.Interval = 2000;
            m_RefreshTimer.Tick += new EventHandler(m_RefreshTimer_Tick);

            ReloadFiles();
        }

        private void FrmServiceLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_RefreshTimer != null)
            {
                m_RefreshTimer.Stop();
                m_RefreshTimer.Dispose();
                m_RefreshTimer = null;
            }
        }

        private void tsmiLog_Refresh_Click(object sender, EventArgs e)
        {
            ReloadFiles();
        }

        private void tsmiLog_AutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if (m_RefreshTimer == null)
            {
                return;
            }

            if (tsmiLog_AutoRefresh.Checked)
            {
                m_RefreshTimer.Start();
            }
            else
            {
                m_RefreshTimer.Stop();
            }
        }

        private void tsmiLog_OpenFolder_Click(object sender, EventArgs e)
        {
            var folder = SMLog.LogPath;

            if (!Directory.Exists(folder))
            {
                var failure = SMLog.WriteFailure;

                var text = failure == null ? "No log was written yet." : "No log was written yet:\r\n" + failure.Message;

                MessageBox.Show(text + "\r\n\r\n" + folder, "Service log", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                SMLog.Warning("ServiceManager: the log folder could not be opened", ex);

                MessageBox.Show("The folder could not be opened:\r\n" + folder, "Service log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowFile(SelectedFile());
        }

        private void m_RefreshTimer_Tick(object sender, EventArgs e)
        {
            ShowFile(SelectedFile());
        }

        /// <summary>
        /// Fills the day list again and keeps the day that was open. A run that crosses midnight gains a
        /// new entry, the window must not jump away from the day being read.
        /// </summary>
        private void ReloadFiles()
        {
            var files = SMLog.GetFiles();

            var selected = SelectedFile();

            cboFiles.BeginUpdate();

            cboFiles.Items.Clear();

            foreach (var file in files)
            {
                cboFiles.Items.Add(Path.GetFileName(file));
            }

            var index = IndexOf(files, selected);

            if (index < 0 && files.Count > 0)
            {
                index = 0;
            }

            if (index >= 0)
            {
                cboFiles.SelectedIndex = index;
            }

            cboFiles.EndUpdate();

            if (files.Count == 0)
            {
                var failure = SMLog.WriteFailure;

                txtContent.Text = failure == null ? "" : "No log file was written.\r\n\r\n" + failure.Message;

                tsmiInfo.Text = "no log file";

                return;
            }

            ShowFile(SelectedFile());
        }

        private String SelectedFile()
        {
            if (cboFiles.SelectedItem == null)
            {
                return null;
            }

            return Path.Combine(SMLog.LogPath, cboFiles.SelectedItem.ToString());
        }

        private static int IndexOf(IEnumerable<String> files, String file)
        {
            if (String.IsNullOrEmpty(file))
            {
                return -1;
            }

            var name = Path.GetFileName(file);

            var index = 0;

            foreach (var one in files)
            {
                if (String.Equals(Path.GetFileName(one), name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        private void ShowFile(String file)
        {
            if (String.IsNullOrEmpty(file) || !File.Exists(file))
            {
                tsmiInfo.Text = "no log file";

                return;
            }

            var lines = SMLog.ReadTail(file, MaxLines);

            ShowLines(lines);

            tsmiInfo.Text = Path.GetFileName(file) + " · " + lines.Count + " lines";
        }

        /// <summary>
        /// Hands the lines to the text box and colours the levels. Every line carries its own mark, so a
        /// reader picks the problems out of a day of ordinary lines without searching.
        /// </summary>
        private void ShowLines(IList<String> lines)
        {
            txtContent.SelectAll();
            txtContent.SelectionColor = InfoColor;

            txtContent.Text = String.Join("\n", lines);

            var position = 0;

            foreach (var line in lines)
            {
                var color = ColorOf(line);

                if (color != null)
                {
                    txtContent.Select(position, line.Length);
                    txtContent.SelectionColor = color.Value;
                }

                //The text is joined with one newline per line, so the separator counts as one character
                position += line.Length + 1;
            }

            txtContent.SelectionLength = 0;
            txtContent.SelectionColor = InfoColor;

            txtContent.SelectionStart = txtContent.TextLength;
            txtContent.ScrollToCaret();
        }

        private static Color? ColorOf(String line)
        {
            if (line.Contains("] [ERROR]"))
            {
                return ErrorColor;
            }

            if (line.Contains("] [WARN]"))
            {
                return WarningColor;
            }

            return null;
        }
    }
}
