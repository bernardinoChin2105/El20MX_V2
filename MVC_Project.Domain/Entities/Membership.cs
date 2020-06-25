using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Membership : IEntity
    {
        public virtual Int64 id { get; set; }
        //public virtual Guid uuid { get; set; }
        public virtual Account account { get; set; }
        public virtual User user { get; set; }
        public virtual Role role { get; set; }
        public virtual Int64? assignedBy { get; set; }
        public virtual string acceptUser { get; set; }

        public virtual IList<MembershipPermission> mebershipPermissions { get; set; }

        public Membership()
        {
            mebershipPermissions = new List<MembershipPermission>();
        }
    }
}
