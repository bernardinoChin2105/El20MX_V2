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
    public interface ICurrencyService : IService<Currency>
    {
    }
    public class CurrencyService : ServiceBase<Currency>, ICurrencyService
    {
        private IRepository<Currency> _repository;
        public CurrencyService(IRepository<Currency> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }       
    }
}
