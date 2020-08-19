﻿using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class BankAccountMap : ClassMap<BankAccount>
    {
        public BankAccountMap()
        {
            Table("bankAccounts");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.accountProviderId).Column("accountProviderId").Not.Nullable();
            Map(x => x.accountProviderType).Column("accountProviderType").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.currency).Column("currency").Not.Nullable();
            Map(x => x.refreshAt).Column("refreshAt").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            References(x => x.account).Column("accountId").Nullable();
            References(x => x.bank).Column("banckId").Nullable();
        }
    }
}
