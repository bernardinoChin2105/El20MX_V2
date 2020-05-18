using MVC_Project.Web.AuthManagement;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web
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
