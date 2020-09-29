using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class PromotionMap:ClassMap<Promotion>
    {
        public PromotionMap()
        {
            Table("promotions");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();

            Map(x => x.type).Column("type").Not.Nullable();

            Map(x => x.discount).Column("discount").Nullable();
            Map(x => x.discountRate).Column("discountRate").Nullable();

            Map(x => x.hasPeriod).Column("hasPeriod").Not.Nullable();
            Map(x => x.periodInitial).Column("periodInitial").Nullable();
            Map(x => x.periodFinal).Column("periodFinal").Nullable();

            Map(x => x.hasValidity).Column("hasValidity").Not.Nullable();
            Map(x => x.validityInitialAt).Column("validityInitialAt").Nullable();
            Map(x => x.validityFinalAt).Column("validityFinalAt").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.status).Column("status").Nullable();
            
        }
    }
}
