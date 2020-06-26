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
        Account ValidateRFC(string rfc);
    }

    public class AccountService : ServiceBase<Account>, IAccountService
    {
        private IRepository<Account> _repository;
        public AccountService(IRepository<Account> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Account ValidateRFC(string rfc)
        {
            var account = _repository.Session.CreateSQLQuery("exec dbo.st_account_rfc " +
               "@RFC =:RFC")
                   .AddEntity(typeof(Account))
                   .SetParameter("RFC", rfc)
                   .UniqueResult<Account>();

            if (account != null) return account;
            return null;
        }
    }
}
