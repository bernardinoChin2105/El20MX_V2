using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class State : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Country country { get; set; }
        public virtual string nameState { get; set; }
        public virtual string keyState { get; set; }

        public virtual IList<Municipality> municipalities { get; set; }

        public State()
        {
            municipalities = new List<Municipality>();
        }
    }
}
