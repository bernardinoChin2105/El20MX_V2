﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Entities
{
    public class PaymentForm :IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string code { get; set; }
        public virtual string Description { get; set; }
        public virtual bool banked { get; set; }        
    }
}
