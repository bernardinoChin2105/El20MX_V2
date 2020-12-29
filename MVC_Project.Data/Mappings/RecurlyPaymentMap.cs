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
            Map(x => x.paymentGateway).Column("paymentGateway").Nullable();
            Map(x => x.statusCode).Column("statusCode").Nullable();
            Map(x => x.statusMessage).Column("statusMessage").Nullable();
            Map(x => x.customerMessage).Column("customerMessage").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.transactionId).Column("transactionId").Nullable();
            //Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            //Map(x => x.status).Column("status").Nullable();

            References(x => x.subscription).Column("subscriptionId").Nullable();

        }
    }
}
