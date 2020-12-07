using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class ProcessExecution
    {
        public virtual Int32 Id { get; set; }

        public virtual Process Process { get; set; }

        public virtual DateTime? StartAt { get; set; }

        public virtual DateTime? EndAt { get; set; }
        
        public virtual bool Status { get; set; }

        public virtual bool Success { get; set; }

        public virtual string Result { get; set; }
    }
}
