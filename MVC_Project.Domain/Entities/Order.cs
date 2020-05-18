using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Order : IEntity
    {
        public virtual int Id { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Store Store { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual int OrderStatus { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime RequiredAt { get; set; }
        public virtual DateTime ShippedAt { get; set; }
    }
}
