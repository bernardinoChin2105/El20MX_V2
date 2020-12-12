using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IRecurlySubscriptionService : IService<RecurlySubscription>
    {
    }
    public class RecurlySubscriptionService : ServiceBase<RecurlySubscription>, IRecurlySubscriptionService
    {
        private IRepository<RecurlySubscription> _repository;
        public RecurlySubscriptionService(IRepository<RecurlySubscription> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
