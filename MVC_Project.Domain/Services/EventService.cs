using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IEventService : IService<Event>
    {
    }

    public class EventService : ServiceBase<Event>, IEventService
    {
        private IRepository<Event> _repository;

        public EventService(IRepository<Event> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
