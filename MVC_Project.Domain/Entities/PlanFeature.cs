using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class PlanFeature : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string name { get; set; }
        public virtual string featureType { get; set; } //FIJO, VARIABLE
        public virtual string resourceTable { get; set; } //Nombre de la tabla donde se obtiene el valor, valor obligatorio si es variable
        public virtual string resourceField { get; set; } //Campo de la tabla donde se obtiene el valor, valor obligatorio si es variable
        public virtual string operation { get; set; } //Equal, Range, GreaterThan, LessThan 

        public virtual string dataType { get; set; } //string, system.Int32, decimal
        public virtual string fielType { get; set; } //text, select, multiselect
        //public virtual bool isReadonly {get;set;}

        public virtual string providerData { get; set; } //Para multiselects (ColoniaProveedorDatosCombo, MunicipioProveedorDatosCombo)

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
