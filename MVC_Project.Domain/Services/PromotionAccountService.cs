using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPromotionAccountService : IService<PromotionAccount>
    {
        List<PromotionAccount> GetPromotionAccount(string uuidPromotion);
    }

    public class PromotionAccountService : ServiceBase<PromotionAccount>, IPromotionAccountService
    {
        private IRepository<PromotionAccount> _repository;
        public PromotionAccountService(IRepository<PromotionAccount> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<PromotionAccount> GetPromotionAccount(string uuidPromotion)
        {
            var prmAccount = _repository.FindBy(x => x.promotion.uuid.ToString() == uuidPromotion);

            if (prmAccount == null)
                return null;

            return prmAccount.ToList();
        }
    }
}
