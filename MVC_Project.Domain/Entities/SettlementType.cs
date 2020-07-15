using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class SettlementType : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string nameSettlementType { get; set; }
        public virtual string keySettlementType { get; set; }        
    }
}
