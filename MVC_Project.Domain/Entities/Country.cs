using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Country : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string nameCountry { get; set; }
        public virtual string keyCountry { get; set; }

        public virtual IList<State> states { get; set; }

        public Country()
        {
            states = new List<State>();
        }
    }
}
