using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class MembershipMap : ClassMap<Membership>
    {
        public MembershipMap()
        {
            Table("memberships");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.account).Column("accountId").Nullable();
            References(x => x.user).Column("userId");
            References(x => x.role).Column("roleId");
            Map(x => x.assignedBy).Column("assignedBy").Nullable();
            Map(x => x.acceptUser).Column("acceptUser").Nullable();
            Map(x => x.status).Column("status").Nullable();

            HasMany(x => x.mebershipPermissions).Inverse().Cascade.All().KeyColumn("membershipId");

            //HasManyToMany(x => x.permissions)
            //    .Cascade.SaveUpdate()
            //    .Table("membershipsPermissions")
            //    .ParentKeyColumn("membershipId")
            //    .ChildKeyColumn("permissionId");
        }
    }
}
