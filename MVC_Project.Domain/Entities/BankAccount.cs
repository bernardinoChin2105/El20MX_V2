using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class BankAccount : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string accountProviderId { get; set; }
        public virtual string accountProviderType { get; set; }
        public virtual string name { get; set; }
        public virtual double balance { get; set; }
        public virtual string currency { get; set; }
        public virtual string number { get; set; }
        public virtual Int32 isDisable { get; set; }
        public virtual DateTime refreshAt { get; set; }

        public virtual string clabe { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual BankCredential bankCredential { get; set; }       

        public virtual IList<BankTransaction> bankTransaction { get; set; }
        public BankAccount()
        {
            bankTransaction = new List<BankTransaction>();
        }
    }
}
