using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class UserMap : ClassMap<User> {

        public UserMap() {
            Table("users");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.password).Column("password").Not.Nullable();
            Map(x => x.passwordExpiration).Column("passwordExpiration").Nullable();
            Map(x => x.token).Column("token").Nullable();
            Map(x => x.tokenExpiration).Column("tokenExpiration").Nullable();
            Map(x => x.lastLoginAt).Column("lastLoginAt").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            References(x => x.profile).Column("profileId");
            //References(x => x.role).Column("roleId");

            HasMany(x => x.accountUsers).Inverse().Cascade.All().KeyColumn("userId");

            HasManyToMany(x => x.permissions)
                .Cascade.SaveUpdate()
                .Table("usersPermissions")
                .ParentKeyColumn("userId")
                .ChildKeyColumn("permissionId");

            //HasManyToMany(x => x.accounts)
            //   .Cascade.SaveUpdate()
            //   .Table("accountsUsers")
            //   .ParentKeyColumn("userId")
            //   .ChildKeyColumn("accountId");
        }
    }
}