using MVC_Project.Desktop.Helpers;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MVC_Project.Desktop.Users
{
    public partial class UserForm : Form
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private Int32? RecordId;

        public UserForm() : this(null)
        {
        }
        public UserForm(Int32? _recordId) 
        {
            RecordId = _recordId;
            _userService = UnityHelper.Resolve<IUserService>();
            _roleService = UnityHelper.Resolve<IRoleService>();
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            if (RecordId.HasValue && RecordId.Value > 0)
            {
                var userBO = _userService.GetById(RecordId.Value);
                txtUsername.Text = userBO.Username;
                txtPassword.Text = userBO.Password;
                txtEmail.Text = userBO.Email;
                txtFirstname.Text = userBO.FirstName;
                txtLastname.Text = userBO.LastName;
                chkStatus.Checked = userBO.Status;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            DateTime todayDate = DateUtil.GetDateTimeNow();
            DateTime passwordExpiration = todayDate.AddYears(5);
            Role role = _roleService.FindBy(x => x.Code == Constants.ROLE_EMPLOYEE).FirstOrDefault();

            if (RecordId.HasValue && RecordId.Value > 0)
            {
                var userBO = _userService.GetById(RecordId.Value);
                userBO.FirstName = txtFirstname.Text;
                userBO.LastName = txtLastname.Text;
                userBO.UpdatedAt = todayDate;
                userBO.Status = chkStatus.Checked;
                _userService.Update(userBO);
            }
            else
            {
                var userBO = new User
                {
                    Uuid = Guid.NewGuid().ToString(),
                    FirstName = txtFirstname.Text,
                    LastName = txtLastname.Text,
                    Username = txtUsername.Text,
                    Password = SecurityUtil.EncryptPassword(txtPassword.Text),
                    Email = txtEmail.Text,
                    Role = role,
                    MobileNumber = "",
                    PasswordExpiration = passwordExpiration,
                    Language = "ES",
                    CreatedAt = todayDate,
                    UpdatedAt = todayDate,
                    Status = chkStatus.Checked
                };
                /*foreach (var permission in role.Permissions)
                {
                    userBO.Permissions.Add(permission);
                }*/
                _userService.Create(userBO);
            }
            this.Close();
        }
    }
}
