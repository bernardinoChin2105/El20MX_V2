using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IRecurlyPaymentService : IService<RecurlyPayment>
    {
    }
    public class RecurlyPaymentService : ServiceBase<RecurlyPayment>, IRecurlyPaymentService
    {
        private IRepository<RecurlyPayment> _repository;
        public RecurlyPaymentService(IRepository<RecurlyPayment> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
