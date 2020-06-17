using MVC_Project.BackendWeb.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
//using MVC_Project.FlashMessages;
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
        public ActionResult CreateUser(RegisterViewModel model)
        {
            try
            {
                if (model.RedSocial)
                {
                    model.ConfirmPassword = model.SocialId.ToString();
                    model.Password = model.SocialId.ToString();
                }

                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada no es válido");

                var user = _userService.FindBy(x => x.name == model.Email).FirstOrDefault();

                if (user != null)
                {
                    if(user.status == SystemStatus.UNCONFIRMED.ToString())
                        throw new Exception("El ususario tiene una invitación pendiente por aceptar");
                    else
                        throw new Exception("Ya existe un usuario con el email proporcionado");
                }

                DateTime todayDate = DateUtil.GetDateTimeNow();

                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));

                user = new User
                {
                    uuid = Guid.NewGuid(),
                    name = model.Email,
                    createdAt = todayDate,
                    modifiedAt = todayDate,
                    status = SystemStatus.UNCONFIRMED.ToString(),
                    profile = new Profile
                    {
                        uuid = Guid.NewGuid(),
                        firstName = model.FistName,
                        lastName = model.LastName,
                        email = model.Email,
                        phoneNumber = model.MobileNumber,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        status = SystemStatus.ACTIVE.ToString(),
                    }
                };

                if (!model.RedSocial)
                {
                    user.password = SecurityUtil.EncryptPassword(model.Password);
                    user.passwordExpiration = passwordExpiration;
                }

                _userService.CreateUser(user);

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

                    MensajeFlashHandler.RegistrarMensaje("Se ha enviado un email de confirmación", TiposMensaje.Success);

                    return RedirectToAction("Login", "Auth");
                }

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
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
                    throw new Exception("El token no es válido");

                var desencriptaToken = EncryptorText.DataDecrypt(token.Replace("!!", "/").Replace("$", "+"));

                if (string.IsNullOrEmpty(desencriptaToken))
                    throw new Exception("El token no es válido");

                var elements = desencriptaToken.Split('@');

                Guid id = new Guid();

                if (!Guid.TryParse(elements.First().ToString(), out id))
                    throw new Exception("El token no es válido");

                var user = _userService.FindBy(e => e.uuid == id).First();
                if (user == null)
                    throw new Exception("Usuario no encontrado en el sistema");
                
                user.status = SystemStatus.ACTIVE.ToString();
                _userService.Update(user);

                MensajeFlashHandler.RegistrarMensaje("¡Tu cuenta ha sido activada!", TiposMensaje.Success);
                return RedirectToAction("Login","Auth");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult NewUserVerify(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    throw new Exception("El token no es válido");

                var desencriptaToken = EncryptorText.DataDecrypt(token.Replace("!!", "/").Replace("$", "+"));

                if (string.IsNullOrEmpty(desencriptaToken))
                    throw new Exception("El token no es válido");

                var elements = desencriptaToken.Split('@');

                Guid id = new Guid();

                if (!Guid.TryParse(elements.First().ToString(), out id))
                    throw new Exception("El token no es válido");

                var user = _userService.FirstOrDefault(e => e.uuid == id);
                if (user == null)
                    throw new Exception("Usuario no encontrado en el sistema");

                if (user.status == SystemStatus.ACTIVE.ToString())
                    throw new Exception("El usuario se encuentra activo, restablezca su contraseña");

                var userViewModel = new ChangePasswordViewModel
                {
                    Uuid=user.uuid,
                    Name = user.name,
                };
                return View(userViewModel);
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return View("Login");
            }
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult NewUserVerify(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada es incorrecto");

                var user = _userService.FindBy(e => e.uuid == model.Uuid).First();

                if (user == null)
                    throw new Exception("El usuario no se encuentra registrado en el sistema");

                user.password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                user.passwordExpiration = passwordExpiration;
                user.status = SystemStatus.ACTIVE.ToString();
                _userService.Update(user);
                MensajeFlashHandler.RegistrarMensaje("Usuario activado", TiposMensaje.Success);
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message, TiposMensaje.Error);
                return View("ChangePassword", model);
            }
        }
    }
}