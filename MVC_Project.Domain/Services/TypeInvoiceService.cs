using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ITypeInvoiceService : IService<TypeInvoice>
    {
    }
    public class TypeInvoiceService : ServiceBase<TypeInvoice>, ITypeInvoiceService
    {
        private IRepository<TypeInvoice> _repository;
        public TypeInvoiceService(IRepository<TypeInvoice> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
