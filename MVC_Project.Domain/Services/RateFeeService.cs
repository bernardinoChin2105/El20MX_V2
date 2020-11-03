using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IRateFeeService : IService<RateFee>
    {
    }

    public class RateFeeService : ServiceBase<RateFee>, IRateFeeService
    {
        private IRepository<RateFee> _repository;
        public RateFeeService(IRepository<RateFee> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
