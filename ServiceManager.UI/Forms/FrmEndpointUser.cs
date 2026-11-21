using ServiceManager.Base.Data;
using ServiceManager.Base.Data.Settings;
using System;
using System.Windows.Forms;

namespace ServiceManager.UI.Forms
{
    /// <summary>
    /// Asks for the Windows account of an endpoint user and the ServiceManager password it answers with.
    /// </summary>
    public partial class FrmEndpointUser : Form
    {
        /// <summary>The account that is being edited, null while a new one is typed.</summary>
        private EndpointUser m_Existing;

        /// <summary>The account the user confirmed, set when Save closes the dialog.</summary>
        public EndpointUser User { get; private set; }

        public FrmEndpointUser()
        {
            InitializeComponent();
        }

        public FrmEndpointUser(EndpointUser User) : this()
        {
            m_Existing = User;

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

            //The stored password is never shown in the dialog, so an empty box keeps the old one
            if (txtPassword.Text.Length > 0)
            {
                User = new EndpointUser(name, SecretStorage.Protect(txtPassword.Text));
            }
            else if (m_Existing != null)
            {
                User = new EndpointUser(name, m_Existing.Password);
            }
            else
            {
                MessageBox.Show("A new account needs a password.", "Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
