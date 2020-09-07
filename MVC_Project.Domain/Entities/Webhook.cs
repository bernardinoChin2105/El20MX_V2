using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Webhook : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }
        public virtual string provider { get; set; }
        public virtual string eventWebhook { get; set; }
        public virtual string endpoint { get; set; }
        public virtual string response { get; set; }
        public virtual DateTime createdAt { get; set; }
    }
}
