using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class DiagnosticsList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string businessName { get; set; }
        public string rfc { get; set; }
        public string plans { get; set; }
        public DateTime createdAt { get; set; }
        public Int32 Total { get; set; }
    }
}
