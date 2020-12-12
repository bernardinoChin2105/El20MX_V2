using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class RecurlyPayment
    {
        public virtual Int64 id { get; set; }
        public virtual decimal total { get; set; }
        public virtual decimal subtotal { get; set; }
        public virtual string paymentGateway { get; set; }
        public virtual string statusCode { get; set; }
        public virtual string statusMessage { get; set; }
        public virtual string customerMessage { get; set; }
        public virtual DateTime createdAt { get; set; }
        /*Agregar datos por si me hacen falta*/

        //public virtual DateTime modifiedAt { get; set; }
        //public virtual string status { get; set; }

        public virtual RecurlySubscription subscription { get; set; }
    }
}
