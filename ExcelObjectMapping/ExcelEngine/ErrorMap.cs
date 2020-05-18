

namespace ExcelEngine
{
    public class ErrorMap
    {
        public int Line{ get; set; }
        public string Description { get; set; }
        public string ColumnLetter { get; set; }
        public int ExcelLine { get; set; }
        public int Column { get; set; }
        public int ExcelColumn{ get; set; }
    }
}