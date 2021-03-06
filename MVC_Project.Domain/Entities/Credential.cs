﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Credential : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual Account account { get; set; }
        public virtual string provider { get; set; }
        public virtual string idCredentialProvider { get; set; }
        public virtual string statusProvider { get; set; }
        public virtual string credentialType { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public Credential()
        {            
        }
    }
}
