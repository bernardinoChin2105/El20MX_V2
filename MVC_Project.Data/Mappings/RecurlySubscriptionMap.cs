using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class RecurlySubscriptionMap : ClassMap<RecurlySubscription>
    {
        public RecurlySubscriptionMap()
        {
            Table("recurlySubscription");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.subscriptionId).Column("subscriptionId").Not.Nullable();
            Map(x => x.planId).Column("planId").Nullable();
            Map(x => x.planCode).Column("planCode").Nullable();
            Map(x => x.planName).Column("planName").Nullable();
            Map(x => x.state).Column("state").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Nullable();
        }
    }
}
