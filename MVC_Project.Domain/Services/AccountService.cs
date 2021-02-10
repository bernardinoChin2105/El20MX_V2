using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using MVC_Project.Utils;
using System.Globalization;
using NHibernate.Linq;

namespace MVC_Project.Domain.Services
{
    public interface IAccountService : IService<Account>
    {
        Account ValidateRFC(string rfc);
        List<AccountCredentialModel> GetAccountRecurly();
        List<AccountCredentialProspectModel> GetAccountCredentialProspect(DateTime DateToday);
        Tuple<IEnumerable<Account>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take);
        List<AccountPaymentsModel> GetLastPaymentsAccount(DateTime DateToday, string statusCode);
    }

    public class AccountService : ServiceBase<Account>, IAccountService
    {
        private IRepository<Account> _repository;
        public AccountService(IRepository<Account> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Account ValidateRFC(string rfc)
        {
            var account = _repository.Session.CreateSQLQuery("exec dbo.st_account_rfc " +
               "@RFC =:RFC")
                   .AddEntity(typeof(Account))
                   .SetParameter("RFC", rfc)
                   .UniqueResult<Account>();

            if (account != null) return account;
            return null;
        }

        public List<AccountCredentialModel> GetAccountRecurly()
        {
            string sql = "select ac.[id], ac.[uuid], ac.[name], ac.[rfc], ac.[planSchema], c.[provider], c.[idCredentialProvider], c.[statusProvider], c.[credentialType] hostedKey, " +
                        "ac.[planFijo], ac.[inicioFacturacion] " +
                        "from [dbo].[accounts] ac " +
                        "inner join [dbo].[credentials] c on ac.id = c.[accountId] " +
                        "left join [dbo].[recurlyPayments] p on p.accountId=ac.id and p.statusCode='success' and " +
                        "YEAR(transactionAt)=YEAR(GETDATE()) and MONTH(transactionAt) = MONTH(getdate()) " +
                        "where c.provider = 'RECURLY' and c.statusProvider = 'active' and ac.[status] = 'ACTIVE' " +
                        "and ac.inicioFacturacion is not null and ac.inicioFacturacion <= DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0) " +
                        "and p.id is null";

            var list = _repository.Session.CreateSQLQuery(sql)
                     //.SetParameter("credentialId", idCredential)
                     .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AccountCredentialModel)))
                     .List<AccountCredentialModel>();

            if (list != null) return list.ToList();
            return null;
        }

        public Tuple<IEnumerable<Account>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take)
        {
            string FilterName = filtersValue.Get("BusinessName").Trim();
            string FilterRfc = filtersValue.Get("Rfc").Trim();
            string FilterAccountOwner = filtersValue.Get("AccountOwner").Trim();
            string FilterStatus = filtersValue.Get("Status").Trim();

            var query = _repository.Session.QueryOver<Account>();

            if (!string.IsNullOrWhiteSpace(FilterName))
            {
                query = query.Where(user => user.name.IsInsensitiveLike("%" + FilterName + "%"));
            }

            if (!string.IsNullOrWhiteSpace(FilterRfc))
            {
                query = query.Where(user => user.rfc.IsInsensitiveLike("%" + FilterRfc + "%"));
            }

            if (!string.IsNullOrWhiteSpace(FilterAccountOwner))
            {
                var accountsWithAccountOwnerFilter = QueryOver.Of<Account>()
                    .JoinQueryOver<Membership>(a => a.memberships)
                    .JoinQueryOver(m => m.user)
                    .JoinQueryOver(u => u.profile)
                    .Where(p => p.firstName.IsInsensitiveLike($"%{FilterAccountOwner}%") || p.lastName.IsInsensitiveLike($"%{FilterAccountOwner}%"))
                    .Select(Projections.Distinct(Projections.Property<Account>(a => a.uuid)));
                query = query.WithSubquery.WhereProperty(a => a.uuid).In(accountsWithAccountOwnerFilter);
            }

            if (!string.IsNullOrWhiteSpace(FilterStatus))
            {
                var fs = (SystemStatus)Enum.Parse(typeof(SystemStatus), FilterStatus);
                query = query.Where(x => x.status == fs.ToString());
            }

            var count = query.RowCount();

            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }
            var list = query.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<Account>, int>(list, count);
        }

        public List<AccountCredentialProspectModel> GetAccountCredentialProspect(DateTime DateToday)
        {
            String today = null;
            if (DateToday != null)
            {
                //dateToday = Convert.ToDateTime(DateToday).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                today = DateToday.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }           

            var list = _repository.Session.CreateSQLQuery("exec dbo.st_prospectList " +
                "@createdOnStart=:createdOnStart")
                    .SetParameter("createdOnStart", today)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AccountCredentialProspectModel)))
                    .List<AccountCredentialProspectModel>();

            if (list != null) return list.ToList();
            return null;
        }

        //Obtener los pagos del mes
        public List<AccountPaymentsModel> GetLastPaymentsAccount(DateTime DateToday, string statusCode)
        {
            String today = null;
            if (DateToday != null)
            {
                //dateToday = Convert.ToDateTime(DateToday).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                today = DateToday.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }

            var list = _repository.Session.CreateSQLQuery("exec dbo.st_getLastFailedPayment " +
                "@createdPayment=:createdPayment,@status=:status")
                    .SetParameter("createdPayment", today)
                    .SetParameter("status", statusCode)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AccountPaymentsModel)))
                    .List<AccountPaymentsModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
