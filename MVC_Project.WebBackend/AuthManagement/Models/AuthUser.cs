using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.AuthManagement.Models
{
    public class AuthUser
    {
        public Int64 Id { get; set; }
        public string FirstName { get; set; }
        public Guid Uuid { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Language { get; set; }
        public string Avatar { get; set; }
        public Role Role { get; set; }
        public IList<Permission> Permissions { get; set; }

        public DateTime? PasswordExpiration { get; set; }

        public Account Account { get; set; }

        public bool HasAccessToModule(string module)
        {
            if (this.Permissions != null && this.Permissions.Count > 0)
            {
                var per = from ap in this.Permissions
                          where ap.Module == module && ap.Level != SystemLevelPermission.NO_ACCESS.ToString() && ap.isCustomizable
                          select ap;
                return per.Count<Permission>() > 0;
            }
            return false;
        }

        public bool HasAccessController(string controller)
        {
            if (this.Permissions != null && this.Permissions.Count > 0)
            {
                var per = from ap in this.Permissions where ap.Controller == controller select ap;
                return per.Count<Permission>() > 0;
            }
            return false;
        }

        public bool HasFullAccessController(string controller)
        {
            if (this.Permissions != null && this.Permissions.Count > 0)
            {
                var per = from ap in this.Permissions where ap.Controller == controller && ap.Level == SystemLevelPermission.FULL_ACCESS.ToString() select ap;
                return per.Count<Permission>() > 0;
            }
            return false;
        }

        public string GetControllerModule(string controller)
        {
            if (this.Permissions != null && this.Permissions.Count > 0)
            {
                var per = from ap in this.Permissions where ap.Controller == controller select ap;
                return per.Count() > 0 ? per.First<Permission>().Module : string.Empty;
            }
            return string.Empty;
        }
    }

    public class Role
    {
        public Int64 Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class Permission
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string Level { get; set; }
        public bool isCustomizable { get; set; }
    }

    public class Account
    {
        public Int64 Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string RFC { get; set; }
        public string Image { get; set; }
    }
    
}