using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class DiagnosticTaxStatusMap : ClassMap<DiagnosticTaxStatus>
    //{
    //    public DiagnosticTaxStatusMap()
    //    {
    //        Table("diagnosticTaxStatus");
    //        Id(x => x.id).GeneratedBy.Identity().Column("id");
    //        References(x => x.diagnostic).Column("diagnosticId").Nullable();
    //        Map(x => x.businessName).Column("businessName").Not.Nullable();
    //        Map(x => x.statusSAT).Column("statusSAT").Not.Nullable();
    //        Map(x => x.taxRegime).Column("taxRegime").Nullable();
    //        Map(x => x.economicActivities).Column("economicActivities").Nullable();
    //        Map(x => x.fiscalObligations).Column("fiscalObligations").Nullable();
    //        Map(x => x.taxMailboxEmail).Column("taxMailboxEmail").Nullable();
    //        Map(x => x.createdAt).Column("createdAt").Not.Nullable();
    //        //Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
    //        //Map(x => x.status).Column("status").Nullable();
    //    }
    //}
}
