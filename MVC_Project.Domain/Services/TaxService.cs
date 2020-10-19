using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ITaxService : IService<Tax>
    {
    }
    public class TaxService : ServiceBase<Tax>, ITaxService
    {
        private IRepository<Tax> _repository;
        public TaxService(IRepository<Tax> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
