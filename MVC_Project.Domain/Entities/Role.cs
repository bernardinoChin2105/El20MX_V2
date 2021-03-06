﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class Role : IEntity {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string name { get; set; }
        public virtual string code { get; set; }
        public virtual string description { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Account account { get; set; }

        //public virtual IList<User> users { get; set; }
        public virtual IList<Membership> memberships { get; set; }
        public virtual IList<Permission> permissions { get; set; }
        public virtual IList<RolePermission> rolePermissions { get; set; }

        public Role()
        {
            //users = new List<User>();
            memberships = new List<Membership>();
            permissions = new List<Permission>();
            rolePermissions = new List<RolePermission>();
        }
    }
}