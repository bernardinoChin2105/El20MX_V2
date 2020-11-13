using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ISupervisorCADService : IService<SupervisorCAD>
    {
    }
    public class SupervisorCADService : ServiceBase<SupervisorCAD>, ISupervisorCADService
    {
        private IRepository<SupervisorCAD> _repository;
        public SupervisorCADService(IRepository<SupervisorCAD> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

    }
}
