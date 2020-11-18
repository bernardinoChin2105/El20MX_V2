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
        public virtual Guid uuid { get; set; }
        public virtual Account account { get; set; }
        public virtual string businessName { get; set; }
        public virtual string commercialCAD { get; set; }
        public virtual string plans { get; set; } 
        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
        public virtual string processId { get; set; }

        public virtual IList<DiagnosticDetail> details { get; set; }
        public virtual IList<DiagnosticTaxStatus> taxStatus { get; set; }

        public Diagnostic()
        {
            details = new List<DiagnosticDetail>();
            taxStatus = new List<DiagnosticTaxStatus>();
        }
    }
}
