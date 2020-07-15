using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class StateMap : ClassMap<State>
    {
        public StateMap()
        {
            Table("states");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.nameState).Column("nameState").Not.Nullable();
            Map(x => x.keyState).Column("last_name").Not.Nullable();

            HasMany(x => x.municipalities).Inverse().Cascade.All().KeyColumn("stateId");
        }
    }
}
