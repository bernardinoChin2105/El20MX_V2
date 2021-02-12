using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class InvoiceIssuedMap : ClassMap<InvoiceIssued>
    {
        public InvoiceIssuedMap()
        {
            Table("invoicesIssued");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.folio).Column("folio").Nullable();
            Map(x => x.serie).Column("serie").Nullable();
            Map(x => x.paymentMethod).Column("paymentMethod").Nullable();
            Map(x => x.paymentForm).Column("paymentForm").Nullable();
            Map(x => x.invoiceType).Column("invoiceType").Nullable();
            Map(x => x.currency).Column("currency").Nullable();
            Map(x => x.total).Column("total").Nullable();
            Map(x => x.subtotal).Column("subtotal").Nullable();
            //Map(x => x.amount).Column("amount").Nullable();
            Map(x => x.iva).Column("iva").Nullable();
            //Map(x => x.totalAmount).Column("totalAmount").Nullable();
            Map(x => x.invoicedAt).Column("invoicedAt").Nullable();
            Map(x => x.xml).Column("xml").Length(8000).Nullable();
            //Map(x => x.xml).Column("xml").Nullable().CustomSqlType("nvarchar(max)");
            //Map(x => x.xml).CustomType("StringClob").CustomSqlType("nvarchar(max)");
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            Map(x => x.homemade).Column("homemade").Nullable();
            //Map(x => x.json).Column("json").Nullable();
            Map(x => x.json).Column("json").Nullable().CustomSqlType("nvarchar(max)");
            References(x => x.branchOffice).Column("branchOfficeId").Nullable();
            Map(x => x.commentsPDF).Column("commentsPDF").Length(8000).Nullable();
            Map(x => x.pdf).Column("pdf").Length(8000).Nullable();

            Map(x => x.loadStatus).Column("loadStatus").Nullable();
            Map(x => x.loadResponse).Column("loadResult").Length(8000).Nullable();

            References(x => x.account).Column("accountId").Nullable();
            References(x => x.customer).Column("customerId").Nullable();
        }
    }
}

