using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
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
    }

    #endregion Interfaces
    public class DiscountService : ServiceBase<Discount>, IDiscountService
    {
        private IRepository<Discount> _repository;

        public DiscountService(IRepository<Discount> baseRepository): base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
