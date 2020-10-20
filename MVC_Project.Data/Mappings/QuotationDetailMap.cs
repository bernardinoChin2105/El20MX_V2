using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class QuotationDetailMap : ClassMap<QuotationDetail>
    {
        public QuotationDetailMap()
        {
            Table("quotationDetails");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.name).Column("name").Nullable();
            Map(x => x.link).Column("link").Nullable();
            
            References(x => x.quotation).Column("quotationId");

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.status).Column("status").Nullable();
        }
    }
}
