using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class DiagnosticDetailMap : ClassMap<DiagnosticDetail>
    {
        public DiagnosticDetailMap()
        {
            Table("diagnosticDetails");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.diagnostic).Column("diagnosticId").Nullable();
            Map(x => x.year).Column("year").Not.Nullable();
            Map(x => x.month).Column("month").Not.Nullable();
            Map(x => x.typeTaxPayer).Column("typeTaxPayer").Nullable();
            Map(x => x.numberCFDI).Column("numberCFDI").Nullable();
            Map(x => x.totalAmount).Column("totalAmount").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            //Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            //Map(x => x.status).Column("status").Nullable();
        }
    }
}
