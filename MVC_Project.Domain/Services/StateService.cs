using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IStateService : IService<State>
    {

    }
    public class StateService : ServiceBase<State>, IStateService
    {
        private IRepository<State> _repository;
        public StateService(IRepository<State> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
