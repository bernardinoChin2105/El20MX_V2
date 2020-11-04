using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ITaxRegimeService : IService<TaxRegime>
    {
    }
    public class TaxRegimeService : ServiceBase<TaxRegime>, ITaxRegimeService
    {
        private IRepository<TaxRegime> _repository;
        public TaxRegimeService(IRepository<TaxRegime> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
