using MVC_Project.BackendWeb.Models;
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
        private IMembershipPermissionService _membershipPermissionService;

        public RegisterController(IUserService userService, IRoleService roleService, IProfileService profileService,
            IAccountService accountService, IMembershipService membershipService, ISocialNetworkLoginService socialNetworkLoginService,
            IMembershipPermissionService membershipPermissionService)
        {
            _userService = userService;
            _roleService = roleService;
            _profileService = profileService;
            _accountService = accountService;
            _membershipService = membershipService;
            _socialNetworkLoginService = socialNetworkLoginService;
            _membershipPermissionService = membershipPermissionService;
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
            //Falta Notificación para verificar usuario
            //redireccionamiento a la cuenta si es por red social
            if (model.RedSocial)
            {
                model.ConfirmPassword = model.SocialId.ToString();
                model.Password = model.SocialId.ToString();                 
            }

            //if (!String.IsNullOrWhiteSpace(model.ConfirmPassword)
            //    && !String.IsNullOrWhiteSpace(model.Password))
            //{
            //    if (!model.Password.Equals(model.ConfirmPassword))
            //    {
            //        ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
            //    }
            //}

            if (ModelState.IsValid)
            {
                var UserVal = _userService.FindBy(x => x.name == model.Email).FirstOrDefault();

                if (UserVal == null)
                {
                    DateTime todayDate = DateUtil.GetDateTimeNow();

                    string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                    DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                    
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

                    var user = new User
                    {
                        uuid = Guid.NewGuid(),
                        name = model.Email,
                        password = SecurityUtil.EncryptPassword(model.Password),
                        passwordExpiration = passwordExpiration, //validar cuando sea red social
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.UNCONFIRMED.ToString(),
                        profile = profile
                    };

                    _userService.CreateUser(user);

                    //var membership = new Membership
                    //{
                    //    user = user,
                    //    role = role,
                    //};
                    
                    //_membershipService.Create(membership);

                    //List<MembershipPermission> membershipPermissions = new List<MembershipPermission>();
                    //foreach (var permission in role.permissions)
                    //{
                    //    membershipPermissions.Add(new MembershipPermission
                    //    {
                    //        permission=permission,
                    //        membership=membership
                    //    });
                    //}
                    //_membershipPermissionService.Create(membershipPermissions);

                    if (model.RedSocial)
                    {
                        SocialNetworkLogin socialNW = new SocialNetworkLogin()
                        {
                            uuid = Guid.NewGuid(),
                            socialNetwork = model.TypeRedSocial,
                            token = model.SocialId,
                            user = user,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };

                        _socialNetworkLoginService.Create(socialNW);

                        user.status = SystemStatus.ACTIVE.ToString();
                        _userService.Update(user);

                        AuthViewModel LoginModel = new AuthViewModel()
                        {
                            Email = model.Email,
                            Password = model.SocialId.ToString(),
                            RedSocial = model.RedSocial,
                            TypeRedSocial = model.TypeRedSocial,
                            SocialId = model.SocialId
                        };


                        //Falta notificación para envio de corro cuando sea registro con redes sociales
                        //string token = (user.uuid + "@");
                        //token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                        //user.token = token;
                        Dictionary<string, string> customParams = new Dictionary<string, string>();
                        string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
                        string link = urlAccion + "";
                        customParams.Add("param1", user.profile.firstName);
                        customParams.Add("param2", link);
                        NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_WELCOME_NETWORK);

                        Session["modelNW"] = LoginModel;
                        return RedirectToAction("LoginAuth", "Auth");
                        //return RedirectToAction("Index", "Account", );
                    }
                    else
                    {
                        //Enviar notificación para activar el correo si no es por red social
                        
                        string token = (user.uuid + "@");
                        token = EncryptorText.DataEncrypt(token).Replace("/", "!!").Replace("+", "$");
                        //user.token = token;
                        Dictionary<string, string> customParams = new Dictionary<string, string>();
                        string urlAccion = (string)ConfigurationManager.AppSettings["_UrlServerAccess"];
                        string link = urlAccion + "Register/VerifyUser?token=" + token;
                        customParams.Add("param1", user.profile.firstName);
                        customParams.Add("param2", link);
                        NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_WELCOME);

                        ViewBag.Message = "Registro exitoso.";

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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult VerifyUser(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login");

                var desencriptaToken = EncryptorText.DataDecrypt(token.Replace("!!", "/").Replace("$", "+"));

                if (string.IsNullOrEmpty(desencriptaToken))
                    return RedirectToAction("Login");

                var elements = desencriptaToken.Split('@');
                Guid id = Guid.Parse(elements.First().ToString());
                var resultado = _userService.FindBy(e => e.uuid == id).First();
                
                if (resultado != null)
                {
                    resultado.status = SystemStatus.ACTIVE.ToString();
                    _userService.Update(resultado);

                    ViewBag.Message = "¡Tu cuenta ha sido activada!.";
                    ViewBag.Success = true;
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Token ha expirado";
                ViewBag.Success = false;
                return View("Login");
                //ErrorController.SaveLogError(this, listAction.Update, "AccedeToken", ex);
            }
            ViewBag.Message = "Error en el token";
            ViewBag.Success = false;
            return View("Login");
        }
    }
}