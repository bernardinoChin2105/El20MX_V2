using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class NotificationController : Controller
    {
        private INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [AllowAnonymous]
        public ActionResult Confirm(int id)
        {
            try
            {
                var notificacion = _notificationService.GetById(id);

                if (notificacion != null)
                {
                    notificacion.status = NotificationStatus.CONFIRMED.ToString();
                    notificacion.modifiedAt = DateUtil.GetDateTimeNow();
                    _notificationService.Update(notificacion);
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}