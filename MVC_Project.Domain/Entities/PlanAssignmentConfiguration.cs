using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class PlanAssignmentConfiguration : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual Plan plan { get; set; }
        public virtual PlanAssignment planAssignment { get; set; }

        public virtual string value1 { get; set; }
        public virtual string value2 { get; set; } //valor2 para rangos

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
