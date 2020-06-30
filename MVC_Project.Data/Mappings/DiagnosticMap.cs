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
            Map(x => x.businessName).Column("businessName").Not.Nullable();
            Map(x => x.commercialCAD).Column("commercialCAD").Nullable();
            Map(x => x.plans).Column("plans").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            References(x => x.account).Column("accountId").Not.Nullable();
        }
    }
}
