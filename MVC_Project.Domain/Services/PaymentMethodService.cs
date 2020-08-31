using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPaymentMethodService : IService<PaymentMethod>
    {
    }
    public class PaymentMethodService : ServiceBase<PaymentMethod>, IPaymentMethodService
    {
        private IRepository<PaymentMethod> _repository;
        public PaymentMethodService(IRepository<PaymentMethod> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
