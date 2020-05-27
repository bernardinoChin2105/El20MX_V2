using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class PaymentMap : ClassMap<Payment>
    //{
    //    public PaymentMap()
    //    {
    //        Schema("dbo");
    //        Table("payments");
    //        Id(x => x.id).GeneratedBy.Identity().Column("PaymentId");
    //        Map(x => x.CreationDate).Column("creation_date").Not.Nullable();
    //        Map(x => x.OrderId).Column("order_id").Nullable();
    //        Map(x => x.Method).Column("method").Nullable();
    //        Map(x => x.TransactionType).Column("transaction_type").Nullable();
    //        Map(x => x.Status).Column("status").Nullable();
    //        Map(x => x.Amount).Column("amount").Nullable();
    //        Map(x => x.ConfirmationDate).Column("confirmation_date").Nullable();
    //        Map(x => x.ProviderId).Column("providerId").Nullable();
    //        Map(x => x.AuthorizationCode).Column("authorization_code").Nullable();
    //        Map(x => x.DueDate).Column("due_date").Nullable();
    //        Map(x => x.LogData).Column("log_data").Nullable();
    //        Map(x => x.ConfirmationEmail).Column("confirmation_email").Nullable();
    //        References(x => x.User).Column("UserId");
    //    }
    //}
}
