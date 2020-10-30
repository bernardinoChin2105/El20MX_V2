using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVC_Project.WebBackend.AuthManagement
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeUsersAttribute : AuthorizeAttribute
    {
        private void HandleUnauthorizedAjaxRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var response = httpContext.Response;
            var user = httpContext.User;
            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;
            if (authenticatedUser == null || user.Identity.IsAuthenticated == false)
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            response.SuppressFormsAuthenticationRedirect = true;
            //response.End();

        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                HandleUnauthorizedAjaxRequest(filterContext);
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }
            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;
            if (authenticatedUser != null && filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                DateTime todayDate = DateUtil.GetDateTimeNow();
                #region Se comenta para evitar validar la expiración del password
                //if (authenticatedUser.PasswordExpiration.HasValue && authenticatedUser.PasswordExpiration.Value.Date < todayDate.Date)
                //{
                //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Auth", action = "ChangePassword" }));
                //    return;
                //}
                //filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                #endregion
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            AuthUser authenticatedUser = Authenticator.AuthenticatedUser;

            if (authenticatedUser == null)
                return false;

            //if (authenticatedUser.Role.Code.Equals(ConfigurationManager.AppSettings.Get("AdminKey")))
            //    return true;

            DateTime todayDate = DateUtil.GetDateTimeNow();
            if (authenticatedUser.PasswordExpiration.HasValue && authenticatedUser.PasswordExpiration.Value.Date < todayDate.Date)
                return false;

            string controller = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string action = httpContext.Request.RequestContext.RouteData.Values["action"].ToString();

            var permision = authenticatedUser.Permissions.FirstOrDefault(x => x.Controller.Equals(controller) && !string.IsNullOrEmpty(x.Level));

            if (permision == null)
                return false;

            if (permision.Level == SystemLevelPermission.NO_ACCESS.ToString())
                return false;

            if (permision.Level == SystemLevelPermission.READ_ONLY.ToString() && action != SystemActions.INDEX.GetDisplayName())
                return false;

            return true;
        }
    }
}