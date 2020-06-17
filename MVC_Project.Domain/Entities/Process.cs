using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Process : IEntity
    {
        public virtual Int64 id { get; set; }

        public virtual DateTime? LastExecutionAt { get; set; }

        public virtual string Code { get; set; }

        public virtual string Description { get; set; }

        public virtual bool Status { get; set; }

        public virtual bool Running { get; set; }

        public virtual string Result { get; set; }

    }
}
