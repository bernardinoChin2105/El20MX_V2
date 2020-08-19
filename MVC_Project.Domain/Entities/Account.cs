using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Account : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string name { get; set; }
        public virtual string rfc { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
        public virtual string avatar { get; set; }
        
        public virtual IList<Membership> memberships { get; set; }
        public virtual IList<BankCredential> bankCredentials { get; set; }
        public virtual IList<BankAccount> bankAccounts { get; set; }

        public Account()
        {
            memberships = new List<Membership>();
            bankCredentials = new List<BankCredential>();
            bankAccounts = new List<BankAccount>();
        }
    }
}
