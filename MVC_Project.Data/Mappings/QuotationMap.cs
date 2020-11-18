using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class QuotationMap : ClassMap<Quotation>
    {
        public QuotationMap()
        {
            Table("quotations");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.startedAt).Column("startedAt").Not.Nullable();
            Map(x => x.hiringDate).Column("hiringDate").Nullable();
            Map(x => x.total).Column("total").Not.Nullable();

            Map(x => x.hasDeferredPayment).Column("hasDeferredPayment").Nullable();
            Map(x => x.partialitiesNumber).Column("partialitiesNumber").Nullable();
            Map(x => x.advancePayment).Column("advancePayment").Nullable();
            Map(x => x.monthlyCharge).Column("monthlyCharge").Nullable();
            
            References(x => x.account).Column("accountId");

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            HasMany(x => x.quotationDetails).Inverse().Cascade.All().KeyColumn("quotationId");
        }
    }
}
