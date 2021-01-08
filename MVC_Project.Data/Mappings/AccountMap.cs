using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class AccountMap : ClassMap<Account>
    {
        public AccountMap()
        {
            Table("accounts");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.rfc).Column("rfc").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.avatar).Column("avatar").Nullable();

            Map(x => x.ciec).Column("ciec").Nullable();
            Map(x => x.ciecStatus).Column("ciecStatus").Nullable();

            Map(x => x.cer).Column("cer").Nullable();
            Map(x => x.key).Column("[key]").Nullable();
            Map(x => x.eFirma).Column("eFirma").Nullable();
            Map(x => x.eFirmaStatus).Column("eFirmaStatus").Nullable();
            Map(x => x.pipedriveId).Column("pipedriveId").Nullable();

            Map(x => x.planSchema).Column("planSchema").Nullable();

            HasMany(x => x.memberships).Inverse().Cascade.All().KeyColumn("accountId");

            //HasManyToMany(x => x.users)
            //   .Cascade.SaveUpdate()
            //   .Table("accountsUsers")
            //   .ParentKeyColumn("accountId");
            //.ChildKeyColumn("userId");
        }
    }
}
