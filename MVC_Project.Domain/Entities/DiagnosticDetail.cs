using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class DiagnosticDetail : IEntity
    {
        public virtual Int64 id { get; set; }
        //public virtual Guid uuid { get; set; }
        public virtual Diagnostic diagnostic { get; set; }
        public virtual int year { get; set; }
        public virtual int month { get; set; }
        public virtual string typeTaxPayer { get; set; }
        public virtual int numberCFDI { get; set; }
        public virtual decimal totalAmount { get; set; }
        public virtual DateTime createdAt { get; set; }


        public DiagnosticDetail()
        {
        }
    }
}
