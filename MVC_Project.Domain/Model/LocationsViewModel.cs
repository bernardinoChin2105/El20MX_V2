using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class LocationsViewModel
    {
        public Int64 id { get; set; }
        public Int64 municipalityId { get; set; }
        public Int64 settlementTypeId { get; set; }
        public Int64 stateId { get; set; }
        public string nameSettlement { get; set; }
        public string zipCode { get; set; }
        public string nameSettlementType { get; set; }
        public string nameMunicipality { get; set; }
        public string nameState { get; set; }
    }
}
