using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelEngine
{
    public class ResultMapExcel
    {
        public ResultMapExcel()
        {
            ErrorMap = new List<ErrorMap>();
            RowResults = new List<RowResult>();
        }
        public bool HasError
        {
            get
            {
                if (RowResults != null && RowResults.Any(x => x.HasError))
                {
                    return true;
                }
                return false;
            }
        }
        //Oldest Error result
        public IList<ErrorMap> ErrorMap { get; set; }
        
        public IList<RowResult> RowResults { get; set; }
    }
}