using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class DiagnosticMap : ClassMap<Diagnostic>
    {
        public DiagnosticMap()
        {
            Table("diagnostics");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.businessName).Column("businessName").Not.Nullable();
            Map(x => x.commercialCAD).Column("commercialCAD").Nullable();
            Map(x => x.plans).Column("plans").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.processId).Column("processId").Nullable();

            References(x => x.account).Column("accountId").Not.Nullable();

            HasMany(x => x.details).Inverse().Cascade.All().KeyColumn("diagnosticId");
            HasMany(x => x.taxStatus).Inverse().Cascade.All().KeyColumn("diagnosticId");
        }
    }
}
