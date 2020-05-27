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
            References(x => x.Role).Column("roleId");
            References(x => x.Permission).Column("permissionId");
        }
    }
}
