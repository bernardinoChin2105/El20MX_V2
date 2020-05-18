using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine
{
    public class ResultExcelImporter<T> where T : class
    {
        public IList<T> Data { get; set; }
        public ResultMapExcel ResultMapExcel { get; set; }
        public bool FileHasError { get; set; }
        public IList<Header> Headers { get; set; }

    }
}
