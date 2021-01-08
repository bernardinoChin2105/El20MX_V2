using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class WebhookProcessMap:ClassMap<WebhookProcess>
    {
        public WebhookProcessMap()
        {
            Table("webhookProcesses");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.processId).Column("processId").Nullable();
            Map(x => x.provider).Column("provider").Nullable();
            Map(x => x.@event).Column("event").Nullable();
            Map(x => x.reference).Column("reference").Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.content).Column("content").CustomSqlType("varchar(max)").Nullable();
            Map(x => x.result).Column("result").CustomSqlType("varchar(max)").Nullable();
            Map(x => x.attempt).Column("attempt").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
        }
    }
}
