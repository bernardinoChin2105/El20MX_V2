using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Quotation : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual DateTime startedAt{get;set;}
        public virtual decimal total { get; set; }

        public virtual bool hasDeferredPayment { get; set; }
        public virtual int partialitiesNumber { get; set; }
        public virtual decimal advancePayment { get; set; }
        public virtual decimal monthlyCharge { get; set; }

        public virtual Account account{ get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual IList<QuotationDetail> quotationDetails { get; set; }

        public Quotation()
        {
            quotationDetails = new List<QuotationDetail>();
        }
    }
}
