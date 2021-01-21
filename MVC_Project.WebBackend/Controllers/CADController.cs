using MVC_Project.Domain.Services;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class CADController : Controller
    {
        private IAccountService _accountService;
        private IUserService _userService;
        private ICADAccountService _cadAccountService;

        public CADController(AccountService accountService, IUserService userService, ICADAccountService cadAccountService)
        {
            _accountService = accountService;
            _userService = userService;
            _cadAccountService = cadAccountService;
        }
        
        public ActionResult AssignAccount()
        {
            var model = new CADAccountsViewModel();
            var cads = _userService.FindBy(x => x.isBackOffice);
            var accounts = _accountService.GetAll();
            model.cads = cads.Select(x => new SelectListItem
            {
                Text = x.name,
                Value = x.id.ToString()
            }).ToList();

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public ActionResult GetAccounts(Int64 id)
        {
            var assigneds = _cadAccountService.GetAll().Select(x => x.account.id);
            var availables = _accountService.FindBy(x => !assigneds.Contains(x.id)).Select(x => new { id = x.id, name = x.name + "( " + x.rfc + " )" });

            return new JsonResult
            {
                Data = new { success = true, accounts = availables },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}