using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace MVC_Project.Domain.Services
{
    public interface IProviderService : IService<Provider>
    {
        List<ListProviders> ListProvider(BasePagination pagination, FilterProvider filter);
        List<ExportListProviders> ExportListProvider(FilterProvider filter);
        List<string> ValidateRFC(List<string> rfcs, Int64 id);
    }

    public class ProviderService : ServiceBase<Provider>, IProviderService 
    {
        private IRepository<Provider> _repository;
        public ProviderService(IRepository<Provider> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<ListProviders> ListProvider(BasePagination pagination, FilterProvider filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_listProvider " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @uuid =:uuid, @rfc=:rfc, @businessName=:businessName, @email=:email ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("uuid", filter.uuid)
                    .SetParameter("rfc", filter.rfc)
                    .SetParameter("businessName", filter.businessName)
                    .SetParameter("email", filter.email)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ListProviders)))
                    .List<ListProviders>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<ExportListProviders> ExportListProvider(FilterProvider filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_exportListProvider " +
                "@uuid =:uuid, @rfc=:rfc, @businessName=:businessName, @email=:email ")
                    .SetParameter("uuid", filter.uuid)
                    .SetParameter("rfc", filter.rfc)
                    .SetParameter("businessName", filter.businessName)
                    .SetParameter("email", filter.email)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ExportListProviders)))
                    .List<ExportListProviders>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<string> ValidateRFC(List<string> rfcs, Int64 id)
        {
            var response = _repository.Session.QueryOver<Provider>()
                .Where(x => x.account.id == id)
                .WhereRestrictionOn(x => x.rfc).IsIn(rfcs)
                .List()
                .Select(x => x.rfc).ToList();

            return response;
        }
    }
}
