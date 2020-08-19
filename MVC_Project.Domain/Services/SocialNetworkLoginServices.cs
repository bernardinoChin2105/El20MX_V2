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
        SocialNetworkLogin AuthenticateSocialNetwork(int userId, string password, string typeSocialNetwork);
    }
    #endregion

    public class SocialNetworkLoginService : ServiceBase<SocialNetworkLogin>, ISocialNetworkLoginService
    {
        private IRepository<SocialNetworkLogin> _repository;

        public SocialNetworkLoginService(IRepository<SocialNetworkLogin> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public SocialNetworkLogin AuthenticateSocialNetwork(int userId, string socialId, string typeSocialNetwork)
        {
            //_repository.Session.Query
            SocialNetworkLogin login = _repository.FindBy(u => u.user.id == userId).FirstOrDefault();
            if (login != null && login.socialNetwork == typeSocialNetwork && login.token == socialId) return login;
            return null;
        }
    }
}
