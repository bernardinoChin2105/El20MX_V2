using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.Web.AuthManagement;
using MVC_Project.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
{
    public class CalendarController : BaseController
    {
        IEventService _eventService;

        public CalendarController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet, Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAllByFilter(string start, string end)
        {
            try
            {
                //NameValueCollection filtersValues = HttpUtility.ParseQueryString(filtros);
                var results = _eventService.GetAll();
                IList<EventData> dataResponse = new List<EventData>();
                foreach (var eventBO in results)
                {
                    EventData eventData = new EventData();
                    eventData.Id = eventBO.Id;
                    eventData.Uuid = eventBO.Uuid;
                    eventData.Title = eventBO.Title;
                    eventData.Description = eventBO.Description;
                    eventData.Start = eventBO.StartDate.ToString(Constants.DATE_FORMAT_CALENDAR);
                    eventData.End = eventBO.EndDate.HasValue ? eventBO.EndDate.Value.ToString(Constants.DATE_FORMAT_CALENDAR) : string.Empty;
                    eventData.IsFullDay = eventBO.IsFullDay;
                    dataResponse.Add(eventData);
                }
                return Json(dataResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveEvent(EventData model)
        {
            DateTime? startDate = DateUtil.ToDateTime(model.Start, Constants.DATE_FORMAT_CALENDAR);
            DateTime? endDate = DateUtil.ToDateTime(model.End, Constants.DATE_FORMAT_CALENDAR);
            var status = false;

            if (startDate.HasValue)
            {
                if (!string.IsNullOrEmpty(model.Uuid))
                {
                    Event eventBO = _eventService.FindBy(x => x.Uuid == model.Uuid).FirstOrDefault();
                    eventBO.Title = model.Title;
                    eventBO.Description = model.Description;
                    eventBO.StartDate = startDate.Value;
                    eventBO.EndDate = endDate;
                    eventBO.IsFullDay = model.IsFullDay;
                    _eventService.Update(eventBO);
                }
                else
                {
                    Event eventBO = new Event();
                    eventBO.Uuid = Guid.NewGuid().ToString();
                    eventBO.Title = model.Title;
                    eventBO.Description = model.Description;
                    eventBO.IsFullDay = model.IsFullDay;
                    eventBO.User = new User() { Id = Authenticator.AuthenticatedUser.Id };
                    eventBO.CreationDate = DateUtil.GetDateTimeNow();
                    eventBO.StartDate = startDate.Value;
                    eventBO.EndDate = endDate;
                    _eventService.Create(eventBO);
                }
                status = true;
            }
            
            return new JsonResult { Data = new { status } };
        }

        [HttpPost, Authorize]
        public JsonResult DeleteEvent(string uuid)
        {
            var status = false;
            if (!string.IsNullOrEmpty(uuid))
            {
                Event eventBO = _eventService.FindBy(x => x.Uuid == uuid).FirstOrDefault();
                _eventService.Delete(eventBO.Id);
                status = true;
            }
            
            return new JsonResult { Data = new { status } };
        }
}
}