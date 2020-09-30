using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Promotion : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string name { get; set; }

        public virtual string type { get; set; }

        public virtual decimal discount { get; set; }
        public virtual decimal discountRate { get; set; }

        public virtual bool hasPeriod { get; set; }
        public virtual int periodInitial { get; set; }
        public virtual int periodFinal { get; set; }

        public virtual bool hasValidity { get; set; }
        public virtual DateTime validityInitialAt { get; set; }
        public virtual DateTime validityFinalAt { get; set; }
        
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
