using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class InvoiceIssued : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string folio { get; set; }
        public virtual string serie { get; set; }
        public virtual string paymentMethod { get; set; }
        public virtual string paymentForm { get; set; }
        public virtual string currency { get; set; }
        public virtual decimal amount { get; set; }
        public virtual decimal iva { get; set; }
        public virtual decimal totalAmount { get; set; }
        public virtual DateTime invoicedAt { get; set; }
        public virtual string xml { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Account account { get; set; }
        public virtual Customer customer { get; set; }        
    }
}
