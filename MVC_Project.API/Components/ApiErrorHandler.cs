using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace MVC_Project.API.Components
{
    public class ApiErrorHandler : ExceptionFilterAttribute
    {
        private IApiLogService apiLogService;

        public ApiErrorHandler()
        {
            
        }
        //<summary>Se ejecuta cuando se  produce una excepcion, esta lee los datos de la excepcion y genera un log en la base de datos.</summary>
        /// <param name="actionExecutedContext">Contexto de ejecucion.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                IRepository<ApiLog> repository = new Repository<ApiLog>(unitOfWork);
                apiLogService = new ApiLogService(repository);

                string requestUuid = actionExecutedContext.Request.GetCorrelationId().ToString();
                //unitOfWork.BeginTransaction();
                try
                {
                    ApiLog requestLog = apiLogService.FindBy(x => x.Uuid == requestUuid).FirstOrDefault();
                    if (requestLog != null)
                    {
                        requestLog.Exception = actionExecutedContext.Exception.GetExceptionDetails();
                        apiLogService.Update(requestLog);
                    }
                    //unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    //unitOfWork.Rollback();
                }
                base.OnException(actionExecutedContext);
            }
        }

    }
}