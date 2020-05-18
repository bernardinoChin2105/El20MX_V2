using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings {

    public class UserMap : ClassMap<User> {

        public UserMap() {
            Table("users");
            Id(x => x.Id).GeneratedBy.Identity().Column("UserId");
            Map(x => x.Uuid).Column("uuid").Not.Nullable();
            Map(x => x.EmployeeIdentifier).Column("employee_identifier").Nullable();
            Map(x => x.MobileNumber).Column("mobile_number").Nullable();
            Map(x => x.Profile).Column("profile").Nullable();
            Map(x => x.FirstName).Column("first_name").Not.Nullable();
            Map(x => x.LastName).Column("last_name").Nullable();
            Map(x => x.Email).Column("email").Not.Nullable();
            Map(x => x.Password).Column("password").Not.Nullable();            
            Map(x => x.CreatedAt).Column("created_at").Not.Nullable();
            Map(x => x.UpdatedAt).Column("updated_at").Not.Nullable();
            Map(x => x.RemovedAt).Column("removed_at").Nullable();
            Map(x => x.Status).Column("status").Nullable();
            Map(x => x.ExpiraToken).Column("expira_token").Nullable();
            Map(x => x.Token).Column("token").Nullable();
            Map(x => x.ApiKey).Column("apikey").Nullable();
            Map(x => x.Language).Column("language").Nullable();
            Map(x => x.ExpiraApiKey).Column("expira_apikey").Nullable();
            Map(x=>x.LastLoginAt).Column("last_login_at").Nullable();
            Map(x => x.Username).Column("username").Nullable();
            Map(x => x.PasswordExpiration).Column("password_expiration").Nullable();
            References(x => x.Role).Column("RoleId");
            HasManyToMany(x => x.Permissions)
                .Cascade.SaveUpdate()
                .Table("permission_user")
                .ParentKeyColumn("UserId")
                .ChildKeyColumn("PermissionId");
        }
    }
}