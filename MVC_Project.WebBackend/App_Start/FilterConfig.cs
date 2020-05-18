using System.Web;
using MVC_Project.WebBackend.AuthManagement;
using System.Web.Mvc;

namespace MVC_Project.WebBackend
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeUsersAttribute());
        }
    }
}
