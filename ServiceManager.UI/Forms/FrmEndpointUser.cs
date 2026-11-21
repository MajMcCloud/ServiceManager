using ServiceManager.Base.Data.Settings;
using System;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// Asks for the Windows account that may connect to the endpoint.
    /// </summary>
    public partial class FrmEndpointUser : Form
    {
        /// <summary>The account the user confirmed, set when Save closes the dialog.</summary>
        public EndpointUser User { get; private set; }

        public FrmEndpointUser()
        {
            InitializeComponent();
        }

        public FrmEndpointUser(EndpointUser User) : this()
        {
            txtName.Text = User.Name;
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            String name = txtName.Text.Trim();

            if (name.Length == 0)
            {
                MessageBox.Show("The account needs a name.", "Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.User = new EndpointUser(name);

            this.DialogResult = DialogResult.OK;
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
