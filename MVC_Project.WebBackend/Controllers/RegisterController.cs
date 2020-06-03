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
using static MVC_Project.Utils.Constants;

namespace MVC_Project.WebBackend.Controllers
{
    public class RegisterController : BaseController
    {
        private IUserService _userService;
        private IRoleService _roleService;
        private IProfileService _profileService;
        private IAccountService _accountService;
        private IMembershipService _membershipService;
        private ISocialNetworkLoginService _socialNetworkLoginService;

        public RegisterController(IUserService userService, IRoleService roleService, IProfileService profileService,
            IAccountService accountService, IMembershipService membershipService, ISocialNetworkLoginService socialNetworkLoginService)
        {
            _userService = userService;
            _roleService = roleService;
            _profileService = profileService;
            _accountService = accountService;
            _membershipService = membershipService;
            _socialNetworkLoginService = socialNetworkLoginService;

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
            //Falta agregar el loghub
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
                    var role = availableRoles.Where(x => x.code == SystemRoles.ACCOUNT_OWNER.ToString()).FirstOrDefault();

                    var profile = new Profile
                    {
                        uuid = Guid.NewGuid(),
                        firstName = model.FistName,
                        lastName = model.LastName,
                        email = model.Email,
                        phoneNumber = model.MobileNumber,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString(),
                    };

                    _profileService.Create(profile);

                    var user = new User
                    {
                        uuid = Guid.NewGuid(),
                        name = model.Email,
                        password = SecurityUtil.EncryptPassword(model.Password),
                        passwordExpiration = passwordExpiration, //validar cuando sea red social
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString(),
                        //role = role,
                        profile = profile
                    };

                    //foreach (var permission in role.permissions)
                    //{
                    //    user.permissions.Add(permission);
                    //}

                    _userService.Create(user);

                    //StringBuilder builder = new StringBuilder();
                    //builder.Append(model.FistName);
                    //builder.Append(" ");
                    //builder.Append(model.LastName);

                    //var account = new Account
                    //{
                    //    uuid = Guid.NewGuid(),
                    //    name = builder.ToString(),
                    //    createdAt = todayDate,
                    //    modifiedAt = todayDate,
                    //    status = Status.ACTIVE.ToString(),
                    //    //rfc = "CAYW880502FK4"
                    //};

                    ////account.users.Add(user);
                    //_accountService.Create(account);

                    var membership = new Membership
                    {
                        //account = account,
                        user = user,
                        role = role,
                        //mebershipPermissions = role.permissions
                    };

                    foreach(var permission in role.permissions)
                    {
                        membership.mebershipPermissions.Add(new MembershipPermission
                        {
                            permission = permission
                        });
                    }


                    _membershipService.Create(membership);
                    //account.users.Add(user);
                    //_accountService.Create(account);

                    if (model.RedSocial)
                    {
                        SocialNetworkLogin socialNW = new SocialNetworkLogin()
                        {
                            uuid = Guid.NewGuid(),
                            socialNetwork = model.TypeRedSocial,
                            token = model.Password,
                            user = user,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };

                        _socialNetworkLoginService.Create(socialNW);
                        return RedirectToAction("Login", "Auth");                        
                    }
                    else
                    {
                        //Enviar notificación para activar el correo si no es por red social
                        return RedirectToAction("Login", "Auth");
                    }

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