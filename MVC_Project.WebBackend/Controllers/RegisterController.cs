using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class RegisterController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IProfileService _profileService;
        private IAccountService _accountService;

        public RegisterController(IUserService userService, IRoleService roleService, IProfileService profileService,
            IAccountService accountService)
        {
            _userService = userService;
            _roleService = roleService;
            _profileService = profileService;
            _accountService = accountService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAccount(RegisterViewModel model)
        {
            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword)
                && !String.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
                }
            }

            if (ModelState.IsValid)
            {
                var UserVal = _userService.FindBy(x => x.name == model.Email).FirstOrDefault();

                if (UserVal == null)
                {
                    DateTime todayDate = DateUtil.GetDateTimeNow();

                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));

                    var availableRoles = _roleService.GetAll();
                    var role = availableRoles.Where(x => x.code == "EMPLOYEE").FirstOrDefault();
                    
                    

                   
                    var profile = new Profile
                    {
                        uuid = Guid.NewGuid(),
                        firstName = model.FistName,
                        lastName = model.LastName,
                        email = model.Email,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = Status.ACTIVE.ToString(),
                    };

                    _profileService.Create(profile);

                    var user = new User
                    {
                        uuid = Guid.NewGuid(),
                        name = model.Email,
                        password = SecurityUtil.EncryptPassword(model.Password),
                        passwordExpiration = passwordExpiration,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = Status.ACTIVE.ToString(),
                        role = role,
                        profile = profile
                    };

                    
                    
                    foreach (var permission in role.permissions)
                    {
                        user.permissions.Add(permission);
                    }
                    _userService.Create(user);

                    StringBuilder builder = new StringBuilder();
                    builder.Append(model.FistName);
                    builder.Append(" ");
                    builder.Append(model.LastName);

                    var account = new Account
                    {
                        uuid = Guid.NewGuid(),
                        name = builder.ToString(),
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = Status.ACTIVE.ToString(),
                        //rfc = "CAYW880502FK4"
                    };

                    account.users.Add(user);
                    _accountService.Create(account);


                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.Error = "Ya existe el usuario con el Email registrado.";
                    return View("Index", model);
                }                
            }
            else
            {                
                return View("Index", model);
            }           
        }
                
    }
}