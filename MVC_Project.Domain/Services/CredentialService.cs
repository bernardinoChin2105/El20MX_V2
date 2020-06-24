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
        //Credential SaveCredential();
    }

    public class CredentialService : ServiceBase<Credential>, ICredentialService
    {
        private IRepository<Credential> _repository;
        public CredentialService(IRepository<Credential> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        //public Credential SaveCredential()
        //{

        //}
    }
}
