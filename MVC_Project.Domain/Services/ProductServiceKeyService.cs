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
    public interface IProductServiceKeyService : IService<ProductServiceKey>
    {
        List<ProductServiceKeyViewModel> GetProdServ(string concept);
    }
    public class ProductServiceKeyService : ServiceBase<ProductServiceKey>, IProductServiceKeyService
    {
        private IRepository<ProductServiceKey> _repository;
        public ProductServiceKeyService(IRepository<ProductServiceKey> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<ProductServiceKeyViewModel> GetProdServ(string concept)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_ProductServiceSearchList " +
                "@concept =:concept ")
                    .SetParameter("concept", concept)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ProductServiceKeyViewModel)))
                    .List<ProductServiceKeyViewModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
