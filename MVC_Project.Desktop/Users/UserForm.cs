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
                txtUsername.Text = userBO.name;
                txtPassword.Text = userBO.password;
                txtEmail.Text = userBO.name;
                txtFirstname.Text = userBO.profile.firstName;
                txtLastname.Text = userBO.profile.lastName;
                chkStatus.Checked = userBO.status.Equals(SystemStatus.ACTIVE.ToString());
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            DateTime todayDate = DateUtil.GetDateTimeNow();
            DateTime passwordExpiration = todayDate.AddYears(5);
            Role role = _roleService.FindBy(x => x.code == Constants.ROLE_EMPLOYEE).FirstOrDefault();

            if (RecordId.HasValue && RecordId.Value > 0)
            {
                var userBO = _userService.GetById(RecordId.Value);
                userBO.name = txtFirstname.Text;
                //userBO.lastName = txtLastname.Text;
                userBO.modifiedAt = todayDate;
                userBO.status = chkStatus.Checked ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString();
                _userService.Update(userBO);
            }
            else
            {
                var userBO = new User
                {
                    uuid = Guid.NewGuid(),
                    name = txtFirstname.Text,
                    //lastName = txtLastname.Text,
                    //userName = txtUsername.Text,
                    password = SecurityUtil.EncryptPassword(txtPassword.Text),
                    //name = txtEmail.Text,
                    //Role = role,
                    //phoneNumber = "",
                    passwordExpiration = passwordExpiration,
                    //language = "ES",
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = chkStatus.Checked ? SystemStatus.ACTIVE.ToString() : SystemStatus.INACTIVE.ToString()
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
