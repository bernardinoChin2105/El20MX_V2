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
    public interface IAllianceService : IService<Alliance>
    {
    }
    public class AllianceService : ServiceBase<Alliance>, IAllianceService
    {
        private IRepository<Alliance> _repository;
        public AllianceService(IRepository<Alliance> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }       
    }
}
