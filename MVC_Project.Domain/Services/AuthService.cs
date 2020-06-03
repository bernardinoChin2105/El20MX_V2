using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
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

        public User GetUserByEmail(string username)
        {
            User user = _repository.FindBy(u => u.name == username).FirstOrDefault();
            if (user != null) return user;
            return null;
        }
    }
}
