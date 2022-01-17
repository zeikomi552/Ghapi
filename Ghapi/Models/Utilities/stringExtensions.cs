using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghapi.Models.Utilities
{
    public static class stringExtensions
    {
        public static string EmptyToText(this string str, string replace_text)
        {
            if (string.IsNullOrEmpty(str))
            {
                return replace_text;
            }
            else
            {
                return str;
            }
        }

        public static string CutText(this string str, int length)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > length)
            {
                return str.Substring(0, length) + "...";
            }
            else
            {
                return str;
            }

        }

        public static string EscapeText(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                Encoding Utf8Encoder = Encoding.GetEncoding(
                    "UTF-8",
                    new EncoderReplacementFallback(string.Empty),
                    new DecoderExceptionFallback()
                );

                var utf8Text = Utf8Encoder.GetString(Utf8Encoder.GetBytes(str));

                return utf8Text.Replace("|", "\\/");
                //return str.Replace("<", "&lt;")
                //    .Replace(">", "&gt;")
                //    .Replace("'", "&#39;")
                //    .Replace("\"", "&quot;")
                //    .Replace("&", "&amp;")
                //    .Replace(" ", "&nbsp;")
                //    .Replace("\\", "&yen;")
                //    .Replace("¢", "&cent;")
                //    .Replace("£", "&pound;")
                //    .Replace("¦", "&brvbar;")
                //    .Replace("©", "&copy;")
                //    .Replace("®", "&reg;")
                //    .Replace("°", "&deg;")
                //    .Replace("±", "&plusmn;")
                //    .Replace("×", "&times;")
                //    .Replace("÷", "&divide;")
                //    .Replace("µ", "&micro;")
                //    .Replace("·", "&middot;")
                //    .Replace("§", "&sect;")
                //    .Replace("«", "&laquo;")
                //    .Replace("»", "&raquo;")
                //    .Replace("²", "&sup2;")
                //    .Replace("³", "&sup3;")
                //    .Replace("¹", "&sup1;")
                //    .Replace("¼", "&frac14;")
                //    .Replace("½", "&frac12;")
                //    .Replace("¾", "&frac34;");
            }
            else
            {
                return string.Empty;
            }

        }
    }
}
