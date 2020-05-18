using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class Permission : IEntity {
        public virtual int Id { get; set; }        
        public virtual string Description { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
        public virtual string Module { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual DateTime? RemovedAt { get; set; }
    }
}