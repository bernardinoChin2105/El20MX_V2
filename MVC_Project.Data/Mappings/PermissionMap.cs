using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class PermissionMap : ClassMap<Permission> {

        public PermissionMap() {
            Table("permissions");
            Id(x => x.Id).GeneratedBy.Identity().Column("PermissionId");            
            Map(x => x.Description).Column("description").Not.Nullable();
            Map(x => x.Controller).Column("controller").Not.Nullable();
            Map(x => x.Action).Column("action").Not.Nullable();
            Map(x => x.Module).Column("module").Nullable();
            Map(x => x.CreatedAt).Column("created_at").Not.Nullable();
            Map(x => x.UpdatedAt).Column("updated_at").Not.Nullable();
            Map(x => x.RemovedAt).Column("removed_at").Nullable();
        }
    }
}