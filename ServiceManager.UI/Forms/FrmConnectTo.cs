using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    public partial class FrmConnectTo : Form
    {
        public String IPOrHost { get; set; }

        public int Port { get; set; }

        public FrmConnectTo()
        {
            InitializeComponent();
        }

        private void bnConnect_Click(object sender, EventArgs e)
        {
            int p = 0;
            if (!int.TryParse(txtPort.Text, out p))
                return;

            if (p <= 0 | p >= 65535)
                return;

            if (txtHost.Text.Trim() == "")
                return;

            this.IPOrHost = txtHost.Text;
            this.Port = p;

            this.DialogResult = DialogResult.OK;
        }
    }
}
