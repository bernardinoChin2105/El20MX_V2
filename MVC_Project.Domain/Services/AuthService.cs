using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using NHibernate.Transform;
using System.Data;
using System.Data.SqlClient;
//using System.Data;
//using System.Data.SqlClient;
using System.Linq;


namespace MVC_Project.Domain.Services
{
    public class AuthService : IAuthService
    {
        private IRepository<User> _repository;

        public AuthService(IRepository<User> repository)
        {
            _repository = repository;
        }

        public User Authenticate(string username, string password)
        {
            User user = _repository.FindBy(u => u.name == username).FirstOrDefault();
            if (user != null && user.password == password) return user;
            return null;
        }

        public User AuthenticateSocialNetwork(string username, string password, string typeSocialNetwork, string SocialId)
        {
            var user = _repository.Session.CreateSQLQuery("exec dbo.st_user_select_by_email_password " +
                "@Email =:Email, @Password=:Password, @TypeRedSocial=:TypeRedSocial, @SocialId=:SocialId")                    
                    .AddEntity(typeof(User))
                    .SetParameter("Email", username)
                    .SetParameter("Password", password)
                    .SetParameter("TypeRedSocial", typeSocialNetwork)
                    .SetParameter("SocialId", SocialId)
                    .UniqueResult<User>();

            if (user != null) return user;
            return null;
        }
    }
}
