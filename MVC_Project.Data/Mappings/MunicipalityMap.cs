using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class MunicipalityMap : ClassMap<Municipality>
    {
        public MunicipalityMap()
        {
            Table("municipalities");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.nameMunicipality).Column("nameMunicipality").Nullable();
            Map(x => x.keyMunicipality).Column("keyMunicipality").Nullable();
            References(x => x.state).Column("stateId").Nullable();

            //HasMany(x => x.settlements).Inverse().Cascade.All().KeyColumn("settlementId");
        }
    }
}
