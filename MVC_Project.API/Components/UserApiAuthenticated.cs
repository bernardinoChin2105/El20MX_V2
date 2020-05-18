using MVC_Project.Domain.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;

namespace MVC_Project.API.Components
{
    public class UserApiAuthenticated
    {

        public static void SetUserAuthenticated(HttpActionContext context, User user)
        {
            /*JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string userSerialized = JsonConvert.SerializeObject(user, jsonSerializerSettings);*/
            context.RequestContext.Principal = new GenericPrincipal(new GenericIdentity(user.Id.ToString()), null);
        }

        public static User GetUserAuthenticated(HttpRequestContext context)
        {
            string data = context.Principal.Identity.Name;
            if(data == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<User>(data);
            
        }

        public static int? GetUserAuthenticatedId(HttpRequestContext context)
        {
            string data = context.Principal.Identity.Name;
            if (data == null)
            {
                return null;
            }
            int userId = 0;
            if (Int32.TryParse(data, out userId))
            {
                return userId;
            }
            else
            {
                return null;
            }

        }
    }
}