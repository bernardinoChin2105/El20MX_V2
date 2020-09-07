using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVC_Project.WebBackend
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            };
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                Response.Headers["X-FRAME-OPTIONS"] = "DENY";
                Response.Headers["Cache-Control"] = "no-cache";
                var browserInformation = Request.Browser;
                if ((Response.ContentType == "pdf/application" || Response.ContentType == "text/csv") && browserInformation.Browser == "IE")
                {
                    Response.Headers["Cache-Control"] = "private";
                    Response.CacheControl = "private";
                }
                //Trace.TraceInformation("Application_EndRequest = Headers[Cache-Control] = " + Response.Headers["Cache-Control"]);
            }
            catch (Exception ex) { }
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            try
            {
                Response.Headers["X-FRAME-OPTIONS"] = "DENY";
                Response.Headers["Cache-Control"] = "no-cache";
                var browserInformation = Request.Browser;
                if ((Response.ContentType == "pdf/application" || Response.ContentType == "text/csv"
                        || Response.ContentType == "text/excel") && browserInformation.Browser == "IE")
                {
                    Response.Headers["Cache-Control"] = "private";
                    Response.CacheControl = "private";
                }
                //Trace.TraceInformation("Application_EndRequest = Headers[Cache-Control] = " + Response.Headers["Cache-Control"]);
            }
            catch (Exception ex) { }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            //Response.Clear();
            string action = "Index";

            HttpException httpException = exception as HttpException;

            if (httpException != null)
            {
                Session["Global.ErrorMessage"] = httpException.Message;
                if (exception.Message.Contains("NoCatch") || exception.Message.Contains("maxUrlLength"))
                    return;

                switch (httpException.GetHttpCode())
                {
                    case 404:
                        action = "PageNotFound";
                        break;

                    case 500:
                        action = "InternalError";
                        break;

                    default:
                        action = "Index";
                        break;
                }
                Server.ClearError();
                Response.Redirect(string.Format("~/Error/{0}", action));
            }


            if (exception != null)
            {
                Response.Write("<h2>Error Global en la Aplicación</h2>\n");

                if (exception is System.Net.WebException)
                {
                    System.Net.WebException webException = exception as System.Net.WebException;
                    Response.Write("<p><b>Existe un error al acceder a los servicios externos<br/><br/>Detalle t&eacute;cnico:</b></p>\n");
                    if (webException.Response != null)
                    {
                        Response.Write("<p><b>Ruta: </b>" + webException.Response.ResponseUri.AbsoluteUri + "</p>\n");
                        //Response.Write("<p><b>Ruta: </b>" + webException.Response.ResponseUri.PathAndQuery + "</p>\n");
                        Response.Write("<p><b>Status: </b>" + webException.Status.ToString() + "</p>\n");
                        Response.Write("<p><b>Fuente: </b>" + webException.Source + "</p>\n");
                    }
                }
                if (exception.Source != null)
                {
                    Response.Write("<p><b>Fuente: </b>" + HttpUtility.HtmlEncode(exception.Source) + "</p>\n");
                }
                if (exception.StackTrace != null)
                {
                    Response.Write(HttpUtility.HtmlEncode("<p><b>Stack: </b>" + HttpUtility.HtmlEncode(exception.StackTrace).Replace("Ensitech", "User1") + "</p>\n"));
                }

                try
                {
                    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(exception, true);
                    Response.Write(
                        String.Format("<p>Error Detail Message :{0}  => Error In :{1}  => Line Number :{2} => Error Method:{3}</p>",
                              HttpUtility.HtmlEncode(exception.Message),
                              trace.GetFrame(0).GetFileName().Replace("Ensitech", "User1"),
                              trace.GetFrame(0).GetFileLineNumber(),
                              trace.GetFrame(0).GetMethod().Name));
                }
                catch (Exception ex) { }

                Response.Write(HttpUtility.HtmlEncode("<p><b>Mensaje de error: </b>" + exception.Message + "</p>\n"));
                if (exception.InnerException != null && !string.IsNullOrEmpty(exception.InnerException.Message))
                {
                    Response.Write("<p>" + HttpUtility.HtmlEncode(exception.InnerException.Message) + "</p>\n");
                }
                Response.Write("<br/>Comuníquese con el administrador para reportar este error\n");
            }
            // Clear the error from the server
            Server.ClearError();
        }
    }
}
