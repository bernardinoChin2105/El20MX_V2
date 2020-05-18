using MVC_Project.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;

namespace MVC_Project.API
{
    public class ActionFilterHelper: ActionFilterAttribute
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //UnitOfWork = actionContext.Request.GetDependencyScope().GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            //UnitOfWork.BeginTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            /*UnitOfWork = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            if(actionExecutedContext.Exception == null)
            { 
                UnitOfWork.Commit();
            } 
            else
            {
                UnitOfWork.Rollback();
            }*/
        }
    }
}