using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class BasePagination
    {
        public Nullable<int> PageNum { get; set; }
        public Nullable<int> PageSize { get; set; }

        public Nullable<DateTime> CreatedOnStart { get; set; }
        public Nullable<DateTime> CreatedOnEnd { get; set; }

        public BasePagination()
        {
            PageNum = 1;
            PageSize = 10;
        }
    }

}
