using MVC_Project.Data.Helpers;
using MVC_Project.Data.Repositories;
using MVC_Project.Domain.Entities;
using MVC_Project.Utils;
using MVC_Project.WebBackend.AuthManagement;
using MVC_Project.WebBackend.AuthManagement.Models;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Helpers
{
    public class NotificationsHelperExtensions
    {
        public static List<NotificationViewModel> Notifications
        {
            get
            {
                var _unitOfWork = new UnitOfWork();
                var _repositoryNotificacion = new Repository<Notification>(_unitOfWork);

                var push = new List<NotificationViewModel>();
                var user = Authenticator.AuthenticatedUser;
                if (user != null && user.Account != null)
                {
                    var notificaciones = _repositoryNotificacion.FindBy(x => x.account.id == user.Account.Id
                    && x.status == NotificationStatus.ACTIVE.ToString());

                    push = notificaciones.Select(x => new NotificationViewModel
                    {
                        Id = x.id,
                        Message = x.message,
                        Moment = "Hace " + ((DateTime.Now - x.modifiedAt).Days > 0 ? (DateTime.Now - x.modifiedAt).Days + " dias" :
                        (DateTime.Now - x.modifiedAt).Hours > 0 ? (DateTime.Now - x.modifiedAt).Hours + " horas" :
                        (DateTime.Now - x.modifiedAt).Minutes > 0 ? (DateTime.Now - x.modifiedAt).Minutes + " minutos" :
                        " un momento")
                    }).ToList();
                }
                return push;
            }
        }
    }
}