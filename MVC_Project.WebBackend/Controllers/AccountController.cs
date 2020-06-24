using MVC_Project.Domain.Services;
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

        public AccountController(IMembershipService accountUserService, IAccountService accountService, ICredentialService credentialService)
        {
            _membership = accountUserService;
            _accountService = accountService;
            _credentialService = credentialService;
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
                    role = x.role.name
                }).ToList();
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
            try
            {                
                //Realizar la captura de la información
                //Llamar al servicio para crear la credencial en el sat.ws y obtener respuesta                  
                var responseSat = SATws.CallServiceSATws("credentials", dataSat, "Post");

                var model = JsonConvert.DeserializeObject<SatAuthResponseModel>(responseSat);

                //Guardar la información si el llamado del servicio es exitoso
                var authUser = Authenticator.AuthenticatedUser;
                var account = _accountService.FindBy(x => x.uuid == authUser.Uuid).FirstOrDefault();
                DateTime todayDate = DateUtil.GetDateTimeNow();

                Domain.Entities.Credential credential = new Domain.Entities.Credential()
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
                //Retornar la información
                string idCredential = credential.id.ToString();

                return new JsonResult
                {
                    //data = { },
                    Data = new { Mensaje = new { title = "response", error = "" },  Success = true, Url = "" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
            catch (Exception ex)
            {                
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message.ToString() }, Success = false, Url = "" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }

        }
    }
}