using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ConfigParamMap : ClassMap<ConfigParam>
    {
        public ConfigParamMap()
        {
            Schema("dbo");
            Table("config_params");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name).Not.Nullable();
            Map(x => x.Value).Not.Nullable();
            Map(x => x.Description).Nullable();
        }
    }
}
