using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class CustomsPatent : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string code { get; set; }
        public virtual string dateInitial { get; set; }
    }
}
