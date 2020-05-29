using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class AccountUserMap : ClassMap<AccountUser>
    {
        public AccountUserMap()
        {
            Table("accountsUsers");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.account).Column("accountId").Nullable();
            References(x => x.user).Column("userId");
            References(x => x.role).Column("roleId");
        }
    }
}
