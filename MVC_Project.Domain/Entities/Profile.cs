using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Profile : IEntity
    {
        public virtual int id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string firstName { get; set; }
        public virtual string lastName { get; set; }
        public virtual string email { get; set; }
        public virtual string phoneNumber { get; set; }
        public virtual string language { get; set; }
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }

        public Profile()
        {

        }
    }
}
