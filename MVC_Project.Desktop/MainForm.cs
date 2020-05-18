using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MVC_Project.Desktop.Helpers;
using MVC_Project.Desktop.Reports;

namespace MVC_Project.Desktop
{
    public partial class MainForm : Form
    {
        Form productForm;
        public MainForm()
        {
            InitializeComponent();
            statusBarUsername.Text = string.Format("Bienvenido, {0}!", Authenticator.GetCurrentUser().FirstName);
        }

        private void agregarProductoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (productForm == null)
            {
                productForm = new ProductForm();
                productForm.MdiParent = this;
                productForm.FormClosed += new FormClosedEventHandler(formClosedEvt);
                productForm.Show();
            } 
            else
            {
                productForm.Activate();

            }
            
        }

        private void formClosedEvt(object sender, FormClosedEventArgs e)
        {
           sender = null;
        }

        private void appClosedEvt(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void usuariosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AdminUsersForm usersForm = new AdminUsersForm();
            usersForm.MdiParent = this;
            usersForm.Show();
        }

        private void reporteDePagosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PaymentsReport reportForm = new PaymentsReport();
            reportForm.MdiParent = this;
            reportForm.Show();
        }
    }
}
