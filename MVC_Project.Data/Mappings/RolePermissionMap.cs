using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class RolePermissionMap : ClassMap<RolePermission>
    {
        public RolePermissionMap()
        {
            Table("rolesPermissions");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.role).Column("roleId");
            References(x => x.permission).Column("permissionId");
            Map(x => x.level).Column("level").Nullable();
            References(x => x.account).Column("accountId").Nullable();
        }
    }
}
