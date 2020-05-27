using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IAccountService : IService<Account>
    {
    }

    public class AccountService : ServiceBase<Account>, IAccountService
    {
        private IRepository<Account> _repository;
        public AccountService(IRepository<Account> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
