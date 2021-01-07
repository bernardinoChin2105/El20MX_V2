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

namespace MVC_Project.Domain.Services
{
    public interface IAccountService : IService<Account>
    {
        Account ValidateRFC(string rfc);
        List<AccountCredentialModel> GetAccountRecurly();
        Tuple<IEnumerable<Account>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take);
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
            string sql = "select ac.[id], ac.[uuid], ac.[name], ac.[rfc], ac.[planSchema], c.[provider], c.[idCredentialProvider], c.[statusProvider], c.[credentialType] hostedKey " +
                        "from [dbo].[accounts] ac " +
                        "inner join [dbo].[credentials] c on ac.id = c.[accountId]" +
                        "inner join [dbo].[memberships] m on ac.id = m.[accountId]" +
                        "where c.provider = 'RECURLY' and c.statusProvider = 'active' and ac.[status] = 'ACTIVE' ";

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
            int FilterStatus = Convert.ToInt32(filtersValue.Get("Status").Trim());

            var query = _repository.Session.QueryOver<Account>();

            if (!string.IsNullOrWhiteSpace(FilterName))
            {
                query = query.Where(user => user.name.IsInsensitiveLike("%" + FilterName + "%"));
            }

            if (!string.IsNullOrWhiteSpace(FilterRfc))
            {
                query = query.Where(user => user.rfc.IsInsensitiveLike("%" + FilterRfc + "%"));
            }

            if (FilterStatus != Constants.SEARCH_ALL)
            {
                if (FilterStatus == (int)SystemStatus.ACTIVE)
                    query = query.Where(x => x.status == SystemStatus.ACTIVE.ToString());
                else if (FilterStatus == (int)SystemStatus.INACTIVE)
                    query = query.Where(x => x.status == SystemStatus.INACTIVE.ToString());
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
    }
}
