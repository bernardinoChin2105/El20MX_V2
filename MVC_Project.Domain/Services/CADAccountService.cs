using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICADAccountService : IService<CADAccount>
    {
    }
    public class CADAccountService : ServiceBase<CADAccount>, ICADAccountService
    {
        private IRepository<CADAccount> _repository;
        public CADAccountService(IRepository<CADAccount> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

    }
}
