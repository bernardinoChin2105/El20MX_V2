using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface INotificationService : IService<Notification>
    {
    }
    public class NotificationService : ServiceBase<Notification>, INotificationService
    {
        private IRepository<Notification> _repository;
        public NotificationService(IRepository<Notification> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
