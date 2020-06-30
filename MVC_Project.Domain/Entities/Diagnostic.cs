using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Diagnostic : IEntity
    {
        public virtual Int64 id { get; set; }
        //public virtual Guid uuid { get; set; }
        public virtual Account account { get; set; }
        public virtual string businessName { get; set; }
        public virtual string commercialCAD { get; set; }
        public virtual string plan { get; set; } 
        public virtual DateTime createdAt { get; set; }      

        public Diagnostic()
        {
        }
    }
}
