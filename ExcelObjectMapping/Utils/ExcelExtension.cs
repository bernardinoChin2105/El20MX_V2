
using ExcelDataReader;
using ExcelEngine;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
    public class ExcelExtension
    {
        public static string ColumnIndexToColumnLetter(int colIndex)
        {
            int div = colIndex;
            string colLetter = String.Empty;
            int mod = 0;

            while (div > 0)
            {
                mod = (div - 1) % 26;
                colLetter = (char)(65 + mod) + colLetter;
                div = (int)((div - mod) / 26);
            }
            return colLetter;
        }
        public static int ColumnLetterToColumnIndex(string columnLetter)
        {
            columnLetter = columnLetter.ToUpper();
            int sum = 0;

            for (int i = 0; i < columnLetter.Length; i++)
            {
                sum *= 26;
                sum += (columnLetter[i] - 'A' + 1);
            }
            return sum - 1;
        }
        public static string GetCSVTable(IList<string> headers, IList<IList<string>> rows)
        {
            StringBuilder content = new StringBuilder();
            string headersStr = String.Join(",", headers);
            content.Append(headersStr);
            content.Append(Environment.NewLine);
            foreach (IList<string> row in rows)
            {
                string rowCsv = String.Join(",", row);
                content.Append(rowCsv);
                content.Append(Environment.NewLine);
            }
            return content.ToString();
        }

        public static IEnumerable<string> GetWorksheetNames(string path, string ext)
        {
            var reader = GetExcelReader(path, ext);
            var workbook = reader.AsDataSet();
            var sheets = from DataTable sheet in workbook.Tables select sheet.TableName;
            return sheets;
        }
        public static IEnumerable<string> GetWorksheetNames(Stream stream, string ext, out IExcelDataReader dataReader, bool isFirstRowAsColumnNames = true)
        {
            dataReader = GetExcelReader(stream, ext);
            var workbook = dataReader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = isFirstRowAsColumnNames
                }
            });
            var sheets = from DataTable sheet in workbook.Tables select sheet.TableName;
            return sheets;
        }
        public static IExcelDataReader GetExcelReader(Stream stream, string ext)
        {
            // ExcelDataReader works with the binary Excel file, so it needs a FileStream
            // to get started. This is how we avoid dependencies on ACE or Interop:

            // We return the interface, so that 
            IExcelDataReader reader = null;
            try
            {
                if (".xls".Equals(ext))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                if (".xlsx".Equals(ext))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                   
                }
                if (".csv".Equals(ext))
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }
                
                return reader;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IExcelDataReader GetExcelReader(string path, string ext)
        {
            // ExcelDataReader works with the binary Excel file, so it needs a FileStream
            // to get started. This is how we avoid dependencies on ACE or Interop:

            FileStream stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read);
            // We return the interface, so that 
            IExcelDataReader reader = null;
            try
            {
                reader = GetExcelReader(stream, ext);
                return reader;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IEnumerable<DataRow> GetData(string sheet, string file, string ext, bool firstRowIsColumnNames = true)
        {
            var reader = GetExcelReader(file, ext);
            var workSheet = reader.AsDataSet(new ExcelDataSetConfiguration() {
                    ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = firstRowIsColumnNames
                    }
            }).Tables[sheet];
            var rows = from DataRow a in workSheet.Rows select a;
            return rows;
        }
        public static IEnumerable<DataRow> GetData(string sheet, IExcelDataReader reader, string ext, bool firstRowIsColumnNames = true, Action<IExcelDataReader> readHeaderRow = null)
        {
            var workSheet = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = firstRowIsColumnNames,
                    ReadHeaderRow = readHeaderRow 
                }
            }).Tables[sheet];
            reader.Read();
            if (workSheet == null)
            {
                return null;
            }
            var rows = from DataRow a in workSheet.Rows select a;
            return rows;
        }
    }
}