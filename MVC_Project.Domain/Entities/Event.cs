using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class Event : IEntity
    {

        public virtual int Id { get; set; }

        public virtual string Uuid { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual bool IsFullDay { get; set; }

        public virtual DateTime StartDate { get; set; }

        public virtual DateTime? EndDate { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual User User { get; set; }
    }
}
