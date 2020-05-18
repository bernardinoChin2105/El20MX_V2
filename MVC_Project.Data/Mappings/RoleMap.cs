using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class RoleMap : ClassMap<Role> {

        public RoleMap() {
            Table("roles");
            Id(x => x.Id).GeneratedBy.Identity().Column("RoleId");
            Map(x => x.Code).Column("code").Not.Nullable();
            Map(x => x.Name).Column("name").Not.Nullable();
            Map(x => x.Uuid).Column("uuid").Not.Nullable();
            Map(x => x.Description).Column("description").Not.Nullable();            
            Map(x => x.CreatedAt).Column("created_at").Not.Nullable();
            Map(x => x.UpdatedAt).Column("updated_at").Not.Nullable();
            Map(x => x.RemovedAt).Column("removed_at").Nullable();
            Map(x => x.Status).Column("status").Nullable();
            HasMany(x => x.Users).Inverse().Cascade.All().KeyColumn("RoleId");
            HasManyToMany(x => x.Permissions).Cascade.All().Table("permission_role").ParentKeyColumn("RoleId").ChildKeyColumn("PermissionId");
        }
    }
}