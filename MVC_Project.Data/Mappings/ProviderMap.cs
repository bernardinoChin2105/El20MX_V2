using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProviderMap : ClassMap<Provider>
    {
        public ProviderMap()
        {
            //Schema("sales");
            Table("providers");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.firstName).Column("first_name").Nullable();
            Map(x => x.lastName).Column("last_name").Nullable();
            Map(x => x.rfc).Column("rfc").Not.Nullable();
            Map(x => x.curp).Column("curp").Nullable();
            Map(x => x.taxRegime).Column("taxRegime").Nullable();
            Map(x => x.businessName).Column("businessName").Nullable();
            Map(x => x.street).Column("street").Nullable();
            Map(x => x.interiorNumber).Column("interiorNumber").Nullable();
            Map(x => x.outdoorNumber).Column("outdoorNumber").Nullable();
            Map(x => x.colony).Column("colony").Nullable();
            Map(x => x.zipCode).Column("zipCode").Nullable();
            Map(x => x.municipality).Column("municipality").Nullable();
            Map(x => x.state).Column("state").Nullable();
            Map(x => x.deliveryAddress).Column("deliveryAddress").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.country).Column("country").Nullable();

            References(x => x.account).Column("accountId").Not.Nullable();
            HasMany(x => x.providerContacts).Inverse().Cascade.All().KeyColumn("providerId");
        }
    }
}

