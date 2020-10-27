using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IUseCFDIService : IService<UseCFDI>
    {
    }
    public class UseCFDIService : ServiceBase<UseCFDI>, IUseCFDIService
    {
        private IRepository<UseCFDI> _repository;
        public UseCFDIService(IRepository<UseCFDI> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }        
    }
}
