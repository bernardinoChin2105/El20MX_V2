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
    public interface IBankCredentialService : IService<BankCredential>
    {               
        List<BankCredentialsList> GetBankCredentials(Int64 AccountId);
        List<BankAccountsList> GetBanksAccounts(Int64 idCredential);
    }
    public class BankCredentialService : ServiceBase<BankCredential>, IBankCredentialService
    {
        private IRepository<BankCredential> _repository;
        public BankCredentialService(IRepository<BankCredential> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<BankCredentialsList> GetBankCredentials(Int64 AccountId)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankCredentialList " +
                "@accountId =:accountId ")
                    .SetParameter("accountId", AccountId)
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
    }
}
