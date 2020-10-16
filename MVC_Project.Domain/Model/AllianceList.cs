using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class AllianceList
    {        
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public decimal allyCommisionPercent { get; set; }
        public decimal customerDiscountPercent { get; set; }
        public string promotionCode { get; set; }
        public int initialPeriod { get; set; }
        public int finalPeriod { get; set; }
        public decimal finalAllyCommisionPercent { get; set; }    
        public DateTime initialDate { get; set; }
        public DateTime finalDate { get; set; }        
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public Int64 allyId { get; set; }
        public string allyName { get; set; }
        public string AllyStatus { get; set; }
        public Int32 Total { get; set; }      
    }

    public class AlliesList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public Int32 Total { get; set; }
    }

    public class PromotionsList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public decimal discount { get; set; }
        public decimal discountRate { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public Int32 Total { get; set; }
    }
}
