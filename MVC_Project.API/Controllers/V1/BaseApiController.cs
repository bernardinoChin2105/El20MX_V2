using MVC_Project.API.Models;
using MVC_Project.API.Models.Api_Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace MVC_Project.API.Controllers
{
    public class BaseApiController : ApiController
    {
        public HttpResponseMessage CreateResponse<T>(T data) where T : class
        {
            var response = new ApiResponse<T>()
            {
                Result = "success",
                ResponseData = data,
                StatusCode = (int)HttpStatusCode.OK,
                Message = string.Empty
            };
            //LOG de respuesta si es necesario
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        public HttpResponseMessage CreateErrorResponse(Exception exception, IList<MessageResponse> messages)
        {
            var response = new ApiResponse<IList<MessageResponse>>();
            if (exception == null)
            {
                messages.Select(x => { x.Type = MessageType.error.ToString("G"); return x; }).ToList();
                response = new ApiResponse<IList<MessageResponse>>
                {
                    Result = "error",
                    ResponseData = messages,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = messages.First().Description
                };
            }
            else
            {
                Win32Exception win32Ex = exception as Win32Exception;
                string errorMsg = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
                int errorCode = win32Ex == null ? (int)HttpStatusCode.BadRequest : (int)HttpStatusCode.InternalServerError;
                messages = new List<MessageResponse>();
                messages.Add(new MessageResponse { Type = MessageType.error.ToString("G"), Description = errorMsg });
                response = new ApiResponse<IList<MessageResponse>>
                {
                    Result = "error",
                    ResponseData = messages,
                    StatusCode = errorCode,
                    Message = messages.First().Description
                };
            }
            //LOG de respuesta si es necesario
            return Request.CreateResponse((HttpStatusCode)response.StatusCode, response);
        }
    }
}
