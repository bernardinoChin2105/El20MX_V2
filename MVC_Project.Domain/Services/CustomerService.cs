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
    public interface ICustomerService : IService<Customer>
    {
        List<CustomerList> CustomerList(BasePagination pagination, CustomerFilter filter);
    }

    public class CustomerService : ServiceBase<Customer>, ICustomerService 
    {
        private IRepository<Customer> _repository;
        public CustomerService(IRepository<Customer> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<CustomerList> CustomerList(BasePagination pagination, CustomerFilter filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_customerList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @uuid =:uuid, @rfc=:rfc, @businessName=:businessName, @email=:email ")
                    //.AddEntity(typeof(DiagnosticList))
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("uuid", filter.uuid)
                    .SetParameter("rfc", filter.rfc)
                    .SetParameter("businessName", filter.businessName)
                    .SetParameter("email", filter.email)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(CustomerList)))
                    .List<CustomerList>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
