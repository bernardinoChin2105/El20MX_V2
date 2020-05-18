using ExcelEngine.TypeAdapters;
using System;


namespace ExcelEngine
{

    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumn : Attribute
    {
        public int Column { get; set; }
        public string Letter { get; set; }

        public Type Adapter { get; set; }

    }
}