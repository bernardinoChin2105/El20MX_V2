using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Settlement: IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Municipality municipality { get; set; }
        public virtual SettlementType SettlementType { get; set; }
        public virtual string nameSettlement { get; set; }
        public virtual string keySettlement { get; set; }
        public virtual string zipCode { get; set; }
    }
}
