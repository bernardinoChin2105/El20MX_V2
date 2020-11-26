using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class MyAccountController : Controller
    {
        // GET: myAccount
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult AcountInvoicing()
        //{
        //    return View();
        //}
    }
}