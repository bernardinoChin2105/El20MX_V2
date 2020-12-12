using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class RecurlyInvoiceMap : ClassMap<RecurlyInvoice>
    {
        public RecurlyInvoiceMap()
        {
            Table("recurlyInvoices");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.mounth).Column("mounth").Nullable();
            Map(x => x.year).Column("year").Nullable();
            Map(x => x.totalInvoice).Column("totalInvoice").Nullable();

            Map(x => x.totalInvoiceReceived).Column("totalInvoiceReceived").Nullable();
            Map(x => x.totalInvoiceIssued).Column("totalInvoiceIssued").Nullable();
            Map(x => x.extraBills).Column("extraBills").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            //Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            //Map(x => x.status).Column("status").Nullable();
            References(x => x.subscription).Column("subscriptionId").Nullable();
        }
    }
}
