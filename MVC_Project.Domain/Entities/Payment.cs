using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Payment : IEntity
    {
        public virtual int id { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual string OrderId { get; set; }

        public virtual string Status { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual DateTime? ConfirmationDate { get; set; }

        public virtual string ProviderId { get; set; }

        public virtual string LogData { get; set; }

        public virtual DateTime? DueDate { get; set; }

        public virtual string AuthorizationCode { get; set; }

        public virtual string Method { get; set; }

        public virtual string TransactionType { get; set; }

        public virtual string ConfirmationEmail { get; set; }

        public virtual User User { get; set; }
    }
}
