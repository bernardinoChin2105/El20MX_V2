using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    //public class PaymentApplicationMap : ClassMap<PaymentApplication>
    //{
    //    public PaymentApplicationMap()
    //    {
    //        Schema("dbo");
    //        Table("payment_applications");
    //        Id(x => x.id).GeneratedBy.Identity().Column("PaymentApplicationId");
    //        Map(x => x.AppKey).Column("app_key").Not.Nullable();
    //        Map(x => x.Name).Column("name").Nullable();
    //        Map(x => x.Active).Column("active").Nullable();
    //        References(x => x.User).Column("UserId");

    //        Map(x => x.PublicKey).Column("PublicKey").Nullable();
    //        Map(x => x.PrivateKey).Column("PrivateKey").Nullable();
    //        Map(x => x.MerchantId).Column("MerchantId").Nullable();
    //        Map(x => x.DashboardURL).Column("DashboardURL").Nullable();
    //        Map(x => x.ClientId).Column("ClientId").Nullable();
    //        Map(x => x.SecureVerificationURL).Column("SecureVerificationURL").Nullable();

    //        Map(x => x.ReturnURL).Column("ReturnURL").Nullable();
    //    }
    //}
}
