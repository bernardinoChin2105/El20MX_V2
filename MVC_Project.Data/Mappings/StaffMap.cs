using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class StaffMap : ClassMap<Staff>
    //{
    //    public StaffMap()
    //    {
    //        Schema("sales");
    //        Table("staffs");
    //        Id(x => x.id).GeneratedBy.Identity().Column("staff_id");
    //        Map(x => x.FirstName).Column("first_name").Not.Nullable();
    //        Map(x => x.LastName).Column("last_name").Nullable();
    //        Map(x => x.Email).Column("email").Not.Nullable();
    //        Map(x => x.Telefono).Column("phone").Not.Nullable();
    //        References(x => x.Store).Column("store_id");
    //    }  
    //}
}
