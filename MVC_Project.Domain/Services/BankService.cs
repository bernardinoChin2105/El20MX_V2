using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankService: IService<Bank>
    {       
    }
    public class BankService : ServiceBase<Bank>, IBankService
    {
        private IRepository<Bank> _repository;
        public BankService(IRepository<Bank> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
