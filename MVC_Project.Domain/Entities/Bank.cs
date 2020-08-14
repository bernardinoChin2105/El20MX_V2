using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Bank
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string name { get; set; }
        public virtual string providerId { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual IList<BankCredential> bankCredential { get; set; }
        public virtual IList<BankAccount> bankAccounts { get; set; }

        public Bank()
        {
            bankCredential = new List<BankCredential>();
            bankAccounts = new List<BankAccount>();
        }
    }
}
