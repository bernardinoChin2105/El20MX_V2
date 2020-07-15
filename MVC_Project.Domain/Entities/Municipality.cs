using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Municipality : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual State  state { get; set; }
        public virtual string nameMunicipality { get; set; }
        public virtual string keyMunicipality { get; set; }

        public virtual IList<Settlement> settlements { get; set; }

        public Municipality()
        {
            settlements = new List<Settlement>();
        }
    }
}
