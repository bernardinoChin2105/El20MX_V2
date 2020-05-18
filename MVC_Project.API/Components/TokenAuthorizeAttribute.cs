﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using MVC_Project.Utils.EnumsExtension;
using MVC_Project.API.App_Code;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Data.Repositories;
using MVC_Project.Data.Helpers;

namespace MVC_Project.API.Components
{
    public class TokenAuthorizeAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext context)
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                IRepository<User> repository = new Repository<User>(unitOfWork);
                IUserService userService = new UserService(repository);
                IAuthService authService = new AuthService(repository);

                bool skipAuthorization = context.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
              context.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
                if (skipAuthorization)
                    return;

                if (context.Request.Headers.Authorization == null)
                {
                    context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Authorization key required" });
                    return;
                }
                string apiKey = context.Request.Headers.Authorization.ToString();
                User user = userService.FindBy(x => x.ApiKey == apiKey).FirstOrDefault();
                DateTime now = DateUtil.GetDateTimeNow();
                if (user == null || (user.ExpiraApiKey.HasValue && user.ExpiraApiKey.Value < now))
                {
                    context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Invalid authorization token" });
                    return;
                }
                string actionName = context.ActionDescriptor.ActionName;
                string controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
                IEnumerable<Permission> permissions = authService.GetPermissions(user.Id,
                    actionName, controllerName,
                    Constants.PERMISSIONS_TYPE_ENUM.API.GetEnumValue<string>());
                if (!permissions.Any())
                {
                    context.Response = context.Request.CreateResponse(HttpStatusCode.Forbidden, new { message = "User has no permissions for this action" });
                    return;
                }
                LanguageMngr.SetLanguage(user.Language);
                UserApiAuthenticated.SetUserAuthenticated(context, user);
                //Validacion de tokens generados
                /*
                if (!tokenEngine.ValidateToken(apiKey, out clienteId))
                {
                    context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized, new { mensaje = "invalid authorization token" });
                    return;
                }
                context.Request.Properties.Add(new KeyValuePair<string, object>("clienteId", clienteId));*/
                //context.Request.Properties.Add(new KeyValuePair<string, object>("clientEmail", clientEmail));
            }
        }
    }
}