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
    public interface IPaymentFormService : IService<PaymentForm>
    {        
    }
    public class PaymentFormService : ServiceBase<PaymentForm>, IPaymentFormService
    {
        private IRepository<PaymentForm> _repository;
        public PaymentFormService(IRepository<PaymentForm> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
