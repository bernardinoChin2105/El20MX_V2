using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPlanChargeConfigService : IService<PlanChargeConfiguration>
    {        
    }

    public class PlanChargeConfigService : ServiceBase<PlanChargeConfiguration>, IPlanChargeConfigService
    {
        private IRepository<PlanChargeConfiguration> _repository;

        public PlanChargeConfigService(IRepository<PlanChargeConfiguration> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
