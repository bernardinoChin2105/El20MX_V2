using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Customer : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual Account account { get; set; }
        public virtual string firstName { get; set; }
        public virtual string lastName { get; set; }
        public virtual string rfc { get; set; }
        public virtual string curp { get; set; }
        public virtual string businessName { get; set; }
        public virtual string taxRegime { get; set; }
        public virtual string street{ get; set; }
        public virtual string interiorNumber { get; set; }
        public virtual string outdoorNumber { get; set; }
        public virtual Int64 colony { get; set; }
        public virtual string zipCode { get; set; }
        public virtual Int64 municipality { get; set; }
        public virtual Int64 state { get; set; }
        public virtual Int64 country { get; set; }
        public virtual bool deliveryAddress { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual IList<CustomerContact> customerContacts { get; set; }

        public Customer()
        {            
            customerContacts = new List<CustomerContact>();
        }        
    }
}
