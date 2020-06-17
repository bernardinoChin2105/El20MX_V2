using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class PaymentApplication : IEntity
    {
        public virtual Int64 id { get; set; }
        
        public virtual string AppKey { get; set; }

        public virtual string Name { get; set; }

        public virtual bool Active { get; set; }

        public virtual User User { get; set; }

        public virtual string PublicKey { get; set; }
        public virtual string PrivateKey { get; set; }
        public virtual string MerchantId { get; set; }

        public virtual string ClientId { get; set; }
        public virtual string DashboardURL { get; set; }
        
        public virtual string SecureVerificationURL { get; set; }

        public virtual string ReturnURL { get; set; }

    }
}
