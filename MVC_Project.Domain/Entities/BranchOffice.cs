using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class BranchOffice : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual Guid uuid { get; set; }

        public virtual string name { get; set; }

        public virtual string street { get; set; }
        public virtual string interiorNumber { get; set; }
        public virtual string outdoorNumber { get; set; }

        public virtual string zipCode { get; set; }

        public virtual Settlement colony { get; set; }
        public virtual Municipality municipality { get; set; }
        public virtual State state { get; set; }
        public virtual Country country { get; set; }

        public virtual Account account { get; set; }

        public virtual string serie { get; set; }
        public virtual Int64 folio { get; set; }

        public virtual string logo { get; set; }

        public virtual string cer { get; set; }
        public virtual string key { get; set; }
        public virtual string password { get; set; }
        public virtual string certificateId { get; set; }

        public virtual DateTime createdAt { get; set; }
        public virtual DateTime modifiedAt { get; set; }
        public virtual string status { get; set; }
    }
}
