using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICustomsService : IService<Customs>
    {
    }
    public class CustomsService : ServiceBase<Customs>, ICustomsService
    {
        private IRepository<Customs> _repository;
        public CustomsService(IRepository<Customs> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
