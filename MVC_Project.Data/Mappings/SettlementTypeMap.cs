using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class SettlementTypeMap : ClassMap<SettlementType>
    {
        public SettlementTypeMap()
        {
            Table("settlementTypes");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.nameSettlementType).Column("nameSettlementType").Nullable();
            Map(x => x.keySettlementType).Column("keySettlementType").Nullable();
        }
    }
}
