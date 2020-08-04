using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class CountryMap : ClassMap<Country>
    {
        public CountryMap()
        {
            Table("countries");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.nameCountry).Column("nameCountry").Not.Nullable();
            Map(x => x.keyCountry).Column("keyCountry").Not.Nullable();

            HasMany(x => x.states).Inverse().Cascade.All().KeyColumn("countryId");
        }
    }
}
