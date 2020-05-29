using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class SocialNetworkLoginMap : ClassMap<SocialNetworkLogin>
    {
        public SocialNetworkLoginMap()
        {
            Table("socialNetworkLogins");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.socialNetwork).Column("socialNetwork").Nullable();
            Map(x => x.token).Column("token").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            References(x => x.user).Column("userId");
        }
    }
}
