using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProviderContactMap : ClassMap<ProviderContact>
    {
        public ProviderContactMap()
        {
            //Schema("sales");
            Table("providersContacts");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.provider).Column("providerId").Not.Nullable();
            Map(x => x.typeContact).Column("typeContact").Nullable();
            Map(x => x.emailOrPhone).Column("emailOrPhone").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}

