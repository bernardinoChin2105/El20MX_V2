using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class SupervisorCADMap : ClassMap<SupervisorCAD>
    {
        public SupervisorCADMap()
        {
            Table("supervisorCads");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.supervisor).Column("supervisorId");
            References(x => x.cad).Column("cadId");
        }
    }
}
