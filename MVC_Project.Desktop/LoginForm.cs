using MVC_Project.Desktop.Helpers;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity;

namespace MVC_Project.Desktop
{
    public partial class LoginForm : Form
    {

        private IUserService _userService;

        private IAuthService _authService;

        public LoginForm()
        {

            _authService = UnityHelper.Resolve<IAuthService>();
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtUsername.Text) || String.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please provide userName and password");
                return;
            }

            string pass = Utils.SecurityUtil.EncryptPassword(txtPassword.Text.Trim());
            User user = _authService.Authenticate(txtUsername.Text.Trim(), pass);

            if (user == null)
            {
                MessageBox.Show("ERROR: No se puede iniciar sesión");
                return;
            }
            else
            {
                AuthUser authUser = new AuthUser()
                {
                    Id = user.id,
                    Email = user.name,
                    FirstName = user.profile.firstName,
                    LastName = user.profile.lastName,
                    Uuid = user.uuid.ToString()
                };
                Authenticator.SetCurrentUser(authUser);
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.Show();
            }
        }

        private void CheckEnterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                btnLogin_Click(null, null);
            }
        }
    }
}
