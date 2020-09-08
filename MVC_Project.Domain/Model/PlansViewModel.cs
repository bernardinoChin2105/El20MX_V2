using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class PlansViewModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }    
        public string name { get; set; }
        //public bool isCurrent { get; set; }    
        //public DateTime createdAt { get; set; }
        //public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public int Total { get; set; }        
    }
}
