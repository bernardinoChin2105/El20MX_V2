using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Producto: IEntity
    {
        public virtual int id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual int Modelo { get; set; }
        public virtual decimal PrecioLista { get; set; }
    }
}
