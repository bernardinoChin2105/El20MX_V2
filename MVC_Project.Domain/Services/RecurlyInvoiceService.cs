using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IRecurlyInvoiceService : IService<RecurlyInvoice>
    {
    }
    public class RecurlyInvoiceService : ServiceBase<RecurlyInvoice>, IRecurlyInvoiceService
    {
        private IRepository<RecurlyInvoice> _repository;
        public RecurlyInvoiceService(IRepository<RecurlyInvoice> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
