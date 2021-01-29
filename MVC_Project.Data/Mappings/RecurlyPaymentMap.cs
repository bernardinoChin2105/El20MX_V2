using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class RecurlyPaymentMap : ClassMap<RecurlyPayment>
    {
        public RecurlyPaymentMap()
        {
            Table("recurlyPayments");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            //Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.subtotal).Column("subtotal").Nullable();
            Map(x => x.total).Column("total").Nullable();
            Map(x => x.paymentGateway).Column("paymentGateway").Nullable();
            Map(x => x.statusCode).Column("statusCode").Nullable();
            Map(x => x.statusMessage).Column("statusMessage").Nullable();
            Map(x => x.customerMessage).Column("customerMessage").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.transactionAt).Column("transactionAt").Not.Nullable();
            Map(x => x.transactionId).Column("transactionId").Nullable();
            Map(x => x.stampStatus).Column("stampStatus").Nullable();
            Map(x => x.stampAttempt).Column("stampAttempt").Not.Nullable();
            Map(x => x.stampStatusMessage).Column("stampStatusMessage").Length(3000).Nullable();
            Map(x => x.email).Column("email").Nullable();
            Map(x => x.invoiceNumber).Column("invoiceNumber").Nullable();

            References(x => x.subscription).Column("subscriptionId").Nullable();
            References(x => x.invoiceIssued).Column("invoiceIssuedId").Nullable();
            References(x => x.invoiceReceived).Column("invoiceReceivedId").Nullable();

            References(x => x.account).Column("accountId").Nullable();

        }
    }
}
