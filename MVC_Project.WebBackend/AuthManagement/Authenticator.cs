using MVC_Project.WebBackend.AuthManagement.Models;
using System.Web;
using System.Web.Security;

namespace MVC_Project.WebBackend.AuthManagement
{
    public class Authenticator
    {
        public static AuthUser AuthenticatedUser
        {
            get
            {
                var authUser = System.Web.HttpContext.Current.Session["ST_AUTH_USER"];
                return authUser != null ? (AuthUser)authUser : null;
            }
        }
        public static void StoreAuthenticatedUser(AuthUser authUser)
        {
            HttpContext.Current.Session.Add("ST_AUTH_USER", authUser);
            FormsAuthentication.SetAuthCookie(authUser.Email, true);
        }

        public static void RefreshAuthenticatedUser(AuthUser authUser)
        {
            HttpContext.Current.Session.Remove("ST_AUTH_USER");
            HttpContext.Current.Session.Add("ST_AUTH_USER", authUser);
        }

        public static void RemoveAuthenticatedUser()
        {            
            HttpContext.Current.Session.Remove("ST_AUTH_USER");            
            FormsAuthentication.SignOut();            
        }
    }
}