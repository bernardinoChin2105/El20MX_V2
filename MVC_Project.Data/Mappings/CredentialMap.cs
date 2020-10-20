using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class CredentialMap : ClassMap<Credential>
    {
        public CredentialMap()
        {
            Table("credentials");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            References(x => x.account).Column("accountId").Nullable();
            Map(x => x.provider).Column("provider").Not.Nullable();
            Map(x => x.idCredentialProvider).Column("idCredentialProvider").Not.Nullable();
            Map(x => x.statusProvider).Column("statusProvider").Nullable();
            Map(x => x.credentialType).Column("credentialType").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();            
        }
    }
}
