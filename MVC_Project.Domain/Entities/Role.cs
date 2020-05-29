using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class Role : IEntity {
        public virtual int id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string name { get; set; }
        public virtual string code { get; set; }
        public virtual string description { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        //public virtual IList<User> users { get; set; }
        public virtual IList<AccountUser> accountUsers { get; set; }
        public virtual IList<Permission> permissions { get; set; }

        public Role()
        {
            //users = new List<User>();
            accountUsers = new List<AccountUser>();
            permissions = new List<Permission>();
        }
    }
}