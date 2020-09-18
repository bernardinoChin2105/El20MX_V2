using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class PlanFeatureMap : ClassMap<PlanFeature>
    {
        public PlanFeatureMap()
        {
            Table("planFeatures");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.featureType).Column("featureType").Not.Nullable();
            Map(x => x.resourceTable).Column("resourceTable").Nullable();
            Map(x => x.resourceField).Column("resourceField").Nullable();
            Map(x => x.operation).Column("operation").Nullable();

            Map(x => x.dataType).Column("dataType").Nullable();
            Map(x => x.fielType).Column("fielType").Nullable();
            Map(x => x.providerData).Column("providerData").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
