using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProductoMap : ClassMap<Producto>
    {
        public ProductoMap()
        {
            Schema("production");
            Table("products");
            Id(x => x.Id).GeneratedBy.Identity().Column("product_id");
            Map(x => x.Nombre).Column("product_name").Not.Nullable();
            Map(x => x.PrecioLista).Column("list_price").Not.Nullable();
            Map(x => x.Modelo).Column("model_year").Not.Nullable();
        }
    }
}
