using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class Role : IEntity {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public virtual string Description { get; set; }
        public virtual IList<User> Users { get; set; }
        public virtual IList<Permission> Permissions { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual DateTime? RemovedAt { get; set; }
        public virtual string Uuid { get; set; }
        public virtual Boolean Status { get; set; }

        public Role()
        {
            Users = new List<User>();
            Permissions = new List<Permission>();
        }
    }
}