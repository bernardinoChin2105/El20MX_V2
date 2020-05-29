using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    #region Interfaces
    public interface ISocialNetworkLoginService : IService<SocialNetworkLogin>
    {
    }
    #endregion
    public class SocialNetworkLoginService : ServiceBase<SocialNetworkLogin>, ISocialNetworkLoginService
    {
        private IRepository<SocialNetworkLogin> _repository;

        public SocialNetworkLoginService(IRepository<SocialNetworkLogin> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
