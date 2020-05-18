using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProcessExecutionMap : ClassMap<ProcessExecution>
    {
        public ProcessExecutionMap()
        {
            Schema("jobs");
            Table("process_execution");
            Id(x => x.Id).GeneratedBy.Identity().Column("processExecutionId");
            References(x => x.Process).Column("processId");
            Map(x => x.StartAt).Column("start_at").Nullable();
            Map(x => x.EndAt).Column("end_at").Nullable();
            Map(x => x.Status).Column("status").Nullable();
            Map(x => x.Success).Column("success").Nullable();
            Map(x => x.Result).Column("result").Nullable();
        }
    }
}
