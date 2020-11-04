using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class RateFeeMap : ClassMap<RateFee>
    {
        public RateFeeMap()
        {
            //Schema("sales");
            Table("ratesFees");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.type).Column("type").Nullable();
            Map(x => x.maximumValue).Column("maximumValue").Nullable();
            Map(x => x.taxes).Column("taxes").Not.Nullable();
            Map(x => x.factor).Column("factor").Nullable();
            Map(x => x.transfer).Column("transfer").Nullable();
            Map(x => x.retention).Column("retention").Nullable();        
        }
    }
}

