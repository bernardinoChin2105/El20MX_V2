using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class ConfigParam : IEntity
    {
        public virtual int id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }
    }
    
}
