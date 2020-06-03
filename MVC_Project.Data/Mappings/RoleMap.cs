using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class RoleMap : ClassMap<Role> {

        public RoleMap() {
            Table("roles");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.code).Column("code").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.description).Column("description").Not.Nullable();            
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Nullable();
            
            //HasMany(x => x.users).Inverse().Cascade.All().KeyColumn("roleId");
            HasMany(x => x.memberships).Inverse().Cascade.All().KeyColumn("roleId");
            HasManyToMany(x => x.permissions).Cascade.All().Table("rolesPermissions").ParentKeyColumn("roleId").ChildKeyColumn("permissionId");
        }
    }
}