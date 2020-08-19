using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class SettlementMap : ClassMap<Settlement>
    {
        public SettlementMap()
        {
            Table("settlements");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.municipality).Column("municipalityId").Nullable();
            References(x => x.SettlementType).Column("settlementTypeId").Nullable();
            Map(x => x.nameSettlement).Column("nameSettlement").Nullable();
            Map(x => x.keySettlement).Column("keySettlement").Nullable();
            Map(x => x.zipCode).Column("zipCode").Nullable();
        }
    }
}
