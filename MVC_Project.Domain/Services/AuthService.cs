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
            /*var user = _repository.Session.CreateSQLQuery("exec dbo.st_user_select_by_email_password "+
                "@Email =:Email, @Password=:Password, @TypeRedSocial=:TypeRedSocial, @SocialId=:SocialId")
                    .SetParameter("Email", username)
                    .SetParameter("Password", password)
                    .SetParameter("TypeRedSocial", typeSocialNetwork)
                    .SetParameter("SocialId", SocialId)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(User)))
                    .UniqueResult<User>();*/

            Profile profile = null;

            User user = (User)_repository.Session.QueryOver<User>()            
                        .JoinAlias(x => x.profile, () => profile);
         



            //(u => u.name == username).FirstOrDefault();
            if (user != null) {
                string hola = user.ToString();
            }//return user;
            return null;
        }
    }
}
