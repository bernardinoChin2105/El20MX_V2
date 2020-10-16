using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class PromotionAccount : IEntity
    {
        public virtual Int64 id { get; set; }
        
        public virtual Account account { get; set; }

        public virtual Promotion promotion { get; set; }
        
    }
}
