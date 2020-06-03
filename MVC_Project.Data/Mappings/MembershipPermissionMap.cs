using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class MembershipPermissionMap : ClassMap<MembershipPermission>
    {
        public MembershipPermissionMap()
        {
            Table("membershipsPermissions");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.membership).Column("membershipId");
            References(x => x.permission).Column("permissionId");
            References(x => x.account).Column("accountId");
        }
    }
}
