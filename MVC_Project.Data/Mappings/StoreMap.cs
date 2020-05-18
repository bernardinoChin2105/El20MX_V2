using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    class StoreMap : ClassMap<Store>
    {
        public StoreMap()
        {
            Schema("sales");
            Table("stores");
            Id(x => x.Id).GeneratedBy.Identity().Column("store_id");
            Map(x => x.Nombre).Column("store_name").Not.Nullable();
            Map(x => x.Email).Column("email").Not.Nullable();
            Map(x => x.Telefono).Column("phone").Not.Nullable();
            Map(x => x.Direccion).Column("street").Nullable();
            Map(x => x.Ciudad).Column("city").Not.Nullable();
            Map(x => x.ZipCode).Column("zip_code").Not.Nullable();
        }
    }
}
