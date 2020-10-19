using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class CustomsRequestNumberMap : ClassMap<CustomsRequestNumber>
    {
        public CustomsRequestNumberMap()
        {
            Table("customsRequestNumber");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.code).Column("code").Not.Nullable();
            Map(x => x.patent).Column("patent").Not.Nullable();
            Map(x => x.practice).Column("practice").Not.Nullable();
            Map(x => x.quantity).Column("quantity").Not.Nullable();
        }
    }
}
