﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class BankCredential : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string credentialProviderId { get; set; }

        public virtual bool isTwofa { get; set; }
        public virtual DateTime dateTimeAuthorized { get; set; }
        public virtual DateTime dateTimeRefresh { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Account account { get; set; }
        public virtual Bank bank { get; set; }

        public virtual IList<BankAccount> bankAccount { get; set; }
        public BankCredential()
        {
            bankAccount = new List<BankAccount>();
        }
    }
}
