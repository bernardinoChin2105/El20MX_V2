using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class MembershipPermission : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Membership membership { get; set; }
        public virtual Permission permission { get; set; }
        public virtual Account account { get; set; }
        public virtual string level { get; set; }
    }
}
