using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Desktop.Helpers
{
    public class Authenticator
    {
        static AuthUser currentUser;

        public static AuthUser GetCurrentUser()
        {
            return currentUser;
        }

        public static void SetCurrentUser(AuthUser authUser)
        {
            currentUser = authUser;
        }

    }

    public class AuthUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Uuid { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
