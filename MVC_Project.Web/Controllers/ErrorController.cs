using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_Project.Resources;

namespace MVC_Project.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult ShowError(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        public ActionResult ShowErrorKey(string errorKey)
        {
            string errorMessage = ViewLabels.ResourceManager.GetString(errorKey);
            ViewBag.ErrorMessage = errorMessage;
            return View("ShowError");
        }
        
        public ActionResult Reauth(string redirectUri)
        {
            ViewBag.RedirectUri = redirectUri;
            return View();
        }

        public ViewResult Index()
        {
            TempData["ErrorMessage"] = "No es posible realizar la acción solicitada.";
            return View("GenericError");
        }

        public ViewResult PageNotFound()
        {
            string msg = Convert.ToString( Session["Global.ErrorMessage"] );
            TempData["ErrorMessage"] = String.Format("La página solicitada no existe: {0}", msg);
            return View("GenericError");
        }

        public ViewResult InternalError()
        {
            string msg = Convert.ToString(Session["Global.ErrorMessage"]);
            TempData["ErrorMessage"] = String.Format("Error de procesamiento interno en servidor: {0}", msg);
            return View("GenericError");
        }
    }
}