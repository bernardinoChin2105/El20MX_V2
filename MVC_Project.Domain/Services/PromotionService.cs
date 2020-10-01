using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPromotionService : IService<Promotion>
    {
        List<PromotionsList> GetPromotionList(BasePagination pagination, string name, string type);
        Promotion GetValidityPromotion(string type);
    }

    public class PromotionService : ServiceBase<Promotion>, IPromotionService
    {
        private IRepository<Promotion> _repository;
        public PromotionService(IRepository<Promotion> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<PromotionsList> GetPromotionList(BasePagination pagination, string name, string type)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_promotionsList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name=:Name, @Type=:Type ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", name)
                    .SetParameter("Type", type)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(PromotionsList)))
                    .List<PromotionsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public Promotion GetValidityPromotion(string type)
        {
            var promocion = _repository.FirstOrDefault(x => x.type == type &&
               x.status == SystemStatus.ACTIVE.ToString());

            if (promocion == null)
                return null;

            if (promocion.hasValidity)
            {
                if (promocion.validityInitialAt > DateTime.Now.Date || promocion.validityFinalAt < DateTime.Now.Date)
                {
                    return null;
                }
            }
            
            return promocion;
        }
    }
}
