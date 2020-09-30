using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Alliance : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string name { get; set; }

        public virtual Ally ally { get; set; }
        public virtual decimal allyCommisionPercent { get; set; }
        public virtual decimal customerDiscountPercent { get; set; }
        public virtual string promotionCode { get; set; }
        public virtual bool allianceValidity { get; set; }

        public virtual bool applyPeriod { get; set; }
        public virtual int initialPeriod { get; set; }
        public virtual int finalPeriod { get; set; }

        public virtual decimal finalAllyCommisionPercent { get; set; }

        public virtual DateTime initialDate { get; set; }
        public virtual DateTime finalDate { get; set; }
        
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
