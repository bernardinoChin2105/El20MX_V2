using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class OrderItems : IEntity
    {
        public virtual int Id { get; set; }
        public virtual Order Order { get; set; }
        public virtual Producto Producto { get; set; }
        public virtual int Cantidad {get;set;}
        public virtual decimal PrecioLista { get; set; }
        public virtual decimal Descuento { get; set; }
    }
}
