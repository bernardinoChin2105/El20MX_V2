using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
{
    [Authorize]
    public class UserServicesController : Controller
    {
        // GET: UserServices
        public ActionResult Index()
        {
            return View();
        }
    }
}