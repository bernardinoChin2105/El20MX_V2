using MVC_Project.Desktop.Helpers;
using MVC_Project.Desktop.Users;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MVC_Project.Desktop
{
    public partial class AdminUsersForm : Form
    {
        private IUserService _userService;
        private Int32? RecordId;
        public AdminUsersForm()
        {
            _userService = UnityHelper.Resolve<IUserService>();
            InitializeComponent();
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            dtvUsers.DataSource = _userService.GetAll();
        }

        private void dtvUsers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Initiate columns
            foreach (DataGridViewColumn i in dtvUsers.Columns)
            {
                i.SortMode = DataGridViewColumnSortMode.NotSortable;
                i.Visible = false;
            }

            dtvUsers.Columns["Id"].Visible = true;
            dtvUsers.Columns["Username"].Visible = true;
            dtvUsers.Columns["FirstName"].Visible = true;
            dtvUsers.Columns["FirstName"].HeaderText = "Nombre(s)";
            dtvUsers.Columns["LastName"].Visible = true;
            dtvUsers.Columns["LastName"].HeaderText = "Apellido(s)";
            dtvUsers.Columns["Email"].Visible = true;
            dtvUsers.Columns["Role"].HeaderText = "Rol";
            dtvUsers.Columns["Role"].Visible = true;
            dtvUsers.Columns["Role"].DataPropertyName = "Name";
            dtvUsers.Columns["Status"].Visible = true;

            foreach (DataGridViewRow row in dtvUsers.Rows)
            {
                string RoleName = null;
                //RoleName = ((User)row.DataBoundItem).Role.Name;
                row.Cells["Role"].Value = RoleName;
            }

            dtvUsers.AutoResizeColumns();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            UserForm userForm = new UserForm();
            userForm.MdiParent = this.MdiParent;
            userForm.Show();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dtvUsers.CurrentRow != null)
            {
                RecordId = Convert.ToInt32(dtvUsers.Rows[dtvUsers.CurrentRow.Index].Cells["Id"].Value);
                UserForm userForm = new UserForm(RecordId);
                userForm.MdiParent = this.MdiParent;
                userForm.Show();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                NameValueCollection filtersValue = new NameValueCollection();
                filtersValue.Add("name", txtSearch.Text.Trim());
                filtersValue.Add("status", "-1");
                var results = _userService.FilterBy(filtersValue, null, null);
                dtvUsers.DataSource = results.Item1;
            }
            else
            {
                dtvUsers.DataSource = _userService.GetAll();
            }
        }

        private void CheckEnterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            btnSearch_Click(null, null);
        }
    }
}
