using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPlanAssignmentsService : IService<PlanAssignment>
    {        
    }

    public class PlanAssignmentsService : ServiceBase<PlanAssignment>, IPlanAssignmentsService
    {
        private IRepository<PlanAssignment> _repository;

        public PlanAssignmentsService(IRepository<PlanAssignment> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
