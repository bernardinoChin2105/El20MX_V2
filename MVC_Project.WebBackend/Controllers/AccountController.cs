using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class AccountController : Controller
    {
        private IAccountUserService _accountUserService;
        private IAccountService _accountService;
        public AccountController(IAccountUserService accountUserService, IAccountService accountService)
        {
            _accountUserService = accountUserService;
            _accountService = accountService;
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
            
            accountViewModel.accountListViewModels = _accountUserService.FindBy(x => x.user.id == authUser.Id).Select(x => new AccountListViewModel
            {
                uuid = x.account.uuid,
                name = x.account.name,
                rfc = x.account.rfc,
                role = x.role.name
            }).ToList();
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
                var accountUser = _accountUserService.FindBy(x => x.account.id == account.id && x.user.id == authUser.Id).FirstOrDefault();

                if (accountUser != null)
                {
                    List<Permission> permissionsUser = accountUser.role.permissions.Select(p => new Permission
                    {
                        Action = p.action,
                        Controller = p.controller,
                        Module = p.module
                    }).ToList();

                    authUser.Role = new Role { Code = accountUser.role.code, Name = accountUser.role.name };
                    authUser.Account = new Account { Uuid = account.uuid, Name = account.name, RFC = account.rfc };
                    authUser.Permissions = permissionsUser;

                    Authenticator.RefreshAuthenticatedUser(authUser);

                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Login", "Auth");
        }

        [AllowAnonymous]
        public ActionResult CreateAccountModal()
        {
            return PartialView("_CreateAccountModal");
        }
    }
}