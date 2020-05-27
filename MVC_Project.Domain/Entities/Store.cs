using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Store :IEntity
    {
        public virtual int id { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Email { get; set; }
        public virtual string Telefono { get; set; }
        public virtual string Direccion { get; set; }
        public virtual string Ciudad { get; set; }
        public virtual string ZipCode { get; set; }
    }
}
