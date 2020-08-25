using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankCredentialService : IService<BankCredential>
    {
        List<BankCredentialsList> GetBankCredentials(Int64 AccountId);
        List<BankAccountsList> GetBanksAccounts(Int64 idCredential);
        List<BankTransactionList> GetBankTransactionList(BasePagination pagination, BankTransactionFilter filter);
    }
    public class BankCredentialService : ServiceBase<BankCredential>, IBankCredentialService
    {
        private IRepository<BankCredential> _repository;
        public BankCredentialService(IRepository<BankCredential> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<BankCredentialsList> GetBankCredentials(Int64 accountId)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankCredentialList " +
                "@accountId =:accountId ")
                    .SetParameter("accountId", accountId)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankCredentialsList)))
                    .List<BankCredentialsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<BankAccountsList> GetBanksAccounts(Int64 idCredential)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankAccountList " +
                "@credentialId =:credentialId ")
                    .SetParameter("credentialId", idCredential)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankAccountsList)))
                    .List<BankAccountsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<BankTransactionList> GetBankTransactionList(BasePagination pagination, BankTransactionFilter filter)
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
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankTransactionList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @createdOnStart=:createdOnStart, @createdOnEnd=:createdOnEnd, "+
                "@accountId =:accountId, @bankId =:bankId, @bankAccountId =:bankAccountId, @movements =: movements")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("createdOnStart", dateinit)
                    .SetParameter("createdOnEnd", dateend)
                    .SetParameter("accountId", filter.accountId)                    
                    .SetParameter("bankId", filter.bankId)
                    .SetParameter("bankAccountId", filter.bankAccountId)
                    .SetParameter("movements", filter.movements)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankTransactionList)))
                    .List<BankTransactionList>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
