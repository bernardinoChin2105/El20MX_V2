using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class OrderItemsMap : ClassMap<OrderItems>
    //{
    //    public OrderItemsMap()
    //    {
    //        Schema("sales");
    //        Table("order_items");
    //        Id(x => x.id).GeneratedBy.Identity().Column("item_id");
    //        References(x => x.Order).Column("order_id");
    //        References(x => x.Producto).Column("product_id");
    //        Map(x => x.Cantidad).Column("quantity").Not.Nullable();
    //        Map(x => x.PrecioLista).Column("list_price").Not.Nullable();
    //        Map(x => x.Descuento).Column("discount").Not.Nullable();
    //    } 
    //}
}
