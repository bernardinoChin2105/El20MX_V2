using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    class AllianceMap : ClassMap<Alliance>
    {
        public AllianceMap()
        {
            Table("alliances");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.name).Column("name").Not.Nullable();
            
            Map(x => x.allyCommisionPercent).Column("allyCommisionPercent").Not.Nullable();
            Map(x => x.customerDiscountPercent).Column("customerDiscountPercent").Not.Nullable();
            Map(x => x.promotionCode).Column("promotionCode").Not.Nullable();

            Map(x => x.initialPeriod).Column("initialPeriod").Nullable();
            Map(x => x.finalPeriod).Column("finalPeriod").Nullable();

            Map(x => x.finalAllyCommisionPercent).Column("finalAllyCommisionPercent").Not.Nullable();

            Map(x => x.initialDate).Column("initialDate").Nullable();
            Map(x => x.finalDate).Column("finalDate").Nullable();

            References(x => x.ally).Column("allyId").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
