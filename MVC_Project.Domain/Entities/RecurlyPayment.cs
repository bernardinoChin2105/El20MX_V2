using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class RecurlyPayment : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string email { get; set; }
        public virtual decimal total { get; set; }
        public virtual decimal subtotal { get; set; }
        public virtual string paymentGateway { get; set; }
        public virtual string statusCode { get; set; }
        public virtual string statusMessage { get; set; }
        public virtual string customerMessage { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual string transactionId { get; set; }
        public virtual string stampStatus { get; set; }
        public virtual int stampAttempt { get; set; }
        public virtual string stampStatusMessage { get; set; }
        public virtual InvoiceReceived invoice { get; set; }
        /*Agregar datos por si me hacen falta*/

        //public virtual DateTime modifiedAt { get; set; }
        //public virtual string status { get; set; }

        public virtual RecurlySubscription subscription { get; set; }
    }
}
