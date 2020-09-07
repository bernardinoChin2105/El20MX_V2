using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class TypeVoucherMap : ClassMap<TypeVoucher>
    {
        public TypeVoucherMap()
        {
            Table("typeVouchers");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.code).Column("code").Not.Nullable();
            Map(x => x.Description).Column("Description").Not.Nullable();            
        }
    }
}