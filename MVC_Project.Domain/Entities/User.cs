using System;
using System.Collections.Generic;
using System.Text;

namespace MVC_Project.Domain.Entities {

    public class User : IEntity {
        public virtual int Id { get; set; }
        public virtual string Uuid { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }
        public virtual string Username { get; set; }
        public virtual string Language { get; set; }
        public virtual string Password { get; set; }
        public virtual string EmployeeIdentifier { get; set; }
        public virtual string MobileNumber { get; set; }
        public virtual string Profile { get; set; }
        public virtual DateTime? PasswordExpiration { get; set; }
        public virtual Role Role { get; set; }
        public virtual IList<Permission> Permissions { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
        public virtual DateTime? RemovedAt { get; set; }
        public virtual Boolean Status { get; set; }
        public virtual string Token { get; set; }
        public virtual DateTime? ExpiraToken { get; set; }
        public virtual string ApiKey { get; set; }
        public virtual DateTime? ExpiraApiKey { get; set; }
        public virtual DateTime? LastLoginAt { get; set; }

        public User()
        {            
            Permissions = new List<Permission>();
        }
    }
}