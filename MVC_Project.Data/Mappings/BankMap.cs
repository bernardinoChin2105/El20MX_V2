﻿using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class BankMap : ClassMap<Bank>
    {
    
        public BankMap()
        {
            Table("banks");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.providerId).Column("providerId").Not.Nullable();

            Map(x => x.nameSite).Column("nameSite").Nullable();
            Map(x => x.providerSiteId).Column("providerSiteId").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
