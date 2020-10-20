using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class BranchOfficeMap : ClassMap<BranchOffice>
    {
        public BranchOfficeMap()
        {
            Table("branchOffices");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.name).Column("name").Nullable();
            
            Map(x => x.street).Column("street").Nullable();
            Map(x => x.interiorNumber).Column("interiorNumber").Nullable();
            Map(x => x.outdoorNumber).Column("outdoorNumber").Nullable();

            Map(x => x.zipCode).Column("zipCode").Nullable();

            References(x => x.colony).Column("colonyId").Nullable();
            References(x => x.municipality).Column("municipalityId").Nullable();
            References(x => x.state).Column("stateId").Nullable();
            References(x => x.country).Column("countryId").Nullable();

            
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Nullable();
            Map(x => x.status).Column("status").Nullable();

            Map(x => x.serie).Column("serie").Nullable();
            Map(x => x.folio).Column("folio").Nullable();
            Map(x => x.logo).Column("logo").Nullable();

            References(x => x.account).Column("accountId").Nullable();
            
        }
    }
}
