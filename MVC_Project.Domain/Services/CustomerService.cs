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
    public interface ICustomerService : IService<Customer>
    {
        List<CustomerList> CustomerList(BasePagination pagination, CustomerFilter filter);
        List<ExportListCustomer> ExportListCustomer(CustomerFilter filter);
        List<string> ValidateRFC(List<string> rfcs, Int64 id);
        List<InvoicesIssuedList> CustomerCDFIList(BasePagination pagination, CustomerCFDIFilter filter);
        List<ListCustomersAC> ListCustomerAutoComplete(Int64 id);
        List<ListCustomersProvider> ReceiverSearchList(ReceiverFilter filter);
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

        public List<ExportListCustomer> ExportListCustomer(CustomerFilter filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_exportListCustomer " +
                "@uuid =:uuid, @rfc=:rfc, @businessName=:businessName, @email=:email ")
                    .SetParameter("uuid", filter.uuid)
                    .SetParameter("rfc", filter.rfc)
                    .SetParameter("businessName", filter.businessName)
                    .SetParameter("email", filter.email)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ExportListCustomer)))
                    .List<ExportListCustomer>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<string> ValidateRFC(List<string> rfcs, Int64 id)
        {
            var response = _repository.Session.QueryOver<Customer>()
                .Where(x => x.account.id == id)
                .WhereRestrictionOn(x => x.rfc).IsIn(rfcs)
                .List()
                .Select(x => x.rfc).ToList();

            return response;
        }

        public List<InvoicesIssuedList> CustomerCDFIList(BasePagination pagination, CustomerCFDIFilter filter)
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

            var list = _repository.Session.CreateSQLQuery("exec dbo.st_customerInvoicesList " +
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
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(InvoicesIssuedList)))
                    .List<InvoicesIssuedList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<ListCustomersAC> ListCustomerAutoComplete(Int64 id)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_customerAutocompleted " +
                "@accountId =:accountId")
                    .SetParameter("accountId", id)
                    //.SetParameter("rfc", rfc)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ListCustomersAC)))
                    .List<ListCustomersAC>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<ListCustomersProvider> ReceiverSearchList(ReceiverFilter filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_receiverSearchList " +
                "@uuid =:uuid, @businessName=:businessName, @typeInvoice=:typeInvoice ")                   
                    .SetParameter("uuid", filter.uuid)
                    //.SetParameter("rfc", filter.rfc)
                    .SetParameter("businessName", filter.businessName)
                    .SetParameter("typeInvoice", filter.typeInvoice)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(ListCustomersProvider)))
                    .List<ListCustomersProvider>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
