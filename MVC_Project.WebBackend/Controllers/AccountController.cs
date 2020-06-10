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
        private IMembershipService _membership;
        private IAccountService _accountService;
        public AccountController(IMembershipService accountUserService, IAccountService accountService)
        {
            _membership = accountUserService;
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
                    var mebershipPermissions = membership.mebershipPermissions.Select(x => x.permission);
                    var permissions = mebershipPermissions.Select(p => new Permission
                        {
                            Action = p.action,
                            Controller = p.controller,
                            Module = p.module
                        }).ToList();

                    authUser.Role = new Role { Code = membership.role.code, Name = membership.role.name };
                    authUser.Account = new Account { Id = account.id, Uuid = account.uuid, Name = account.name, RFC = account.rfc };
                    authUser.Permissions = permissions;

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