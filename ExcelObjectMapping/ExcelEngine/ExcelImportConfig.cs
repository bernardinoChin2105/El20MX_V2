using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine
{
    public class ExcelImportConfig
    {
        public ExcelImportConfig()
        {
            IsFirstRowAsColumNames = true;
            HeaderRow = 0;
        }

        public bool IsFirstRowAsColumNames { get; set; }
        public int HeaderRow { get; set; }
    }
}
