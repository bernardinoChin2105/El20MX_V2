using LogHubSDK.Models;
using MVC_Project.BackendWeb.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.FlashMessages;
//using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
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
                    if (user.status == SystemStatus.UNCONFIRMED.ToString())
                        throw new Exception("El usuario tiene una invitación pendiente por aceptar");
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
                    isBackOffice = false,
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
                        avatar = ConfigurationManager.AppSettings["Avatar.User"]
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
                    string link = urlAccion + "Auth/Login";
                    customParams.Add("param1", user.profile.firstName);
                    customParams.Add("param2", link);
                    NotificationUtil.SendNotification(user.name, customParams, Constants.NOT_TEMPLATE_WELCOME_NETWORK);

                    Session["modelNW"] = LoginModel;

                    LogUtil.AddEntry(
                       "Nuevo usuario registrado por red social: " + JsonConvert.SerializeObject(model),
                       ENivelLog.Info,
                       user.id,
                       user.name,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow())
                    );

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

                    LogUtil.AddEntry(
                       "Nuevo usuario registrado: " + JsonConvert.SerializeObject(model),
                       ENivelLog.Info,
                       user.id,
                       user.name,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow())
                    );
                    return RedirectToAction("Login", "Auth");
                }

            }
            catch (Exception ex)
            {
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
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

                if (user.status == SystemStatus.ACTIVE.ToString())
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    user.status = SystemStatus.ACTIVE.ToString();
                    _userService.Update(user);

                    LogUtil.AddEntry(
                       "Verificar Usuario con el token " + token,
                       ENivelLog.Info,
                       user.id,
                       user.name,
                       EOperacionLog.ACCESS,
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                       ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                       string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow())
                    );

                    //MensajeFlashHandler.RegistrarMensaje("¡Tu cuenta ha sido activada!", TiposMensaje.Success);
                    return View();
                }
   
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
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
                if (elements.Count() != 4)
                    throw new Exception("El token no es válido");

                Guid id = new Guid();

                if (!Guid.TryParse(elements[0], out id))
                    throw new Exception("El token no es válido");

                var user = _userService.FirstOrDefault(e => e.uuid == id);
                if (user == null)
                    throw new Exception("El usuario no se encontró en el sistema");

                if (!user.name.Equals(elements[2] + "@" + elements[3]))
                    throw new Exception("El correo de origen no corresponde al correo registrado en el sistema");


                var userViewModel = new ChangePasswordViewModel
                {
                    Uuid = user.uuid,
                    Name = user.name,
                    isBackOffice = user.isBackOffice
                };

                if (!user.isBackOffice)
                {
                    if (!Guid.TryParse(elements[1], out id))
                        throw new Exception("El token no es válido");

                    var account = _accountService.FirstOrDefault(e => e.uuid == id);
                    if (account == null)
                        throw new Exception("La cuenta no se encontró en el sistema");

                    var membership = _membershipService.FirstOrDefault(x => x.user.id == user.id && x.account.id == account.id);

                    if (membership == null)
                        throw new Exception("La suscripcion no se encontró en el sistema");

                    if (user.status == SystemStatus.ACTIVE.ToString())
                    {
                        membership.status = SystemStatus.ACTIVE.ToString();
                        _membershipService.Update(membership);
                        MensajeFlashHandler.RegistrarMensaje("El usuario a sido agregado a la cuenta", TiposMensaje.Success);
                        return RedirectToAction("Login", "Auth");
                    }

                    userViewModel.AcccountUuid = account.uuid;
                }

                LogUtil.AddEntry(
                   "Verificar nuevo usuario " + token,
                   ENivelLog.Info,
                   user.id,
                   user.name,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow())
                );

                return View(userViewModel);
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return RedirectToAction("Login", "Auth");
            }
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult NewUserVerify(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("El modelo de entrada es incorrecto");

                var user = _userService.FirstOrDefault(e => e.uuid == model.Uuid);
                if (user == null)
                    throw new Exception("El usuario no se encuentra registrado en el sistema");

                if (!user.isBackOffice)
                {
                    var account = _accountService.FirstOrDefault(e => e.uuid == model.AcccountUuid);
                    if (account == null)
                        throw new Exception("La cuenta no se encuentra registrada en el sistema");

                    var membership = _membershipService.FirstOrDefault(x => x.user.id == user.id && x.account.id == account.id);

                    if (membership == null)
                        throw new Exception("La suscripcion no se encontró en el sistema");

                    membership.status = SystemStatus.ACTIVE.ToString();
                    _membershipService.Update(membership);
                }
                else
                {
                    var membership = _membershipService.FirstOrDefault(x => x.user.id == user.id);

                    if (membership == null)
                        throw new Exception("La suscripcion no se encontró en el sistema");

                    membership.status = SystemStatus.ACTIVE.ToString();
                    _membershipService.Update(membership);
                }

                user.password = SecurityUtil.EncryptPassword(model.Password);
                DateTime todayDate = DateUtil.GetDateTimeNow();
                string daysToExpirateDate = ConfigurationManager.AppSettings["DaysToExpirateDate"];
                DateTime passwordExpiration = todayDate.AddDays(Int32.Parse(daysToExpirateDate));
                user.passwordExpiration = passwordExpiration;
                user.status = SystemStatus.ACTIVE.ToString();
                user.agreeTerms = model.AgreeTerms;
                _userService.Update(user);

                LogUtil.AddEntry(
                   "Verificar nuevo usuario " + JsonConvert.SerializeObject(model),
                   ENivelLog.Info,
                   user.id,
                   user.name,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", user.name, DateUtil.GetDateTimeNow())
                );

                MensajeFlashHandler.RegistrarMensaje("Usuario activado", TiposMensaje.Success);
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                LogUtil.AddEntry(
                   "Se encontro un error: " + ex.Message.ToString(),
                   ENivelLog.Error,
                   0,
                   "",
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "", DateUtil.GetDateTimeNow())
                );
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
                return View("NewUserVerify", model);
            }
        }
    }
}