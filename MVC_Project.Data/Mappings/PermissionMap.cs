using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class PermissionMap : ClassMap<Permission> {

        public PermissionMap() {
            Table("permissions");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.description).Column("description").Not.Nullable();
            Map(x => x.controller).Column("controller").Not.Nullable();
            Map(x => x.action).Column("action").Not.Nullable();
            Map(x => x.module).Column("module").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Nullable();
            References(x => x.feature).Column("featureId").Nullable();

            HasMany(x => x.mebershipPermissions).Inverse().Cascade.All().KeyColumn("permissionId");
        }
    }
}