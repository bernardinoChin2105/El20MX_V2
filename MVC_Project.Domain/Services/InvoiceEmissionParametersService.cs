using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;

namespace MVC_Project.Domain.Services
{
    public interface IInvoiceEmissionParametersService : IService<InvoiceEmissionParameters>
    {
    }
    public class InvoiceEmissionParametersService : ServiceBase<InvoiceEmissionParameters>, IInvoiceEmissionParametersService
    {
        private IRepository<InvoiceEmissionParameters> _repository;
        public InvoiceEmissionParametersService(IRepository<InvoiceEmissionParameters> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
