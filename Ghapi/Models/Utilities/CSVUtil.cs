using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.Utilities
{
    public class CSVUtil
    {
        public static string EscapeText(string text)
        {
            int number = 0;

            if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out number))
            {
                text = text.Replace("\"", "\"\"");
                text = "\"" + text + "\"";
            }

            return text;
        }
    }
}
