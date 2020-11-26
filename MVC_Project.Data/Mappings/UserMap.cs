using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class UserMap : ClassMap<User> {

        public UserMap() {
            Table("users");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();
            Map(x => x.name).Column("name").Not.Nullable();
            Map(x => x.password).Column("password").Nullable();
            Map(x => x.passwordExpiration).Column("passwordExpiration").Nullable();
            Map(x => x.token).Column("token").Nullable();
            Map(x => x.tokenExpiration).Column("tokenExpiration").Nullable();
            Map(x => x.lastLoginAt).Column("lastLoginAt").Nullable();
            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();
            Map(x => x.agreeTerms).Column("agreeTerms").Nullable();
            Map(x => x.pipedriveId).Column("pipedriveId").Nullable();

            References(x => x.profile).Column("profileId");

            Map(x => x.isBackOffice).Column("isBackOffice").Nullable();//Indica si el usuario es colaborador de el20mx

            HasMany(x => x.memberships).Inverse().Cascade.All().KeyColumn("userId");
            
            //HasManyToMany(x => x.accounts)
            //   .Cascade.SaveUpdate()
            //   .Table("accountsUsers")
            //   .ParentKeyColumn("userId")
            //   .ChildKeyColumn("accountId");
        }
    }
}