﻿using MVC_Project.Data.Helpers;
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
                        Moment = "Hace " + ((DateUtil.GetDateTimeNow() - x.createdAt).Days > 0 ? (DateUtil.GetDateTimeNow() - x.createdAt).Days + " dias" :
                        (DateUtil.GetDateTimeNow() - x.createdAt).Hours > 0 ? (DateUtil.GetDateTimeNow() - x.createdAt).Hours + " horas" :
                        (DateUtil.GetDateTimeNow() - x.createdAt).Minutes > 0 ? (DateUtil.GetDateTimeNow() - x.createdAt).Minutes + " minutos" :
                        " un momento")
                    }).ToList();
                }
                return push;
            }
        }
    }
}