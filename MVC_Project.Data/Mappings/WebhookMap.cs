using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class WebhookMap : ClassMap<Webhook>
    {
        public WebhookMap()
        {
            Table("webhooks");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.provider).Column("provider").Not.Nullable();
            Map(x => x.eventWebhook).Column("eventWebhook").Nullable();
            Map(x => x.endpoint).Column("endpoint").Nullable();
            Map(x => x.response).Column("response").Length(2500).Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
        }
    }
}
