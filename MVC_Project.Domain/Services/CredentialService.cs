using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICredentialService : IService<Credential>
    {
        Credential CreateCredentialAccount(Credential credential);
        Credential CreateCredentialAccount(Credential credential, BranchOffice branchOffice);
        Credential CreateCredentialAccount(Credential credential, Discount discount);
        Credential CreateCredentialAccount(Credential credential, Discount discount, BranchOffice branchOffice);
    }

    public class CredentialService : ServiceBase<Credential>, ICredentialService
    {
        private IRepository<Credential> _repository;
        public CredentialService(IRepository<Credential> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Credential CreateCredentialAccount(Credential credential)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(credential.account);
                    _repository.Session.Save(credential);
                    transaction.Commit();
                    return credential;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Credential CreateCredentialAccount(Credential credential, Discount discount)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(credential.account);
                    _repository.Session.Save(credential);
                    _repository.Session.Save(discount);
                    transaction.Commit();
                    return credential;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Credential CreateCredentialAccount(Credential credential, BranchOffice branchOffice)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(credential.account);
                    _repository.Session.Save(credential);
                    _repository.Session.Save(branchOffice);

                    transaction.Commit();
                    return credential;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Credential CreateCredentialAccount(Credential credential, Discount discount, BranchOffice branchOffice)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(credential.account);
                    _repository.Session.Save(credential);
                    _repository.Session.Save(discount);
                    _repository.Session.Save(branchOffice);

                    transaction.Commit();
                    return credential;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}
