using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class TypeInvoiceMap : ClassMap<TypeInvoice>
    {
        public TypeInvoiceMap()
        {
            Table("typeInvoices");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.description).Column("Description").Not.Nullable();
        }
    }
}
