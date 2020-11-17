using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class RateFee : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string type { get; set; }
        public virtual double maximumValue { get; set; }
        public virtual string taxes { get; set; }
        public virtual string factor { get; set; }
        public virtual bool transfer { get; set; }
        public virtual bool retention { get; set; }
    }
}
