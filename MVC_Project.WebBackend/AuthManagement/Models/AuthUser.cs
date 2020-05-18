﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.AuthManagement.Models
{
    public class AuthUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Uuid { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Language { get; set; }
        public Role Role { get; set; }
        public IList<Permission> Permissions { get; set; }

        public DateTime? PasswordExpiration { get; set; }

        public bool HasAccessToModule(string module)
        {
            if (this.Permissions != null && this.Permissions.Count > 0)
            {
                var per = from ap in this.Permissions where ap.Module == module select ap;
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
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class Permission
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
    }
    
}