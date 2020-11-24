using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankTransactionService : IService<BankTransaction>
    {
    }
    public class BankTransactionService : ServiceBase<BankTransaction>, IBankTransactionService
    {
        private IRepository<BankTransaction> _repository;
        public BankTransactionService(IRepository<BankTransaction> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }

}
