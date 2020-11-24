using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class BankCredentialMap : ClassMap<BankCredential>
    {
        public BankCredentialMap()
        {
            Table("bankCredentials");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.credentialProviderId).Column("credentialProviderId").Not.Nullable();

            Map(x => x.dateTimeAuthorized).Column("dateTimeAuthorized").Nullable();
            Map(x => x.dateTimeRefresh).Column("dateTimeRefresh").Nullable();
            Map(x => x.isTwofa).Column("isTwofa").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Nullable();
            References(x => x.bank).Column("banckId").Nullable();
            HasMany(x => x.bankAccount).Inverse().Cascade.All().KeyColumn("bankCredentialId");
        }
    }
}
