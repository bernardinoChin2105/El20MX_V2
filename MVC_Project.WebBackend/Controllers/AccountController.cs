using MVC_Project.Domain.Services;
using MVC_Project.Integrations.Paybook;
using MVC_Project.Integrations.SAT;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.WebBackend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class AccountController : Controller
    {
        private IMembershipService _membership;
        private IAccountService _accountService;
        private ICredentialService _credentialService;
        private IRoleService _roleService;
        private IUserService _userService;

        public AccountController(IMembershipService accountUserService, IAccountService accountService,
            ICredentialService credentialService, IRoleService roleService, IUserService userService)
        {
            _membership = accountUserService;
            _accountService = accountService;
            _credentialService = credentialService;
            _roleService = roleService;
            _userService = userService;
        }

        // GET: Account
        public ActionResult Index()
        {
            var authUser = Authenticator.AuthenticatedUser;
            if (authUser == null)
            {
                ViewBag.Error = "Sesion del usuario inválida";
                return RedirectToAction("Index", "Auth");
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult SelectAccount()
        {
            var authUser = Authenticator.AuthenticatedUser;
            var accountViewModel = new AccountSelectViewModel { accountListViewModels = new List<AccountListViewModel>() };
            var memberships = _membership.FindBy(x => x.user.id == authUser.Id && x.account != null);
            if (memberships.Any())
            {
                accountViewModel.accountListViewModels = memberships.Select(x => new AccountListViewModel
                {
                    uuid = x.account.uuid,
                    name = x.account.name,
                    rfc = x.account.rfc,
                    role = x.role.name,
                    accountId = x.account.id,
                    imagen = x.account.imagen
                }).ToList();

                #region Obtener información de la credencial para saber si esta ya activo
                foreach (var item in accountViewModel.accountListViewModels)
                {
                    var credential = _credentialService.FindBy(x => x.account.id == item.accountId).FirstOrDefault();
                    if (credential != null)
                    {
                        if (credential.statusProvider == "pending")
                        {
                            var responseSat = SATws.CallServiceSATws("credentials/" + credential.idCredentialProvider, null, "Get");
                            var model = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);
                            credential.statusProvider = model.status;
                            _credentialService.Update(credential);
                        }
                        item.statusValidate = credential.statusProvider;
                    }
                }
                #endregion
            }
            accountViewModel.count = accountViewModel.accountListViewModels.Count;
            return PartialView("_SelectAccountModal", accountViewModel);
        }

        [AllowAnonymous]
        public ActionResult SetAccount(Guid uuid)
        {
            var authUser = Authenticator.AuthenticatedUser;
            var account = _accountService.FindBy(x => x.uuid == uuid).FirstOrDefault();

            if (account != null)
            {
                var membership = _membership.FindBy(x => x.account.id == account.id && x.user.id == authUser.Id).FirstOrDefault();

                if (membership != null)
                {
                    var permissions = membership.role.rolePermissions.Where(x => x.permission.status == SystemStatus.ACTIVE.ToString()).Select(p => new Permission
                    {
                        //Action = p.permission.action,
                        Controller = p.permission.controller,
                        Module = p.permission.module,
                        Level = p.level
                    }).ToList();

                    authUser.Role = new Role { Code = membership.role.code, Name = membership.role.name };
                    authUser.Account = new Account { Id = account.id, Uuid = account.uuid, Name = account.name, RFC = account.rfc };
                    authUser.Permissions = permissions;

                    Authenticator.RefreshAuthenticatedUser(authUser);

                    return RedirectToAction("Index", "User");
                }
            }

            return RedirectToAction("Login", "Auth");
        }

        [AllowAnonymous]
        public ActionResult CreateAccountModal()
        {
            return PartialView("_CreateAccountModal");
        }

        [HttpPost]
        [AllowAnonymous]
        //[Authorize, HttpPost, ValidateAntiForgeryToken, ValidateInput(true)]
        public JsonResult CreateCredential(LogInSATModel dataSat)
        {
            bool result = false;
            string message = string.Empty;
            string typeNoti = string.Empty;

            try
            {
                //Realizar la captura de la información
                //Validar que no se repita el rfc
                var accountExist = _accountService.ValidateRFC(dataSat.rfc);

                if (accountExist == null)
                {
                    //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
                    var responseSat = SATws.CallServiceSATws("credentials", dataSat, "Post");

                    var model = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);

                    //Guardar la información si el llamado del servicio es exitoso
                    var authUser = Authenticator.AuthenticatedUser;
                    //var user = _accountService.FindBy(x => x.uuid == authUser.Uuid).FirstOrDefault();
                    DateTime todayDate = DateUtil.GetDateTimeNow();

                    //vamos a crear el account y memberships. Pendiente a que me confirme William los memberships
                    #region Cración de la cuenta (Account)

                    var roleD = _roleService.FindBy(x => x.code == SystemRoles.ACCOUNT_OWNER.ToString()).FirstOrDefault();
                    var userD = _userService.FindBy(x => x.uuid == authUser.Uuid).FirstOrDefault();

                    Domain.Entities.Account account = new Domain.Entities.Account()
                    {
                        uuid = Guid.NewGuid(),
                        name = authUser.FirstName + " " + authUser.LastName,
                        rfc = dataSat.rfc,
                        createdAt = todayDate,
                        modifiedAt = todayDate,
                        imagen = "/Images/p1.jpg",
                        status = SystemStatus.ACTIVE.ToString()
                    };

                    account.memberships.Add(new Domain.Entities.Membership
                    {
                        account = account,
                        user = userD,
                        role = roleD
                    });

                    _accountService.Create(account);
                    #endregion

                    Domain.Entities.Credential credential = new Domain.Entities.Credential();
                    if (account.id > 0)
                    {
                        credential = new Domain.Entities.Credential()
                        {
                            account = account,
                            provider = "SAT.ws",
                            idCredentialProvider = model.id,
                            statusProvider = model.status,
                            createdAt = todayDate,
                            modifiedAt = todayDate,
                            status = SystemStatus.ACTIVE.ToString()
                        };

                        _credentialService.Create(credential);
                    }

                    //Retornar la información
                    //string idCredential = credential.id.ToString();
                    if (credential.id > 0)
                    {
                        result = true;
                        message = "Cuenta registrada";
                        typeNoti = "success";
                    }
                    else
                    {
                        //borrar registros o conexiones en satws si no se creo la credencial o la cuenta
                    }
                }
                else
                {
                    message = "Existe una cuenta registrada con este RFC.";
                    typeNoti = "error";
                }

                return new JsonResult
                {
                    Data = new { Mensaje = message, Type = typeNoti, Success = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                return new JsonResult
                {
                    Data = new { Mensaje = ex.Message.ToString(), Type = "error", Success = false },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }

        }



        //Llamadas para paybook
        [HttpGet]
        [AllowAnonymous]
        public JsonResult getToken()
        {
            try
            {

                //Obtener usuario                 
                var responseUsers = Paybook.CallServicePaybook("users", null, "Get");

                var dataUser = new Object();
                //{
                //    id_external = "{{sync_user_id_external}}",
                //    name = "{{sync_user_name}}"
                //};

                //Crea un usuario
                var responseUserPost = Paybook.CallServicePaybook("users", dataUser, "Post");

                return new JsonResult
                {
                    //Data = new { Mensaje = message, Type = typeNoti, Success = result },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
                return new JsonResult
                {
                    Data = new { Mensaje = ex.Message.ToString(), Type = "error", Success = false },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }
    }
}