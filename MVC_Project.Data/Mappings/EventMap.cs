using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class EventMap : ClassMap<Event>
    {
        public EventMap()
        {
            Schema("calendar");
            Table("events");
            Id(x => x.Id).GeneratedBy.Identity().Column("EventId");
            Map(x => x.CreationDate).Column("creation_date").Not.Nullable();
            Map(x => x.Uuid).Column("uuid").Nullable();
            Map(x => x.Title).Column("title");
            Map(x => x.Description).Column("description").Nullable();
            Map(x => x.StartDate).Column("startdate").Nullable();
            Map(x => x.EndDate).Column("enddate").Nullable();
            Map(x => x.IsFullDay).Column("isFullDay").Nullable();
            References(x => x.User).Column("userId");
        }
    }
}
