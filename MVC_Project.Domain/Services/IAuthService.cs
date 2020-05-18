using MVC_Project.Domain.Entities;

namespace MVC_Project.Domain.Services
{
    public interface IAuthService
    {
        User Authenticate(string username, string password);        
    }
}
