using System;
using System.Collections.Generic;


namespace ExcelEngine
{
    public class ExcelImporterMapper 
    {
        public static ResultExcelImporter<T> ReadExcel<T>(ExcelFileInputData excelFileInputData) where T : class
        {
            ResultExcelImporter<T> resultExcelImporter = new ResultExcelImporter<T>();
            ObjectMapExcel objectMapExcel = new ObjectMapExcel();
            IList<T> data = null;
            resultExcelImporter.FileHasError = !objectMapExcel.TryGetData(excelFileInputData.ContentLength, 
                excelFileInputData.FileName, excelFileInputData.InputStream, 
                out data, excelFileInputData.Config.IsFirstRowAsColumNames);
            resultExcelImporter.Data = data;
            resultExcelImporter.ResultMapExcel = objectMapExcel.Results;
            resultExcelImporter.Headers = objectMapExcel.Headers;
            return resultExcelImporter;
        }
    }
}
