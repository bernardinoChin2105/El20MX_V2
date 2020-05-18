using MVC_Project.Domain.Helpers;
using MVC_Project.WebBackend.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Unity.Attributes;

namespace MVC_Project.WebBackend.Controllers
{
    public class BaseController : Controller
    {
        //[Dependency]
        //public IUnitOfWork UnitOfWork { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (!filterContext.IsChildAction) UnitOfWork.BeginTransaction();
        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //if (!filterContext.IsChildAction) UnitOfWork.Commit();
        }
        
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string lang = null;
            HttpCookie langCookie = Request.Cookies["culture"];
            if (langCookie != null)
            {
                lang = langCookie.Value;
            }
            else
            {
                var userLanguage = Request.UserLanguages;
                var userLang = userLanguage != null ? userLanguage[0] : "";
                lang = string.IsNullOrEmpty(userLang) ? LanguageMngr.GetDefaultLanguage() : userLang;
            }
            LanguageMngr.SetLanguage(lang);
            return base.BeginExecuteCore(callback, state);
        }
        protected JsonResult JsonStatusGone(string message)
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.Gone;
            return Json(new
            {
                Message = message
            }, JsonRequestBehavior.AllowGet);
        }
    }
}