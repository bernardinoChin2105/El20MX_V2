using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IAccountUserService : IService<AccountUser>
    {
    }

    public class AccountUserService : ServiceBase<AccountUser>, IAccountUserService
    {
        private IRepository<AccountUser> _repository;
        public AccountUserService(IRepository<AccountUser> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
