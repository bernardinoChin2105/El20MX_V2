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

        public CADController(AccountService accountService, IUserService userService)
        {
            _accountService = accountService;
            _userService = userService;
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
    }
}