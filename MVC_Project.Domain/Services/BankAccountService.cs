using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankAccountService : IService<BankAccount>
    {
    }
    public class BankAccountService : ServiceBase<BankAccount>, IBankAccountService
    {
        private IRepository<BankAccount> _repository;
        public BankAccountService(IRepository<BankAccount> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }                 
    }
    //public interface IBankAccountService : IService<BankAccount>
    //{
    //}

    //public class BankAccountService : ServiceBase<BankAccount>, IBankAccountService
    //{
    //    private IRepository<BankAccount> _repository;
    //    public BankAccountService(IRepository<BankAccount> baseRepository) : base(baseRepository)
    //    {
    //        _repository = baseRepository;
    //    }
    //}
}
