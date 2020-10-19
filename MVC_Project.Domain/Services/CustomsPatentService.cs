using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICustomsPatentService : IService<CustomsPatent>
    {
    }
    public class CustomsPatentService : ServiceBase<CustomsPatent>, ICustomsPatentService
    {
        private IRepository<CustomsPatent> _repository;
        public CustomsPatentService(IRepository<CustomsPatent> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
