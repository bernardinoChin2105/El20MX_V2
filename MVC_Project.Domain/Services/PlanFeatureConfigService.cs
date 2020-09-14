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
    public interface IPlanFeatureConfigService : IService<PlanFeatureConfiguration>
    {        
    }

    public class PlanFeatureConfigService : ServiceBase<PlanFeatureConfiguration>, IPlanFeatureConfigService
    {
        private IRepository<PlanFeatureConfiguration> _repository;

        public PlanFeatureConfigService(IRepository<PlanFeatureConfiguration> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
