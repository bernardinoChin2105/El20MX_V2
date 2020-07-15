using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class CustomerContact: IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Customer customer { get; set; }
        public virtual string typeContact { get; set; }
        public virtual string emailOrPhone { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
