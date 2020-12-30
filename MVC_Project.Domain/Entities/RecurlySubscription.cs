using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class RecurlySubscription : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string subscriptionId { get; set; }
        public virtual string planId { get; set; }
        public virtual string planCode { get; set; }
        public virtual string planName { get; set; }
        public virtual string state { get; set; }
        //public virtual DateTime created_atRecurly { get; set; }
        //public virtual DateTime created_atRecurly { get; set; }
        //Averiguar los días de cobros y termino
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public virtual Account account { get; set; }

        /*Agregar datos por si me hacen falta*/
    }
}
