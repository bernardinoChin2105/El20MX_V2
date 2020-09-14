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
using System.Globalization;

namespace MVC_Project.Domain.Services
{
    public interface IProviderService : IService<Provider>
    {
        List<ListProviders> ListProvider(BasePagination pagination, FilterProvider filter);
        List<ExportListProviders> ExportListProvider(FilterProvider filter);
        List<string> ValidateRFC(List<string> rfcs, Int64 id);
        List<InvoicesReceivedList> ProviderCDFIList(BasePagination pagination, CustomerCFDIFilter filter);
        List<ListProvidersAC> ListProviderAutoComplete(Int64 id);
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

        public List<InvoicesReceivedList> ProviderCDFIList(BasePagination pagination, CustomerCFDIFilter filter)
        {
            String dateinit = null;
            String dateend = null;
            if (pagination.CreatedOnStart != null)
            {
                dateinit = Convert.ToDateTime(pagination.CreatedOnStart).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }
            if (pagination.CreatedOnEnd != null)
            {
                dateend = Convert.ToDateTime(pagination.CreatedOnEnd).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }

            var list = _repository.Session.CreateSQLQuery("exec dbo.st_providerInvoicesList " +
                "@PageNum=:PageNum, @PageSize=:PageSize, @createdOnStart=:createdOnStart, @createdOnEnd=:createdOnEnd, " +
                "@accountId=:accountId, @folio=:folio, @rfc=:rfc, @paymentMethod=:paymentMethod, @paymentForm=:paymentForm, " +
                "@currency=:currency, @serie=:serie, @nombreRazonSocial=:nombreRazonSocial ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("createdOnStart", dateinit)
                    .SetParameter("createdOnEnd", dateend)
                    .SetParameter("accountId", filter.accountId)
                    .SetParameter("folio", filter.folio)
                    .SetParameter("rfc", filter.rfc)
                    .SetParameter("paymentMethod", filter.paymentMethod)
                    .SetParameter("paymentForm", filter.paymentForm)
                    .SetParameter("currency", filter.currency)
                    .SetParameter("serie", filter.serie)
                    .SetParameter("nombreRazonSocial", filter.nombreRazonSocial)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(InvoicesReceivedList)))
                    .List<InvoicesReceivedList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<ListProvidersAC> ListProviderAutoComplete(Int64 id)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_providerAutocompleted " +
                "@accountId =:accountId")
                    .SetParameter("accountId", id)
                    //.SetParameter("rfc", rfc)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ListProvidersAC)))
                    .List<ListProvidersAC>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
