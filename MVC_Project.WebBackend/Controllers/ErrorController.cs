using System;
using System.Web.Mvc;
using LogHubSDK.Models;
using MVC_Project.Resources;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;

namespace MVC_Project.WebBackend.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult ShowError(string errorMessage)
        {
            var authUser = Authenticator.AuthenticatedUser;
            ViewBag.ErrorMessage = errorMessage;
            LogUtil.AddEntry(
               "Se encontro un error: " + errorMessage,
               ENivelLog.Error,
               authUser.Id,
               authUser.Email,
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
            );
            return View();
        }

        public ActionResult ShowErrorKey(string errorKey)
        {
            var authUser = Authenticator.AuthenticatedUser;
            string errorMessage = ViewLabels.ResourceManager.GetString(errorKey);
            ViewBag.ErrorMessage = errorMessage;
            LogUtil.AddEntry(
               "Se encontro un error: " + errorMessage,
               ENivelLog.Error,
               authUser.Id,
               authUser.Email,
               EOperacionLog.ACCESS,
               string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
               ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
               string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
            );
            return View("ShowError");
        }

        public ActionResult Reauth(string redirectUri)
        {
            var authUser = Authenticator.AuthenticatedUser;
            ViewBag.RedirectUri = redirectUri;
            LogUtil.AddEntry(
              "Se encontro un error: " + redirectUri,
              ENivelLog.Error,
              authUser.Id,
              authUser.Email,
              EOperacionLog.ACCESS,
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
              ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
           );
            return View();
        }

        public ViewResult Index()
        {
            var authUser = Authenticator.AuthenticatedUser;
            TempData["ErrorMessage"] = "No es posible realizar la acción solicitada.";
            LogUtil.AddEntry(
              "No es posible realizar la acción solicitada.",
              ENivelLog.Error,
              authUser.Id,
              authUser.Email,
              EOperacionLog.ACCESS,
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
              ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
           );
            return View("GenericError");
        }

        public ViewResult PageNotFound()
        {
            var authUser = Authenticator.AuthenticatedUser;
            string msg = Convert.ToString(Session["Global.ErrorMessage"]);
            TempData["ErrorMessage"] = String.Format("La página solicitada no existe: {0}", msg);
            LogUtil.AddEntry(
             TempData["ErrorMessage"].ToString(),
              ENivelLog.Error,
              authUser.Id,
              authUser.Email,
              EOperacionLog.ACCESS,
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
              ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
           );
            return View("GenericError");
        }

        public ViewResult InternalError()
        {
            var authUser = Authenticator.AuthenticatedUser;
            string msg = Convert.ToString(Session["Global.ErrorMessage"]);
            TempData["ErrorMessage"] = String.Format("Error de procesamiento interno en servidor: {0}", msg);
            LogUtil.AddEntry(
             TempData["ErrorMessage"].ToString(),
              ENivelLog.Error,
              authUser.Id,
              authUser.Email,
              EOperacionLog.ACCESS,
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow()),
              ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
              string.Format("Usuario {0} | Fecha {1}", authUser.Email, DateUtil.GetDateTimeNow())
           );
            return View("GenericError");
        }
    }
}