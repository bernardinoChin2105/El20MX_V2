using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class InvoiceReceivedMap : ClassMap<InvoiceReceived>
    {
        public InvoiceReceivedMap()
        {
            //Schema("sales");
            Table("invoicesReceived");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.folio).Column("folio").Nullable();
            Map(x => x.serie).Column("serie").Nullable();
            Map(x => x.paymentMethod).Column("paymentMethod").Nullable();
            Map(x => x.paymentForm).Column("paymentForm").Nullable();
            Map(x => x.currency).Column("currency").Nullable();
            Map(x => x.amount).Column("amount").Nullable();
            Map(x => x.iva).Column("iva").Nullable();
            Map(x => x.totalAmount).Column("totalAmount").Nullable();
            Map(x => x.invoicedAt).Column("invoicedAt").Nullable();
            //Map(x => x.xml).Column("xml").Length(8000).Nullable();
            Map(x => x.xml).Column("xml").Nullable().CustomSqlType("nvarchar(max)");
            //Map(x => x.xml).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Not.Nullable();
            References(x => x.provider).Column("providerId").Not.Nullable();
        }
    }
}

