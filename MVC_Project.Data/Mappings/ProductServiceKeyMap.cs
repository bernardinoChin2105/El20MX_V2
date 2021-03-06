﻿using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class ProductServiceKeyMap : ClassMap<ProductServiceKey>
    {
        public ProductServiceKeyMap()
        {
            Table("productServiceKeys");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.code).Column("code").Not.Nullable();
            Map(x => x.description).Column("description").Not.Nullable();

            Map(x => x.includeIVATransferred).Column("includeIVATransferred").Not.Nullable();
            Map(x => x.includeIEPSTranslated).Column("includeIEPSTranslated").Not.Nullable();
            Map(x => x.borderStripStimulus).Column("borderStripStimulus").Not.Nullable();
            Map(x => x.similarWords).Column("similarWords").Nullable();
        }
    }
}
