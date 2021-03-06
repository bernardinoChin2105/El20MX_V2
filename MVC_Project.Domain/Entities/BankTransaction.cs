﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class BankTransaction : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string transactionId { get; set; }
        public virtual string description { get; set; }
        public virtual decimal amount { get; set; }
        public virtual string currency { get; set; }
        public virtual string reference { get; set; }
        public virtual DateTime transactionAt { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual string statusSend { get; set; }
        public virtual string linkError { get; set; }

        public virtual BankAccount bankAccount { get; set; }
    }
}
