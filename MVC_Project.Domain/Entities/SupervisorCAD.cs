using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class SupervisorCAD : IEntity
    {
        public virtual Int64 id { get; set; }

        public virtual User supervisor { get; set; }
        public virtual User cad { get; set; }
    }
}
