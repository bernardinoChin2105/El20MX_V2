using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    [Serializable]
    public class UserProfile
    {
        public string Uuid { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public IList<Permission> AccessPermissions { get; set; }
    }
}