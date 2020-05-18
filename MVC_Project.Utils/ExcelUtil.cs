using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.IO;
using System.ComponentModel;
using System.Collections;

namespace MVC_Project.Utils
{
    public class ExcelUtil
    {
        private static string _TableNameDefault = "Hoja";
        private static string _Mensage = string.Empty;
        public static MemoryStream ExportListCollection(Hashtable ListModelOrigen, Boolean ExtraerDatos = false, Boolean FiltrarColumnas = true, Int32 Width = 30, Dictionary<string, Int32> WidthColumns = null, List<string> ExcluirColumnas = null, Dictionary<string, string> NuevoNombreColumnas = null)
        {
            _Mensage = string.Empty;
            MemoryStream streambook = new MemoryStream();
            try
            {
                using (var workbook = SpreadsheetDocument.Create(streambook, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                    var stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    Stylesheet styles = new CustomStylesheet();
                    styles.Save(stylesPart);

                    foreach (DictionaryEntry entry in ListModelOrigen)
                    {
                        ExtraerDatosCollection(entry.Value, entry.Key.ToString(), workbook, ExtraerDatos, FiltrarColumnas, Width, WidthColumns, ExcluirColumnas, NuevoNombreColumnas);
                    }
                }
            }
            catch (Exception ex)
            {
                _Mensage = "No se ha podido generar el archivo de Excel. Verifique si hay información para procesar";
                CrearMensage(ref streambook, _Mensage, _TableNameDefault);
            }
            return streambook;
        }

        /// <summary>
        /// Método que devuelve el MemoryStream de la generación del archivo Excel, el origen de datos puede ser un DataSet o una lista.
        /// </summary>
        /// <typeparam name="T">Tipo del modelo.</typeparam>
        /// <param name="dsOrigen">DataSet que contiene la información que se exportará en el Excel.</param>
        /// <param name="Origen">Lista que contiene la información que se exportará en el Excel.</param>
        /// <param name="oListColumnas"></param>
        /// <param name="ExtraerDatos">True: Indica si la información será transferida al Excel. False: La información del origen no es transferida al Excel.</param>
        /// <param name="FiltrarColumnas">Indica si contendrá filtros</param>
        /// <param name="ExcluirColumnas">Lista que contiene las columnas que se excluirán.</param>
        /// <param name="NombreHoja">Nombre que se le asigna a la hoja.</param>
        /// <param name="NuevoNombreColumnas">Lista que contiene los nuevos nombres de las columnas, para que no tomen el nombre por default.</param>
        /// <returns></returns>
        public static MemoryStream ExportDataSet<T>(DataSet dsOrigen, List<T> Origen, T oListColumnas, Boolean ExtraerDatos = false, Boolean FiltrarColumnas = true, Int32 Width = 30, Dictionary<string, Int32> WidthColumns = null, List<string> ExcluirColumnas = null, string NombreHoja = "", Dictionary<string, string> NuevoNombreColumnas = null)
        {
            _Mensage = string.Empty;
            MemoryStream streambook = new MemoryStream();
            try
            {
                using (var workbook = SpreadsheetDocument.Create(streambook, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                    var stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    Stylesheet styles = new CustomStylesheet();
                    styles.Save(stylesPart);

                    if (dsOrigen == null && Origen != null)
                    {
                        if (Origen.Count() > 0)
                        {
                            ExtraerDatosCollection(Origen, NombreHoja == "" && Origen.Count() > 0 ? Origen[0].GetType().Name : NombreHoja, workbook, ExtraerDatos, FiltrarColumnas, Width, WidthColumns, ExcluirColumnas, NuevoNombreColumnas);
                        }
                        else
                        {
                            _Mensage = "No hay Información para procesar.";
                        }
                    }
                    else
                    {
                        if (dsOrigen != null)
                        {
                            ExtraerDatosInfo(dsOrigen, workbook, "", ExtraerDatos, FiltrarColumnas, Width, WidthColumns, ExcluirColumnas, NuevoNombreColumnas, NombreHoja);
                        }
                        else
                        {
                            //Crear formato con las columnas
                            if (oListColumnas != null)
                            {
                                CrearArchivoExcel(oListColumnas, NombreHoja == string.Empty ? _TableNameDefault : NombreHoja, workbook, FiltrarColumnas, Width, WidthColumns, ExcluirColumnas, NuevoNombreColumnas);
                            }
                            else
                            {
                                _Mensage = "No hay un origen para crear el archivo de Excel";
                            }
                        }
                    }
                }

                //Verificamos si hay mensaje de error.
                if (_Mensage.Trim() != string.Empty)
                {
                    CrearMensage(ref streambook, _Mensage, _TableNameDefault);
                }

            }
            catch (Exception ex)
            {
                _Mensage = "No se ha podido generar el archivo de Excel. Verifique si hay información para procesar.";
                CrearMensage(ref streambook, _Mensage, _TableNameDefault);
            }
            return streambook;
        }

        static void ExtraerDatosCollection<T>(T Origen, string TableName, SpreadsheetDocument workbook, Boolean ExtraerDatos = false, Boolean FiltrarColumnas = true, Int32 Width = 30, Dictionary<string, Int32> WidthColumns = null, List<string> ExcluirColumnas = null, Dictionary<string, string> NuevoNombreColumnas = null)
        {
            try
            {
                if (((IList)Origen).Count > 0)
                {
                    List<string> oColumnas = ((IList)Origen)[0].GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(propertyInfo => propertyInfo.Name).ToList();
                    RemoverColumnas(ref oColumnas, ExcluirColumnas);

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    Sheets sheets = CrearHoja(workbook, TableName, ref sheetPart, ref sheetData);
                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    AutoFilter autoFilter1 = new AutoFilter() { Reference = "A1:" };// + GetCellReference(1, (uint)oColumnas.Count() - 1)
                    CreateHeader(oColumnas, ref headerRow, ref sheetPart, Width, WidthColumns, NuevoNombreColumnas);
                    //add columns header
                    sheetData.AppendChild(headerRow);
                    for (int i = 0; i < ((IList)Origen).Count; i++)
                    {
                        var Iitem = ((IList)Origen)[i];
                        if (ExtraerDatos)
                        {
                            Row Rowitem = new Row();
                            if (ExtraerData(ref Rowitem, Iitem, oColumnas))
                                sheetData.AppendChild(Rowitem);
                        }
                    }
                    if (FiltrarColumnas)
                        sheetPart.Worksheet.Append(autoFilter1);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void ExtraerDatosInfo<T>(DataSet ds, SpreadsheetDocument workbook, T oListColumnas, Boolean ExtraerDatos = false, Boolean FiltrarColumnas = true, Int32 Width = 30, Dictionary<string, Int32> WidthColumns = null, List<string> ExcluirColumnas = null, Dictionary<string, string> NuevoNombreColumnas = null, string NombreHojas = null)
        {
            try
            {
                List<string> oColumnas;
                var nombresTablas = NombreHojas.Split('|');
                int numTabla = 0;
                //Por Cada table representa una hoja en el libro de Excel.
                foreach (System.Data.DataTable table in ds.Tables)
                {
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    var nombreTabla = table.TableName;

                    if (nombresTablas != null && ds.Tables.Count == nombresTablas.Count())
                    {
                        nombreTabla = nombresTablas[numTabla];
                    }

                    Sheets sheets = CrearHoja(workbook, nombreTabla, ref sheetPart, ref sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    numTabla++;
                    //RECORRER LAS COLUMNAS PARA AGREGARLAS A LA HOLA
                    Boolean hasColumns;
                    var ColumnList = oListColumnas;
                    if (ColumnList == null)
                    {
                        hasColumns = false;
                    }
                    else
                    {
                        hasColumns = oListColumnas.GetType().ToString() == "System.String" ? !(((string)(object)oListColumnas) == "") : true;
                    }

                    // 1 - Extraer las columnas
                    if (hasColumns && ExtraerDatos)
                    {
                        oColumnas = ExtraerColumnas(oListColumnas);
                    }
                    else
                    {
                        oColumnas = ExtraerColumnas(table);
                    }
                    RemoverColumnas(ref oColumnas, ExcluirColumnas);
                    //autoFilter1.
                    AutoFilter autoFilter1 = new AutoFilter() { Reference = "A1:" };//+ GetCellReference(1, (uint)oColumnas.Count() - 1)
                    CreateHeader(oColumnas, ref headerRow, ref sheetPart, Width, WidthColumns, NuevoNombreColumnas);
                    //add columns header
                    sheetData.AppendChild(headerRow);

                    if (ExtraerDatos)
                    {
                        //Extraer Datos de DS
                        ExtraerData(table, ref sheetData, oColumnas);
                    }

                    if (FiltrarColumnas)
                        sheetPart.Worksheet.Append(autoFilter1);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void CrearArchivoExcel<T>(T Origen, string TableName, SpreadsheetDocument workbook, Boolean FiltrarColumnas = true, Int32 Width = 30, Dictionary<string, Int32> WidthColumns = null, List<string> ExcluirColumnas = null, Dictionary<string, string> NuevoNombreColumnas = null)
        {
            try
            {
                List<string> oColumnas = ExtraerColumnas(Origen);
                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                Sheets sheets = CrearHoja(workbook, TableName, ref sheetPart, ref sheetData);
                Row headerRow = new Row();
                AutoFilter autoFilter1 = new AutoFilter() { Reference = "A1:" };//+ GetCellReference(1, (uint)oColumnas.Count() - 1)
                CreateHeader(oColumnas, ref headerRow, ref sheetPart, Width, WidthColumns, NuevoNombreColumnas);
                sheetData.AppendChild(headerRow);
                if (FiltrarColumnas)
                    sheetPart.Worksheet.Append(autoFilter1);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Eliminar las columnas de la lista de columnas.
        /// </summary>
        /// <param name="Columnas">Lista de columnas a mostrar</param>
        /// <param name="ExcluirColumnas">Lista de columnas a eliminar o remover para que ya no se muestra en la hoja de Excel</param>
        static void RemoverColumnas(ref List<string> Columnas, List<string> ExcluirColumnas)
        {
            try
            {
                if (ExcluirColumnas != null)
                {
                    for (int i = 0; i < ExcluirColumnas.Count; i++)
                    {
                        Columnas.Remove(ExcluirColumnas[i]);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        static Boolean CreateHeader(List<string> oColumnas, ref Row headerRow, ref WorksheetPart sheetPart, Int32 Width, Dictionary<string, Int32> WidthColumns, Dictionary<string, string> ListNuevoNombreColumnas = null)
        {
            try
            {
                Boolean AgregarHeader = false;
                string NuevoNombreColumna = string.Empty;
                uint cont = 1;
                foreach (var Columna in oColumnas)
                {
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    NuevoNombreColumna = CambiarNombre(Columna, ListNuevoNombreColumnas);
                    NuevoNombreColumna = NuevoNombreColumna.TrimEnd() == string.Empty ? SepararNombreColumna(Columna) : NuevoNombreColumna;
                    cell.CellValue = new CellValue(NuevoNombreColumna);
                    cell.StyleIndex = 11;

                    headerRow.AppendChild(cell);

                    SetColumnWidth(sheetPart.Worksheet, cont, Width);

                    if (WidthColumns != null && WidthColumns.ContainsKey(Columna))
                    {
                        SetColumnWidth(sheetPart.Worksheet, cont, WidthColumns[Columna]);
                    }

                    cont = cont + 1;
                    AgregarHeader = false;
                }
                return AgregarHeader;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Separa el nombre de las columnas con espacio en blanco donde encuente Mayuscula por ejemplo: OrdenTrabajo - Orden Trabajo.
        /// </summary>
        /// <param name="NombreColumna">Nombre de la columna a cambiar</param>
        /// <returns></returns>
        private static string SepararNombreColumna(string NombreColumna)
        {
            string NombreSeparado = string.Empty;
            int ContMayusculas = 0;
            char LetraAnterior = ' ';
            foreach (char item in NombreColumna)
            {
                if (char.IsUpper(item))
                {
                    if (ContMayusculas > 0 && char.IsLower(LetraAnterior))
                    {
                        NombreSeparado += " ";
                        LetraAnterior = ' ';
                    }
                    else
                    {
                        ContMayusculas += 1;
                        LetraAnterior = item;
                    }
                    NombreSeparado += item;
                }
                else
                {
                    NombreSeparado += item;
                    LetraAnterior = item;
                }
            }
            return NombreSeparado;
        }

        /// <summary>
        /// Cambia el nombre de la columna de acuerdo al valor que se le asigne.
        /// </summary>
        /// <param name="Columna">Nombre de la columna</param>
        /// <param name="ListColumnas">Lista de columnas donde cada columna tiene el nuevo nombre que mostrará</param>
        /// <returns></returns>
        private static string CambiarNombre(string Columna, Dictionary<string, string> ListColumnas)
        {
            string ColumnaCambiada = string.Empty;
            if (ListColumnas != null && ListColumnas.Count > 0)
            {
                ColumnaCambiada = ListColumnas.ContainsKey(Columna) ? ListColumnas[Columna] : string.Empty;
            }

            return ColumnaCambiada;
        }

        //public static StringValue GetCellReference(uint row, uint column) => new StringValue($"{GetColumnName("", column)}{row}");

        //static string GetColumnName(string prefix, uint column) => column < 26 ? $"{prefix}{(char)(65 + column)}" :
        //GetColumnName(GetColumnName(prefix, (column - column % 26) / 26 - 1), column % 26);

        static void SetColumnWidth(Worksheet worksheet, uint Index, DoubleValue dwidth, bool hidden = false)
        {
            try
            {
                DocumentFormat.OpenXml.Spreadsheet.Columns cs = worksheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Columns>();
                if (cs != null)
                {
                    IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Column> ic = cs.Elements<DocumentFormat.OpenXml.Spreadsheet.Column>().Where(r => r.Min == Index).Where(r => r.Max == Index);
                    if (ic.Count() > 0)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Column c = ic.First();
                        c.Width = dwidth;
                    }
                    else
                    {
                        cs = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                        DocumentFormat.OpenXml.Spreadsheet.Column c = new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = Index, Max = Index, Hidden = hidden, Width = dwidth, CustomWidth = true };
                        cs.Append(c);
                        worksheet.InsertAfter(cs, worksheet.GetFirstChild<SheetProperties>());
                    }
                }
                else
                {
                    cs = new DocumentFormat.OpenXml.Spreadsheet.Columns();
                    DocumentFormat.OpenXml.Spreadsheet.Column c = new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = Index, Max = Index, Hidden = hidden, Width = dwidth, CustomWidth = true };
                    cs.Append(c);
                    worksheet.InsertAfter(cs, worksheet.GetFirstChild<SheetProperties>());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<String> ExtraerColumnas<T>(T oListColumnas)
        {
            try
            {

                List<String> columns = new List<string>();
                string TypeElement = typeof(T).Name;

                switch (TypeElement)
                {
                    case "DataSet":
                        foreach (DataColumn column in ((DataSet)(object)oListColumnas).Tables[0].Columns)
                        {
                            columns.Add(column.ColumnName);
                        }
                        break;
                    case "DataTable":
                        foreach (DataColumn column in ((DataTable)(object)oListColumnas).Columns)
                        {
                            columns.Add(column.ColumnName);
                        }
                        break;
                    case "String[]":
                        string[] oCols;
                        oCols = (string[])(object)oListColumnas;
                        columns.AddRange(oCols);
                        break;
                    case "String":
                        string ocols = (string)(object)oListColumnas;
                        columns.AddRange(((string)(object)oListColumnas).Split(','));
                        break;
                    default:
                        columns = oListColumnas.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(propertyInfo => propertyInfo.Name).ToList();
                        break;

                }
                return columns;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static Sheets CrearHoja(SpreadsheetDocument workbook, string TableName, ref WorksheetPart sheetPart, ref SheetData sheetData)
        {
            try
            {
                sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                uint sheetId = 1;
                if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = TableName };
                sheets.Append(sheet);
                return sheets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void ExtraerData(DataTable table, ref SheetData sheetData, List<string> columns)
        {
            try
            {
                foreach (DataRow dsrow in table.Rows)
                {
                    Row newRow = new Row();
                    foreach (String col in columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString());
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static Boolean ExtraerData<T>(ref Row oRow, T Origen, List<string> oColumnas)
        {
            try
            {
                Boolean AgregarElemento = false;
                PropertyInfo[] Props = Origen.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var values = new object[Props.Length];

                for (int k = 0; k < Props.Length; k++)
                {
                    if (oColumnas.Contains(Props[k].Name))
                    {
                        values[k] = Props[k].GetValue(Origen, null);//Insert values.
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(values[k] == null ? string.Empty : values[k].ToString());
                        oRow.AppendChild(cell);
                        AgregarElemento = true;
                    }
                }

                return AgregarElemento;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void CrearMensage(ref MemoryStream streambook, string Mensage, string TableName)
        {
            try
            {
                using (var workbook = SpreadsheetDocument.Create(streambook, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                    var stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    Stylesheet styles = new CustomStylesheet();
                    styles.Save(stylesPart);

                    List<string> oColumnas = ExtraerColumnas("Mensaje");
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    Sheets sheets = CrearHoja(workbook, TableName, ref sheetPart, ref sheetData);
                    Row headerRow = new Row();
                    CreateHeader(oColumnas, ref headerRow, ref sheetPart, 50, null);
                    sheetData.AppendChild(headerRow);
                    Row Rowitem = new Row();
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(Mensage);
                    Rowitem.AppendChild(cell);
                    sheetData.AppendChild(Rowitem);
                }
            }
            catch (Exception)
            { }
        }


        /// <summary>
        /// Devuelve un memory stream que contiene el formato de un libro de excel, con DocumentFormat.OpenXml
        /// </summary>
        /// <param name="ds"></param>
        /// <returns>Memory stream con formato de excel</returns>
        public static System.IO.MemoryStream ExportDataSet(DataSet ds)
        {

            System.IO.MemoryStream streambook = new System.IO.MemoryStream();
            using (var workbook = SpreadsheetDocument.Create(streambook, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
                var stylesPart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                Stylesheet styles = new CustomStylesheet();
                styles.Save(stylesPart);

                foreach (System.Data.DataTable table in ds.Tables)
                {

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();

                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                    AutoFilter autoFilter1 = new AutoFilter() { Reference = "A1:C1" };


                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        cell.StyleIndex = 11;
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);
                    sheetPart.Worksheet.Append(autoFilter1);
                    foreach (System.Data.DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
            }
            return streambook;
        }

        private static SheetData CreateSheetData<T>(List<T> objects,
                      List<string> headerNames)
        {
            var sheetData = new SheetData();
            if (objects != null)
            {
                //Get fields names of object
                List<string> fields = GetPropertyInfo<T>();
                //Get a list of A to Z
                var az = new List<Char>(Enumerable.Range('A', 'Z' -
                                      'A' + 1).Select(i => (Char)i).ToArray());
                //A to E number of columns 
                List<Char> headers = az.GetRange(0, fields.Count);
                int numRows = objects.Count;
                int numCols = fields.Count;
                var header = new Row();
                int index = 1;
                header.RowIndex = (uint)index;
                for (int col = 0; col < numCols; col++)
                {
                    var c = new HeaderCell(headers[col].ToString(),
                                           headerNames[col], index);
                    header.Append(c);
                }
                sheetData.Append(header);
                for (int i = 0; i < numRows; i++)
                {
                    index++;
                    var obj1 = objects[i];
                    var r = new Row { RowIndex = (uint)index };
                    for (int col = 0; col < numCols; col++)
                    {
                        string fieldName = fields[col];
                        PropertyInfo myf = obj1.GetType().GetProperty(fieldName);
                        if (myf != null)
                        {
                            object obj = myf.GetValue(obj1, null);
                            if (obj != null)
                            {
                                if (obj.GetType() == typeof(string))
                                {
                                    var c = new TextCell(headers[col].ToString(),
                                                obj.ToString(), index);
                                    r.Append(c);
                                }
                                else if (obj.GetType() == typeof(bool))
                                {
                                    string value =
                                      (bool)obj ? "Yes" : "No";
                                    var c = new TextCell(headers[col].ToString(),
                                                         value, index);
                                    r.Append(c);
                                }
                                else if (obj.GetType() == typeof(DateTime))
                                {
                                    var c = new DateCell(headers[col].ToString(),
                                               (DateTime)obj, index);
                                    r.Append(c);
                                }
                                else if (obj.GetType() == typeof(decimal) ||
                                         obj.GetType() == typeof(double))
                                {
                                    var c = new FormatedNumberCell(
                                                 headers[col].ToString(),
                                                 obj.ToString(), index);
                                    r.Append(c);
                                }
                                else
                                {
                                    long value;
                                    if (long.TryParse(obj.ToString(), out value))
                                    {
                                        var c = new NumberCell(headers[col].ToString(),
                                                    obj.ToString(), index);
                                        r.Append(c);
                                    }
                                    else
                                    {
                                        var c = new TextCell(headers[col].ToString(),
                                                    obj.ToString(), index);
                                        r.Append(c);
                                    }
                                }
                            }
                        }
                    }
                    sheetData.Append(r);
                }
                index++;
                Row total = new Row();
                total.RowIndex = (uint)index;
                for (int col = 0; col < numCols; col++)
                {
                    var obj1 = objects[0];
                    string fieldName = fields[col];
                    PropertyInfo myf = obj1.GetType().GetProperty(fieldName);
                    if (myf != null)
                    {
                        object obj = myf.GetValue(obj1, null);
                        if (obj != null)
                        {
                            if (col == 0)
                            {
                                var c = new TextCell(headers[col].ToString(),
                                                     "Total", index);
                                c.StyleIndex = 10;
                                total.Append(c);
                            }
                            else if (obj.GetType() == typeof(decimal) ||
                                     obj.GetType() == typeof(double))
                            {
                                string headerCol = headers[col].ToString();
                                string firstRow = headerCol + "2";
                                string lastRow = headerCol + (numRows + 1);
                                string formula = "=SUM(" + firstRow + " : " + lastRow + ")";
                                //Console.WriteLine(formula);
                                var c = new FomulaCell(headers[col].ToString(),
                                                       formula, index);
                                c.StyleIndex = 9;
                                total.Append(c);
                            }
                            else
                            {
                                var c = new TextCell(headers[col].ToString(),
                                                     string.Empty, index);
                                c.StyleIndex = 10;
                                total.Append(c);
                            }
                        }
                    }
                }
                sheetData.Append(total);
            }
            return sheetData;
        }
        private static List<string> GetPropertyInfo<T>()
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            // write property names
            return propertyInfos.Select(propertyInfo => propertyInfo.Name).ToList();
        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static Dictionary<string, string> ConvertModelToDictionary(object obj, string columnasNuevas)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            var columsNuevas = columnasNuevas.Split('|');
            int i = 0;
            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                string propName = prop.Name;
                if (i < columsNuevas.Count())
                {
                    ret.Add(propName, columsNuevas[i].ToString());
                    i++;
                }
            }

            return ret;
        }
    }

    /// <summary>
    /// Clase para generar la estructura de estilos para un libro de excel
    /// </summary>
    public class CustomStylesheet : Stylesheet
    {
        /// <summary>
        /// Constructor que inicializa la hoja de estilos del libro de excel, se basa en incides de estilos.
        /// </summary>
        public CustomStylesheet()
        {
            var fonts = new Fonts();
            var font = new DocumentFormat.OpenXml.Spreadsheet.Font();
            var fontName = new FontName { Val = StringValue.FromString("Arial") };
            var fontSize = new FontSize { Val = DoubleValue.FromDouble(11) };
            font.FontName = fontName;
            font.FontSize = fontSize;
            fonts.Append(font);
            //Font Index 1
            font = new DocumentFormat.OpenXml.Spreadsheet.Font();
            fontName = new FontName { Val = StringValue.FromString("Arial") };
            fontSize = new FontSize { Val = DoubleValue.FromDouble(12) };
            font.FontName = fontName;
            font.FontSize = fontSize;
            font.Bold = new Bold();
            fonts.Append(font);
            fonts.Count = UInt32Value.FromUInt32((uint)fonts.ChildElements.Count);

            //Font Index 2
            font = new DocumentFormat.OpenXml.Spreadsheet.Font();
            fontName = new FontName { Val = StringValue.FromString("Arial") };
            fontSize = new FontSize { Val = DoubleValue.FromDouble(10) };
            font.FontName = fontName;
            font.FontSize = fontSize;



            var col = new PatternFill
            {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor()
            };
            col.ForegroundColor =
               TranslateForeground(System.Drawing.Color.White);

            font.Color = new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = col.ForegroundColor.Rgb };
            font.Bold = new Bold();
            fonts.Append(font);
            fonts.Count = UInt32Value.FromUInt32((uint)fonts.ChildElements.Count);


            var fills = new Fills();
            var fill = new Fill();
            var patternFill = new PatternFill { PatternType = PatternValues.None };
            fill.PatternFill = patternFill;
            fills.Append(fill);
            fill = new Fill();
            patternFill = new PatternFill { PatternType = PatternValues.Gray125 };
            fill.PatternFill = patternFill;
            fills.Append(fill);
            //Fill index  2
            fill = new Fill();
            patternFill = new PatternFill
            {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor()
            };
            patternFill.ForegroundColor =
               TranslateForeground(System.Drawing.Color.LightBlue);
            patternFill.BackgroundColor =
                new BackgroundColor { Rgb = patternFill.ForegroundColor.Rgb };
            fill.PatternFill = patternFill;
            fills.Append(fill);
            //Fill index  3
            fill = new Fill();
            patternFill = new PatternFill
            {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor()
            };
            patternFill.ForegroundColor =
               TranslateForeground(System.Drawing.Color.DodgerBlue);
            patternFill.BackgroundColor =
               new BackgroundColor { Rgb = patternFill.ForegroundColor.Rgb };
            fill.PatternFill = patternFill;
            fills.Append(fill);
            fills.Count = UInt32Value.FromUInt32((uint)fills.ChildElements.Count);


            //Fill index  4
            fill = new Fill();
            patternFill = new PatternFill
            {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor()
            };
            patternFill.ForegroundColor =
               TranslateForeground(System.Drawing.Color.AliceBlue);
            patternFill.BackgroundColor =
               new BackgroundColor { Rgb = patternFill.ForegroundColor.Rgb };
            fill.PatternFill = patternFill;
            fills.Append(fill);
            fills.Count = UInt32Value.FromUInt32((uint)fills.ChildElements.Count);

            var borders = new Borders();
            var border = new Border
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            };
            borders.Append(border);
            //All Boarder Index 1
            border = new Border
            {
                LeftBorder = new LeftBorder { Style = BorderStyleValues.Thin },
                RightBorder = new RightBorder { Style = BorderStyleValues.Thin },
                TopBorder = new TopBorder { Style = BorderStyleValues.Thin },
                BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin },
                DiagonalBorder = new DiagonalBorder()
            };
            borders.Append(border);
            //Top and Bottom Boarder Index 2
            border = new Border
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder { Style = BorderStyleValues.Thin },
                BottomBorder = new BottomBorder { Style = BorderStyleValues.Thin },
                DiagonalBorder = new DiagonalBorder()
            };
            borders.Append(border);
            borders.Count = UInt32Value.FromUInt32((uint)borders.ChildElements.Count);
            var cellStyleFormats = new CellStyleFormats();
            var cellFormat = new CellFormat
            {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
            };
            cellStyleFormats.Append(cellFormat);
            cellStyleFormats.Count =
               UInt32Value.FromUInt32((uint)cellStyleFormats.ChildElements.Count);
            uint iExcelIndex = 164;
            var numberingFormats = new NumberingFormats();
            var cellFormats = new CellFormats();
            cellFormat = new CellFormat
            {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0
            };
            cellFormats.Append(cellFormat);
            var nformatDateTime = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                FormatCode = StringValue.FromString("dd/mm/yyyy hh:mm:ss")
            };
            numberingFormats.Append(nformatDateTime);
            var nformat4Decimal = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                FormatCode = StringValue.FromString("#,##0.0000")
            };
            numberingFormats.Append(nformat4Decimal);
            var nformat2Decimal = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                FormatCode = StringValue.FromString("#,##0.00")
            };
            numberingFormats.Append(nformat2Decimal);
            var nformatForcedText = new NumberingFormat
            {
                NumberFormatId = UInt32Value.FromUInt32(iExcelIndex),
                FormatCode = StringValue.FromString("@")
            };
            numberingFormats.Append(nformatForcedText);
            // index 1
            // Cell Standard Date format 
            cellFormat = new CellFormat
            {
                NumberFormatId = 14,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 2
            // Cell Standard Number format with 2 decimal placing
            cellFormat = new CellFormat
            {
                NumberFormatId = 4,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 3
            // Cell Date time custom format
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatDateTime.NumberFormatId,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 4
            // Cell 4 decimal custom format
            cellFormat = new CellFormat
            {
                NumberFormatId = nformat4Decimal.NumberFormatId,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 5
            // Cell 2 decimal custom format
            cellFormat = new CellFormat
            {
                NumberFormatId = nformat2Decimal.NumberFormatId,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 6
            // Cell forced number text custom format
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatForcedText.NumberFormatId,
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 7
            // Cell text with font 12 
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatForcedText.NumberFormatId,
                FontId = 1,
                FillId = 0,
                BorderId = 0,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 8
            // Cell text
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatForcedText.NumberFormatId,
                FontId = 0,
                FillId = 0,
                BorderId = 1,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 9
            // Coloured 2 decimal cell text
            cellFormat = new CellFormat
            {
                NumberFormatId = nformat2Decimal.NumberFormatId,
                FontId = 0,
                FillId = 2,
                BorderId = 2,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 10
            // Coloured cell text
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatForcedText.NumberFormatId,
                FontId = 0,
                FillId = 2,
                BorderId = 2,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            // Index 11
            // Coloured cell text
            cellFormat = new CellFormat
            {
                NumberFormatId = nformatForcedText.NumberFormatId,
                FontId = 2,
                FillId = 3,
                BorderId = 2,
                FormatId = 0,
                ApplyNumberFormat = BooleanValue.FromBoolean(true)
            };
            cellFormats.Append(cellFormat);
            numberingFormats.Count =
              UInt32Value.FromUInt32((uint)numberingFormats.ChildElements.Count);
            cellFormats.Count = UInt32Value.FromUInt32((uint)cellFormats.ChildElements.Count);
            this.Append(numberingFormats);
            this.Append(fonts);
            this.Append(fills);
            this.Append(borders);
            this.Append(cellStyleFormats);
            this.Append(cellFormats);
            var css = new CellStyles();
            var cs = new CellStyle
            {
                Name = StringValue.FromString("Normal"),
                FormatId = 0,
                BuiltinId = 0
            };
            css.Append(cs);
            css.Count = UInt32Value.FromUInt32((uint)css.ChildElements.Count);
            this.Append(css);
            var dfs = new DifferentialFormats { Count = 0 };
            this.Append(dfs);
            var tss = new TableStyles
            {
                Count = 0,
                DefaultTableStyle = StringValue.FromString("TableStyleMedium9"),
                DefaultPivotStyle = StringValue.FromString("PivotStyleLight16")
            };
            this.Append(tss);
        }
        private static ForegroundColor TranslateForeground(System.Drawing.Color fillColor)
        {
            return new ForegroundColor()
            {
                Rgb = new HexBinaryValue()
                {
                    Value =
                              System.Drawing.ColorTranslator.ToHtml(
                              System.Drawing.Color.FromArgb(
                                  fillColor.A,
                                  fillColor.R,
                                  fillColor.G,
                                  fillColor.B)).Replace("#", "")
                }
            };
        }
    }
    public class CustomColumn : Column
    {
        public CustomColumn(UInt32 startColumnIndex,
               UInt32 endColumnIndex, double columnWidth)
        {
            this.Min = startColumnIndex;
            this.Max = endColumnIndex;
            this.Width = columnWidth;
            this.CustomWidth = true;
        }
    }
    public class TextCell : Cell
    {
        public TextCell(string header, string text, int index)
        {
            this.DataType = CellValues.InlineString;
            this.CellReference = header + index;
            //Add text to the text cell.
            this.InlineString = new InlineString { Text = new Text { Text = text } };
        }
    }
    public class NumberCell : Cell
    {
        public NumberCell(string header, string text, int index)
        {
            this.DataType = CellValues.Number;
            this.CellReference = header + index;
            this.CellValue = new CellValue(text);
        }
    }
    public class FormatedNumberCell : NumberCell
    {
        public FormatedNumberCell(string header, string text, int index)
            : base(header, text, index)
        {
            this.StyleIndex = 2;
        }
    }
    public class DateCell : Cell
    {
        public DateCell(string header, DateTime dateTime, int index)
        {
            this.DataType = CellValues.Date;
            this.CellReference = header + index;
            this.StyleIndex = 1;
            this.CellValue = new CellValue { Text = dateTime.ToOADate().ToString() }; ;
        }
    }
    public class FomulaCell : Cell
    {
        public FomulaCell(string header, string text, int index)
        {
            this.CellFormula = new CellFormula { CalculateCell = true, Text = text };
            this.DataType = CellValues.Number;
            this.CellReference = header + index;
            this.StyleIndex = 2;
        }
    }
    public class HeaderCell : TextCell
    {
        public HeaderCell(string header, string text, int index) :
               base(header, text, index)
        {
            this.StyleIndex = 11;
        }
    }

    #region "Clase para Datatables y listas

    public class DatatableHelper
    {
        public static DataTable ToDataTable<T>(List<T> items, Boolean ExtraerDatos = true)
        {

            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);//Setting column names
            }
            if (ExtraerDatos)
            {
                try
                {
                    foreach (T item in items)
                    {
                        var values = new object[Props.Length];

                        for (int i = 0; i < Props.Length; i++)
                        {
                            values[i] = Props[i].GetValue(item, null);//Insert values.
                        }
                        dataTable.Rows.Add(values);
                    }
                }
                catch (Exception)
                { }
            }
            return dataTable;
        }
        #endregion
    }
}
