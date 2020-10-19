using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IProductServiceKeyService : IService<ProductServiceKey>
    {
    }
    public class ProductServiceKeyService : ServiceBase<ProductServiceKey>, IProductServiceKeyService
    {
        private IRepository<ProductServiceKey> _repository;
        public ProductServiceKeyService(IRepository<ProductServiceKey> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
