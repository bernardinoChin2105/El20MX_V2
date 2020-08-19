using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProfileMap : ClassMap<Profile>
    {
        public ProfileMap()
        {
            Table("profiles");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.firstName).Column("firstName").Not.Nullable();
            Map(x => x.lastName).Column("lastName").Not.Nullable();
            Map(x => x.email).Column("email").Nullable();
            Map(x => x.phoneNumber).Column("phoneNumber").Nullable();
            Map(x => x.language).Column("language").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.avatar).Column("avatar").Nullable();
        }
    }
}
