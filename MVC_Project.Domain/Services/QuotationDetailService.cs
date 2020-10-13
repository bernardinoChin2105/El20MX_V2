using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IQuotationDetailService: IService<QuotationDetail>
    {
    }
    public class QuotationDetailService:ServiceBase<QuotationDetail>, IQuotationDetailService
    {
        private IRepository<QuotationDetail> _repository;
        public QuotationDetailService(IRepository<QuotationDetail> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
