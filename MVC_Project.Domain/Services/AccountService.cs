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
    public interface IAccountService : IService<Account>
    {
        Account ValidateRFC(string rfc);
        List<AccounModel> GetAccountRecurly();
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

        public List<AccounModel> GetAccountRecurly()
        {
            string sql = "select ac.[id], ac.[uuid], ac.[name], ac.[rfc], c.[provider], c.[idCredentialProvider], c.[statusProvider], c.[credentialType] hostedKey " +
                        "from[dbo].[accounts] ac " +
                        "inner join[dbo].[credentials] c on ac.id = [accountId]" +
                        "inner join[dbo].[memberships] m on ac.id = m.[accountId]" +
                        "where c.provider = 'RECURLY' and c.statusProvider = 'active' and ac.[status] = 'ACTIVE' ";

            var list = _repository.Session.CreateSQLQuery("exec " + sql)
                     //.SetParameter("credentialId", idCredential)
                     .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AccounModel)))
                     .List<AccounModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
