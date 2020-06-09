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
            //[dbo].[st_user_select_by_email_password]

            //@IDPers=:IDPers, @jaar=:jaar, @maand=:maand
            var user = _repository.Session.CreateSQLQuery("exec dbo.st_user_select_by_email_password "+
                "@Email =:Email, @Password=:Password, @TypeRedSocial=:TypeRedSocial, @SocialId=:SocialId")
                    .SetParameter("Email", username)
                    .SetParameter("Password", password)
                    .SetParameter("TypeRedSocial", typeSocialNetwork)
                    .SetParameter("SocialId", SocialId)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(User)))
                    .UniqueResult<User>();
                    //.SetResultTransformer(new AliasToBeanResultTransformer(typeof(User))).
            //.List<User>()
            //.ToList();

            //var user = queryU;

            //var emailParameter = new SqlParameter("@Email", SqlDbType.NVarChar);
            //emailParameter.Value = username;

            //var passwordParameter = new SqlParameter("Password", SqlDbType.NVarChar);
            //passwordParameter.Value = password;

            //var typeSNParameter = new SqlParameter("@TypeRedSocial", SqlDbType.NVarChar);
            //typeSNParameter.Value = typeSocialNetwork;

            //var SocialIdParameter = new SqlParameter("SocialId", SqlDbType.NVarChar);
            //SocialIdParameter.Value = SocialId;


            //User user = _repository.Session.CreateSQLQuery<User>("exec dbo.st_user_select_by_email_password " +
            //"@Email" +
            //",@Password" +
            //",@TypeRedSocial" +
            //",@SocialId",
            //emailParameter,
            //passwordParameter,
            //typeSNParameter,
            //SocialIdParameter
            //).FirstOrDefault();

            /*.GetCurrentSession()
                .GetNamedQuery("GetForumProfileDetails")
                .SetInt32("UserID", user.UserID)
                .SetResultTransformer(
                        Transformers.AliasToBean(typeof(ForumProfile)))
                .UniqueResult<ForumProfile>();*/

            //User user = _repository.Session.GetNamedQuery("st_user_select_by_email_password")
            //    .SetString("Email", username)
            //    .SetString("Password", password)
            //    .SetString("TypeRedSocial", typeSocialNetwork)
            //    .SetString("SocialId", SocialId)
            //    .SetResultTransformer(Transformers.AliasToBean(typeof(User)))
            //    .UniqueResult<User>();



            //(u => u.name == username).FirstOrDefault();
            if (user != null) {
                string hola = user.ToString();
            }//return user;
            return null;
        }
    }
}
