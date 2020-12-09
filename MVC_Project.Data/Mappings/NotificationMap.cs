using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class NotificationMap : ClassMap<Notification>
    {
        public NotificationMap()
        {
            Table("notifications");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.message).Column("message").Not.Nullable();
            
            References(x => x.account).Column("accountId").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
