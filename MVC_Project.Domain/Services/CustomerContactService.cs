using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace MVC_Project.Domain.Services
{
    public interface ICustomerContactService : IService<CustomerContact>
    {
    }

    public class CustomerContactService : ServiceBase<CustomerContact>, ICustomerContactService
    {
        private IRepository<CustomerContact> _repository;
        public CustomerContactService(IRepository<CustomerContact> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
