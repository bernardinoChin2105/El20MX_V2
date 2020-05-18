using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelEngine.TypeAdapters
{
    public class YesNoBoleanAdapter : ITypeAdapter
    {
        protected string AfirmativeWord { get; set; }
        protected string NegativeWord { get; set; }

        public YesNoBoleanAdapter()
        {
            this.AfirmativeWord = "yes";
            this.NegativeWord = "no";
        }
        public virtual object AdaptFormat(string dataValue)
        {
            if (AfirmativeWord.Equals(dataValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            if (NegativeWord.Equals(dataValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return null;
        }
    }
}
