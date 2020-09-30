
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPlanAssignmentConfigService : IService<PlanAssignmentConfiguration>
    {        
    }

    public class PlanAssignmentConfigService : ServiceBase<PlanAssignmentConfiguration>, IPlanAssignmentConfigService
    {
        private IRepository<PlanAssignmentConfiguration> _repository;

        public PlanAssignmentConfigService(IRepository<PlanAssignmentConfiguration> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
