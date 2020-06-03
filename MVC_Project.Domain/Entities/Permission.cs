﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class Permission : IEntity {
        public virtual int id { get; set; }       
        public virtual Guid uuid { get; set; }
        public virtual string description { get; set; }
        public virtual string controller { get; set; }
        public virtual string action { get; set; }
        public virtual string module { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Account account { get; set; }

        public virtual IList<MembershipPermission> mebershipPermissions { get; set; }

        public Permission()
        {
            mebershipPermissions = new List<MembershipPermission>();
        }
    }
}