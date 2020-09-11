using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICountryService : IService<Country>
    {
    }
    public class CountryService : ServiceBase<Country>, ICountryService
    {
        private IRepository<Country> _repository;
        public CountryService(IRepository<Country> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
