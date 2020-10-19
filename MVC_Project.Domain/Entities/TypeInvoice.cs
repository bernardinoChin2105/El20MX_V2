using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class TypeInvoice : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string description { get; set; }
    }
}
