using ExcelDataReader;
using Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelEngine.TypeAdapters;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace ExcelEngine
{
    public class ObjectMapExcel
    {
        public ResultMapExcel Results { get; set; }

        public IList<Header> Headers { get; set; }

        private IList<int> ModelColumnsNumber { get; set; }
        public ObjectMapExcel()
        {
            Results = new ResultMapExcel();
            this.Headers = new List<Header>();
        }
        public void GetHeaders(IExcelDataReader reader)
        {
            foreach(int headerIndex in this.ModelColumnsNumber)
            {
                int excelIndex = headerIndex + 1;
                this.Headers.Add(new Header
                {
                    Column = excelIndex,
                    Letter = ExcelExtension.ColumnIndexToColumnLetter(excelIndex),
                    Name = Convert.ToString(reader.GetValue(headerIndex)),
                    Index = headerIndex
                });
            }
        }
        public bool TryGetDataFile(int contentLength, string fileName, Stream inputStream, out IEnumerable<DataRow> dataRows, bool isFirstRowAsColumNames = true)
        {
            dataRows = new List<DataRow>();
            if (contentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(fileName);

                if (".xls".Equals(fileExtension) || ".xlsx".Equals(fileExtension) || ".csv".Equals(fileExtension))
                {
                    IExcelDataReader dataReader = null;
                    IEnumerable<string> sheets = ExcelExtension.GetWorksheetNames(inputStream, fileExtension, out dataReader, isFirstRowAsColumNames);
                    if (!sheets.Any())
                    {
                        return false;
                    }
                    dataRows = ExcelExtension.GetData(sheets.First(), dataReader, fileExtension, isFirstRowAsColumNames, this.GetHeaders);
                    if (dataRows != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void SetColumnsNumber<T>()
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            object instance = constructor.Invoke(new object[] { });
            PropertyInfo[] properties = type.GetProperties();
            this.ModelColumnsNumber = new List<int>();
            foreach (PropertyInfo property in properties)
            {
                ExcelColumn excelColumn = property.GetCustomAttribute<ExcelColumn>();
                if (excelColumn == null)
                {
                    continue;
                }
                int columnNumber = excelColumn.Column;
                if (!String.IsNullOrWhiteSpace(excelColumn.Letter))
                {
                    columnNumber = ExcelExtension.ColumnLetterToColumnIndex(excelColumn.Letter);
                }
                this.ModelColumnsNumber.Add(columnNumber);
            }
        }
        public bool TryGetObject<T>(DataRow dataRow, out T dataObj, int rowNumber)
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            object instance = constructor.Invoke(new object[] { });
            PropertyInfo[] properties = type.GetProperties();
            dataObj = default(T);
            dynamic dataObjRaw = new ExpandoObject();
            IDictionary<string, object> dynamicObjDictionary = dataObjRaw as IDictionary<string, object>;
            RowResult rowResult = new RowResult
            {
                Index = rowNumber,
                Number = rowNumber + 1
            };
            foreach (PropertyInfo property in properties)
            {
                ExcelColumn excelColumn = property.GetCustomAttribute<ExcelColumn>();
                if (excelColumn == null)
                {
                    continue;
                }
                int columnNumber = excelColumn.Column;
                if (!String.IsNullOrWhiteSpace(excelColumn.Letter))
                {
                    columnNumber = ExcelExtension.ColumnLetterToColumnIndex(excelColumn.Letter);
                }
                if (columnNumber >= dataRow.ItemArray.Length)
                {
                    return false;
                }
                object columnData = dataRow[columnNumber];
                dynamicObjDictionary.Add(property.Name, Convert.ToString(columnData));
                Header header = this.Headers.FirstOrDefault(x => x.Index == columnNumber);
                string excelName = header.Name;
                if (String.IsNullOrWhiteSpace(excelName))
                {
                    excelName = String.Format("\"{0}\"", header.Letter);
                }
                //Validar
                IEnumerable<ValidationAttribute> validationAttributes = property.GetCustomAttributes<ValidationAttribute>();
                IList<ValidationResult> result = null;

                ColumnResult columnResult = new ColumnResult
                {
                    Header = header
                };
                if(columnData != null)
                {
                    columnResult.Value = Convert.ToString(columnData);
                    
                }
                if (!ValidatorUtil.ValidateValue(columnData, excelName, validationAttributes, out result))
                {
                    columnResult.ErrorMessages = result.Select(x => x.ErrorMessage).ToList();
                    rowResult.ColumnResults.Add(columnResult);
                    continue;
                }
                //if has a adapte process
                if (excelColumn.Adapter != null)
                {
                    string dataStr = Convert.ToString(columnData);
                    ITypeAdapter adapter = (ITypeAdapter)Activator.CreateInstance(excelColumn.Adapter);
                    columnData = adapter.AdaptFormat(dataStr);
                }
                string typeErrorMessage = null;
                if(!this.TryGetAttribute(instance, property, columnData, header, rowResult, out typeErrorMessage))
                {
                    columnResult.ErrorMessages.Add(typeErrorMessage);
                }
                rowResult.ColumnResults.Add(columnResult);
            }
            rowResult.RowsValues = dataObjRaw;
            Results.RowResults.Add(rowResult);
            dataObj = (T)instance;
            return !rowResult.HasError;
        }
        private bool  TryGetAttribute(object instance, PropertyInfo property, object data, Header header, RowResult rowResult, out string errorMessage)
        {
            errorMessage = null;
            Type propertyType = property.PropertyType;
            ErrorMap errorMap = new ErrorMap
            {
                Line = rowResult.Index,
                ExcelLine = rowResult.Number,
                Column = header.Index,
                ExcelColumn = header.Column,
                ColumnLetter = header.Letter
            };
            string excelName = header.Name;
            if (String.IsNullOrWhiteSpace(excelName))
            {
                excelName = String.Format("\"{0}\"", header.Letter);
            }
            if (propertyType == typeof(DateTime))
            {
                DateTime objectData = new DateTime();
                if (DateUtil.TryParserObject(data, out objectData))
                {
                    property.SetValue(instance, objectData);
                    return true;
                }
                errorMessage = String.Format("El campo {0} no tiene el formato de fecha ", excelName);
                errorMap.Description = errorMessage;
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            if (propertyType == typeof(DateTime?))
            {
                string dataStr = Convert.ToString(data);
                if (String.IsNullOrWhiteSpace(dataStr))
                {
                    property.SetValue(instance, null);
                    return true;
                }
                DateTime objectData = new DateTime();
                if (DateUtil.TryParserObject(data, out objectData))
                {
                    property.SetValue(instance, objectData);
                    return true;
                }
                errorMessage = String.Format("El campo {0} no tiene el formato de fecha ", excelName);
                errorMap.Description = errorMessage;
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            try
            {
                if (propertyType == typeof(string))
                {
                    property.SetValue(instance, Convert.ToString(data));
                }
                else
                {
                    object value = null;
                    if(Nullable.GetUnderlyingType(propertyType) != null)
                    {
                        if(data == null || String.IsNullOrWhiteSpace(Convert.ToString(data)))
                        {
                            property.SetValue(instance, null);
                            return true;
                        }
                    }
                    if (FormatUtils.TryGetValue(data, propertyType, out value))
                    {
                        property.SetValue(instance, value);
                    }
                    else
                    {
                        errorMessage = String.Format("El campo {0} no coincide con el formato especificado ", excelName);
                        return false;
                    }
                }
            }
            catch (InvalidCastException e)
            {

                errorMessage = String.Format("El campo {0} no coincide con el formato especificado ", excelName);
                errorMap.Description = errorMessage;
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            catch (FormatException e)
            {

                errorMessage = String.Format("El campo {0} no coincide con el formato especificado ", excelName);
                errorMap.Description = errorMessage;
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            return true;
        }
        private bool TryGetAttribute(object instance, PropertyInfo property, object data, int columnNumber, int rowNumber)
        {
            Type propertyType = property.PropertyType;
            ErrorMap errorMap = new ErrorMap
            {
                Line = rowNumber,
                ExcelLine = rowNumber + 1,
                Column = columnNumber,
                ExcelColumn = columnNumber + 1,
                ColumnLetter = ExcelExtension.ColumnIndexToColumnLetter(columnNumber)
            };
            if (propertyType == typeof(DateTime))
            {
                DateTime objectData = new DateTime();
                if (DateUtil.TryParserObject(data, out objectData))
                {
                    property.SetValue(instance, objectData);
                    return true;
                }
                errorMap.Description = "Error en el formato de la fecha";
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            if (propertyType == typeof(DateTime?))
            {
                string dataStr = Convert.ToString(data);
                if (String.IsNullOrWhiteSpace(dataStr))
                {
                    property.SetValue(instance, null);
                    return true;
                }
                DateTime objectData = new DateTime();
                if (DateUtil.TryParserObject(data, out objectData))
                {
                    property.SetValue(instance, objectData);
                    return true;
                }
                errorMap.Description = "Error en el formato de la fecha";
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            try
            {
                if (propertyType == typeof(string))
                {
                    property.SetValue(instance, Convert.ToString(data));
                }
                else
                {
                    object value = null;
                    if (FormatUtils.TryGetValue(data, property.PropertyType, out value))
                    {
                        property.SetValue(instance, value);
                    }
                }
            }
            catch (InvalidCastException e)
            {

                errorMap.Description = "Error en el tipo de dato no coincide con el especificado";
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            catch (FormatException e)
            {

                errorMap.Description = "Error en el tipo de dato no coincide con el especificado";
                Results.ErrorMap.Add(errorMap);
                return false;
            }
            return true;
        }
        public bool IsAllColumnsEmpty(DataRow dataRow)
        {
            return dataRow == null || dataRow.ItemArray.All(x => x == null || String.IsNullOrWhiteSpace(Convert.ToString(x)));
        }
        public bool TryGetData<T>(int contentLength, string fileName, Stream inputStream, out IList<T> data, bool isFirstRowAsColumNames = true, int headerColumn = 0)
        {
            IEnumerable<DataRow> dataRows = null;
            data = new List<T>();
            this.SetColumnsNumber<T>();
            if (this.TryGetDataFile(contentLength, fileName, inputStream, out dataRows, isFirstRowAsColumNames))
            {
                int count = dataRows.Count();
                for (int rowNumber = 0; rowNumber < dataRows.Count(); rowNumber++)
                {
                    DataRow dataRow = dataRows.ElementAt(rowNumber);
                    int numOfColumns = dataRow.ItemArray.Count();
                    if (this.IsAllColumnsEmpty(dataRow))
                    {
                        continue;
                    }
                    T obj = default(T);
                    if (this.TryGetObject(dataRow, out obj, rowNumber))
                    {
                        data.Add(obj);
                    }
                }
                return true;
            }
            return false;
        }
    }
}