using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class OrderMap : ClassMap<Order>
    //{
    //    public OrderMap()
    //    {
    //        Schema("sales");
    //        Table("Orders");
    //        Id(x => x.id).GeneratedBy.Identity().Column("order_id");
    //        References(x => x.Customer).Column("customer_id");
    //        References(x => x.Store).Column("store_id");
    //        References(x => x.Staff).Column("staff_id");
    //        Map(x => x.OrderStatus).Column("order_status").Not.Nullable();
    //        Map(x => x.CreatedAt).Column("order_date").Not.Nullable();
    //        Map(x => x.RequiredAt).Column("required_date").Not.Nullable();
    //        Map(x => x.ShippedAt).Column("shipped_date").Not.Nullable();
    //    }
    //}
}
