﻿using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class TypeRelationshipMap : ClassMap<TypeRelationship>
    {
        public TypeRelationshipMap()
        {
            Table("typeRelationships");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.code).Column("code").Not.Nullable();
            Map(x => x.description).Column("description").Not.Nullable();
        }
    }
}
