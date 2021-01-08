using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class SATExtractionProcessMap : ClassMap<SATExtractionProcess>
    {
        public SATExtractionProcessMap()
        {
            Table("satExtractionsProcess");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.processId).Column("processId").Nullable();
            Map(x => x.provider).Column("provider").Nullable();
            Map(x => x.@event).Column("event").Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.result).Column("result").CustomSqlType("varchar(max)").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.isHistorical).Column("isHistorical").Nullable();
            References(x => x.account).Column("accountId").Nullable();
        }
    }
}
