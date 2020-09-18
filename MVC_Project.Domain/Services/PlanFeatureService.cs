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
    public interface IPlanFeatureService : IService<PlanFeature>
    {        
    }

    public class PlanFeatureService : ServiceBase<PlanFeature>, IPlanFeatureService
    {
        private IRepository<PlanFeature> _repository;

        public PlanFeatureService(IRepository<PlanFeature> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
