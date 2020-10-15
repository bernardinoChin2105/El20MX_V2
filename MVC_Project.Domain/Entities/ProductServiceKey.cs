using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class ProductServiceKey : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string code { get; set; }
        public virtual string description { get; set; }

        public virtual string includeIVATransferred { get; set; }
        public virtual string includeIEPSTranslated { get; set; }       
        public virtual int borderStripStimulus { get; set; }
        public virtual string similarWords { get; set; }
    }
}
