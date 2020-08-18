using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankService: IService<Bank>
    {
        //List<string> ValidateBankCredentials(List<string> idCredentials, Int64 id);
        List<BankAccount> GetBanksAccounts(string idCredential);
    }
    public class BankService : ServiceBase<Bank>, IBankService
    {
        private IRepository<Bank> _repository;
        public BankService(IRepository<Bank> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<BankAccount> GetBanksAccounts(string idCredential)
        {
            var response = _repository.Session.QueryOver<BankAccount>()
                //.Where(x => x.credentialProvider == idCredential)
                .List().ToList();
            return response;
        }

        //public List<string> ValidateBankCredentials(List<string> idCredentials, Int64 id)
        //{
        //    var response = _repository.Session.QueryOver<BankCredential>()
        //        .Where(x => x.account.id == id)
        //        .WhereRestrictionOn(x => x.credentailProviderId).IsIn(idCredentials)
        //        .List()
        //        .Select(x => x.credentailProviderId).ToList();

        //    return response;
        //}
    }
}
