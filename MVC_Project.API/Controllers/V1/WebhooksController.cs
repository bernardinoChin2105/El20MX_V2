﻿//using LogHubSDK.Models;
using Microsoft.Web.Http;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MVC_Project.API.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/v{version:apiVersion}/Webhooks")]
    public class WebhooksController : ApiController
    {
        private IWebhookService _webhookService;

        public WebhooksController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost]
        [Route("WebhookPaybook")]
        public HttpResponseMessage WebhookPaybook(Object response)
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
                               
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            catch (Exception Ex)
            {
                model.endpoint = Ex.Message.ToString();
                _webhookService.Create(model);
                throw;
            }
        }
    }
}