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
    #region Interfaces

    public interface IDiscountService : IService<Discount>
    {
        List<Discount> GetDiscounts(string uuidPromotion);
    }

    #endregion Interfaces
    public class DiscountService : ServiceBase<Discount>, IDiscountService
    {
        private IRepository<Discount> _repository;

        public DiscountService(IRepository<Discount> baseRepository): base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<Discount> GetDiscounts(string uuidPromotion)
        {
            var discounts = _repository.FindBy(x => x.promotion.uuid.ToString() == uuidPromotion &&
               x.status == SystemStatus.ACTIVE.ToString());

            if (discounts == null)
                return null;
            
            return discounts.ToList();
        }
    }
}
