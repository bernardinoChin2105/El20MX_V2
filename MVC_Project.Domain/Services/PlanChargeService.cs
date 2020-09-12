using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPlanChargeService : IService<PlanCharge>
    {        
    }

    public class PlanChargeService : ServiceBase<PlanCharge>, IPlanChargeService
    {
        private IRepository<PlanCharge> _repository;

        public PlanChargeService(IRepository<PlanCharge> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
