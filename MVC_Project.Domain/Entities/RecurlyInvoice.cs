using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class RecurlyInvoice : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string mounth { get; set; }
        public virtual string year { get; set; }
        public virtual Int32 totalInvoice { get; set; }
        public virtual Int32 totalInvoiceReceived { get; set; }
        public virtual Int32 totalInvoiceIssued { get; set; }
        public virtual Int32 extraBills { get; set; }

        public virtual DateTime createdAt { get; set; }
        //public virtual DateTime modifiedAt { get; set; }
        //public virtual string status { get; set; }

        public virtual RecurlySubscription subscription { get; set; }

        /*Agregar datos por si me hacen falta*/
    }
}
