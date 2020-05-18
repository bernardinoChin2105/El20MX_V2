using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine
{
    public class ExcelFileInputData
    {
        public ExcelFileInputData()
        {
            this.Config = new ExcelImportConfig();
        }
        public int ContentLength { get; set; }
        public Stream InputStream { get; set; }
        public string FileName { get; set; }
        public ExcelImportConfig Config { get; set; }

    }
}
