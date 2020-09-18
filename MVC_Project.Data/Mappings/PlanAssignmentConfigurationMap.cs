using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class PlanAssignmentConfigurationMap : ClassMap<PlanAssignmentConfiguration>
    {
        public PlanAssignmentConfigurationMap()
        {
            Table("planAssignmentConfigurations");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            References(x => x.plan).Column("planId");
            References(x => x.planAssignment).Column("planAssignmentId");

            Map(x => x.value1).Column("value1").Nullable();
            Map(x => x.value2).Column("value2").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
