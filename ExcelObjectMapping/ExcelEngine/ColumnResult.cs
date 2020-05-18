using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine
{
    public class ColumnResult
    {
        public ColumnResult()
        {
            ErrorMessages = new List<string>();
        }
        public string Value { get; set; }
        public Header Header { get; set; }
        public IList<string> ErrorMessages { get; set; }
    }
}
