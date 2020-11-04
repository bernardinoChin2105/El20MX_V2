using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class CADAccount : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual Account account { get; set; }
        public virtual User user { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
