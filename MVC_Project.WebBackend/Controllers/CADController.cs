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
            try
            {
                var assigneds = _cadAccountService.GetAll().Select(x => x.account.id);
                var availables = _accountService.FindBy(x => !assigneds.Contains(x.id)).
                    Select(x => new { id = x.id, name = x.name + " ( " + x.rfc + " )" });
                var assignedToCAD = _cadAccountService.FindBy(x => x.cad.id == id).
                    Select(x => new { id = x.account.id, name = x.account.name + " ( " + x.account.rfc + " )" });

                return new JsonResult
                {
                    Data = new { success = true, availables = availables, assigneds = assignedToCAD },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch(Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, message = ex.Message },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        [HttpPost, AllowAnonymous]
        public ActionResult SetAccounts(Int64 cadId, List<Int64> accountIds)
        {
            try
            {
                if (accountIds == null)
                    accountIds = new List<Int64>();

                var assignedToCAD = _cadAccountService.FindBy(x => x.cad.id == cadId).
                    Select(x => x);

                var inserts = accountIds.Where(x => !assignedToCAD.Select(y => y.account.id).Contains(x));
                var deletes = assignedToCAD.Where(x => !accountIds.Contains(x.account.id));
                _cadAccountService.AssignCustomers(cadId, inserts, deletes);

                return new JsonResult
                {
                    Data = new { success = true },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch(Exception ex)
            {
                return new JsonResult
                {
                    Data = new { success = false, message = ex.Message },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }
    }
}