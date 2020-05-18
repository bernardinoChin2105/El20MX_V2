using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Domain.Services;
using Newtonsoft.Json;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace MVC_Project.API.Components
{
    public class RequestLogHandler : DelegatingHandler
    {
        private IApiLogService apiLogService;
        private IUserService userService;
        
        public RequestLogHandler()
        {
            
        }
        //<summary>Recibe la peticion del servidor antes de que llegue al controlador para realzar el log de la peticion.</summary>
        /// <param name="cancellationToken"></param>
        /// <param name="request">Objeto que contiene informacion de las peticiones.</param>
        /// <returns>Respuesta del servidor</returns>
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                IRepository<ApiLog> repository = new Repository<ApiLog>(unitOfWork);
                IRepository<User> repositoryUser = new Repository<User>(unitOfWork);
                apiLogService = new ApiLogService(repository);
                userService = new UserService(repositoryUser);

                StringBuilder content = new StringBuilder();
                string requestUuid = request.GetCorrelationId().ToString();
                HttpConfiguration config = GlobalConfiguration.Configuration;
                IHttpRouteData routeData = config.Routes.GetRouteData(request);
                HttpControllerContext controllerContext = new HttpControllerContext(config, routeData, request);
                DefaultHttpControllerSelector controllerSelector = new DefaultHttpControllerSelector(config);
                HttpControllerDescriptor controllerDescriptor = controllerSelector.SelectController(request);
                ApiControllerActionSelector apiControllerSelection = new ApiControllerActionSelector();
                controllerContext.ControllerDescriptor = controllerDescriptor;
                HttpActionDescriptor actionDescriptor = apiControllerSelection.SelectAction(controllerContext);
                ApiLog requestLog = new ApiLog();
                requestLog.Uuid = requestUuid;
                requestLog.Controller = controllerDescriptor.ControllerName;
                requestLog.Action = actionDescriptor.ActionName;
                //Obtiene y decodifica el post para que se muestre la informacion de la manera adecuada
                string data = HttpUtility.UrlDecode(request.Content.ReadAsStringAsync().Result);
                requestLog.Data = data;
                requestLog.CreatedAt = DateTime.Now;
                requestLog.Url = request.RequestUri.AbsoluteUri;//request.RequestUri.Host + HttpUtility.UrlDecode(request.RequestUri.PathAndQuery);
                requestLog.Method = request.Method.Method;
                requestLog.Status = "Started";
                object headers = request.Headers.ToDictionary(x => x.Key, y => y.Value);
                requestLog.Headers = JsonConvert.SerializeObject(headers);
                //unitOfWork.BeginTransaction();
                try
                {
                    apiLogService.Create(requestLog);
                    //unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    //unitOfWork.Rollback();
                }
                //Se espera la respuesta del controlador
                var response = await base.SendAsync(request, cancellationToken);

                requestUuid = request.GetCorrelationId().ToString();
                //unitOfWork.BeginTransaction();
                try
                {
                    requestLog = apiLogService.FindBy(x => x.Uuid == requestUuid).FirstOrDefault();
                    User user = null;
                    int? userId = UserApiAuthenticated.GetUserAuthenticatedId(request.GetRequestContext());
                    if (userId.HasValue)
                    {
                        user = userService.GetById(userId.Value);
                    }
                    if (user != null)
                    {
                        requestLog.CreatedBy = user;
                    }
                    if (requestLog != null)
                    {

                        if (response.RequestMessage != null && response.Content != null)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            if (requestLog.Exception != null)
                            {
                                requestLog.Status = "HasException";
                            }
                            else
                            {
                                requestLog.Status = "Completed";
                            }
                            requestLog.Result = responseContent.ToString();
                            apiLogService.Update(requestLog);
                        }
                    }
                    //unitOfWork.Commit();
                }catch(Exception e)
                {
                    //unitOfWork.Rollback();
                }
                return response;
            }
        }
    }
}