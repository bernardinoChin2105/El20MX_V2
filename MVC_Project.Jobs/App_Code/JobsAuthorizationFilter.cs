using Hangfire.Dashboard;
using Microsoft.Owin;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace MVC_Project.Jobs.App_Code
{
    public class JobsAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext dcontext)
        {
            string dashUsername = System.Configuration.ConfigurationManager.AppSettings["Jobs.Dashboard.Username"];
            string dashPassword = System.Configuration.ConfigurationManager.AppSettings["Jobs.Dashboard.Password"];

            OwinContext context = new OwinContext(dcontext.GetOwinEnvironment());
          
            string header = context.Request.Headers["Authorization"];
            if (String.IsNullOrWhiteSpace(header) == false)
            {
                AuthenticationHeaderValue authValues = AuthenticationHeaderValue.Parse(header);
                if ("Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
                    var parts = parameter.Split(':');
                    if (parts.Length > 1)
                    {
                        string login = parts[0];
                        string password = parts[1];

                        if ((String.IsNullOrWhiteSpace(login) == false) && (String.IsNullOrWhiteSpace(password) == false))
                        {
                            if (login== dashUsername && password == dashPassword)
                            {
                                return true;
                            }
                            else 
                            {
                                Challenge(context);
                            }
                        }
                    }
                }
            }

            //return true;// owinContext.Authentication.User.Identity.IsAuthenticated;
            return Challenge(context);
        }

        private bool Challenge(OwinContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");

            context.Response.Write("Authentication is required.");

            return false;
        }
    }
}