using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    class ProcessMap : ClassMap<Process>
    {
        public ProcessMap()
        {
            Schema("jobs");
            Table("process");
            Id(x => x.Id).GeneratedBy.Identity().Column("processId");
            Map(x => x.Code).Column("code").Not.Nullable();
            Map(x => x.LastExecutionAt).Column("last_execution_at").Nullable();
            Map(x => x.Description).Column("description").Nullable();
            Map(x => x.Status).Column("status").Nullable();
            Map(x => x.Running).Column("running").Nullable();
            Map(x => x.Result).Column("result").Nullable();
        }
    }
}
