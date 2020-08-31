using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IInvoiceReceivedService : IService<InvoiceReceived>
    {
    }

    public class InvoiceReceivedService : ServiceBase<InvoiceReceived>, IInvoiceReceivedService
    {
        private IRepository<InvoiceReceived> _repository;
        public InvoiceReceivedService(IRepository<InvoiceReceived> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
