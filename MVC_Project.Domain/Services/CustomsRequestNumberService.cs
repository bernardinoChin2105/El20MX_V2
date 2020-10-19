using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICustomsRequestNumberService : IService<CustomsRequestNumber>
    {
    }
    public class CustomsRequestNumberService : ServiceBase<CustomsRequestNumber>, ICustomsRequestNumberService
    {
        private IRepository<CustomsRequestNumber> _repository;
        public CustomsRequestNumberService(IRepository<CustomsRequestNumber> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
