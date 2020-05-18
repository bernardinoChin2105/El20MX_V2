using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine
{
    public class RowResult
    {

        public RowResult()
        {
            this.ColumnResults = new List<ColumnResult>();
        }
        public dynamic RowsValues { get; set; }
        public int Index { get; set; }
        public int Number { get; set; }
        public bool HasError { get
            {
                return this.ColumnResults.Any(x=>x.ErrorMessages.Any());
            }
        }

        public IEnumerable<string> ErrorMessages
        {
            get
            {
                return ColumnResults.SelectMany(y => y.ErrorMessages);
            }
        }
        public IList<ColumnResult> ColumnResults { get; set; }
    }
}
