using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine.TypeAdapters
{
    public interface ITypeAdapter
    {
        object AdaptFormat(string data);
    }
}
