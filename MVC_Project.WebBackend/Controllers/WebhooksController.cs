using LogHubSDK.Models;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class WebhooksController : Controller
    {
        // GET: Webhooks

        private IWebhookService _webhookService;

        public WebhooksController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost]
        //[Route("WebhookPaybook")]
        public JObject WebhookPaybook(Object response)
        {
            DateTime today = DateUtil.GetDateTimeNow();
            Webhook model = new Webhook()
            {
                uuid = Guid.NewGuid(),
                createdAt = today,
                provider = SystemProviders.SYNCFY.GetDisplayName(),
                response = JsonConvert.SerializeObject(response.ToString()),
            };

            try
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                var eventWh = responseData.First(x => x.Key == "event").Value;
                model.eventWebhook = eventWh.ToString();

                var endpointsWh = responseData.First(x => x.Key == "endpoints").Value;
                var endpoints = JsonConvert.DeserializeObject<Dictionary<string, object>>(endpointsWh.ToString());

                if (eventWh.ToString() == "credential_create" || eventWh.ToString() == "credential_update")
                {
                    var credentials = endpoints.FirstOrDefault(x => x.Key == "credential");
                    if (credentials.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(credentials.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }
                }
                else if (eventWh.ToString() == "refresh")
                {
                    var accounts = endpoints.FirstOrDefault(x => x.Key == "accounts");
                    if (accounts.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(accounts.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var attachments = endpoints.FirstOrDefault(x => x.Key == "attachments");
                    if (attachments.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(attachments.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var credentials = endpoints.FirstOrDefault(x => x.Key == "credential");
                    if (credentials.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(credentials.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }

                    var transactions = endpoints.FirstOrDefault(x => x.Key == "transactions");
                    if (transactions.Value != null)
                    {
                        var items = JsonConvert.DeserializeObject<List<string>>(transactions.Value.ToString());
                        foreach (var item in items)
                        {
                            model.endpoint = item;
                            _webhookService.Create(model);
                        }
                    }
                }
                else
                {
                    //result = "Event unidentified";
                    _webhookService.Create(model);
                }

                return JObject.Parse(JsonConvert.SerializeObject(model));
            }
            catch (Exception Ex)
            {
                model.endpoint = Ex.Message.ToString();
                _webhookService.Create(model);
                throw;
            }

        }


        //Falta crear el modelo que recibe
        [HttpPost]
        public JObject WebhookSATws(Object response)
        {
            DateTime today = DateUtil.GetDateTimeNow();
            Webhook model = new Webhook()
            {
                uuid = Guid.NewGuid(),
                createdAt = today,
                provider = SystemProviders.SATWS.ToString(),
                response = JsonConvert.SerializeObject(response.ToString()),
            };

            try
            {
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                var eventWh = responseData.First(x => x.Key == "events").Value;
                //model.eventWebhook = eventWh.ToString();
                var events = eventWh.ToString();

                if (events != null)
                {
                    string[] eventsArray = events.Split(',');
                    foreach (var item in eventsArray)
                    {
                        model.endpoint = item;
                        _webhookService.Create(model);
                    }

                    return JObject.Parse(JsonConvert.SerializeObject(model));
                }
            }
            catch (Exception Ex)
            {
                model.endpoint = Ex.Message.ToString();
                _webhookService.Create(model);
                throw;
            }
            return null;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SatwsExtractionHandler(Object response)
        {
            LogUtil.AddEntry(
                   JsonConvert.SerializeObject(response.ToString()),
                   ENivelLog.Debug,
                   1,
                   1,
                   EOperacionLog.ACCESS,
                   string.Format("Usuario {0} | Fecha {1}", "wcaamal", DateUtil.GetDateTimeNow()),
                   ControllerContext.RouteData.Values["controller"].ToString() + "/" + Request.RequestContext.RouteData.Values["action"].ToString(),
                   string.Format("Usuario {0} | Fecha {1}", "wcaamal", DateUtil.GetDateTimeNow())
                );
            return null;
        }
    }
}