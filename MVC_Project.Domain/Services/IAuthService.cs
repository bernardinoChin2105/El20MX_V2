using MVC_Project.Domain.Entities;

namespace MVC_Project.Domain.Services
{
    public interface IAuthService
    {
        User Authenticate(string username, string password);        
        User AuthenticateSocialNetwork(string username, string password, string typeSocialNetwork, string SocialId);
    }
}
