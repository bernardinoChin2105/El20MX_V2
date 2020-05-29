using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities
{
    public class User : IEntity
    {
        public virtual int id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string name { get; set; }
        public virtual string password { get; set; }
        public virtual DateTime? passwordExpiration { get; set; }
        public virtual string token { get; set; }
        public virtual DateTime? tokenExpiration { get; set; }
        public virtual DateTime? lastLoginAt { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Profile profile { get; set; }

        public virtual IList<Permission> permissions { get; set; }
        //public virtual IList<Account> accounts { get; set; }
        public virtual IList<AccountUser> accountUsers { get; set; }

        public User()
        {            
            permissions = new List<Permission>();
            //accounts = new List<Account>();
            accountUsers = new List<AccountUser>();
        }
    }
}