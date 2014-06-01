using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlAgilityPackTest.Crawler
{
    static class Helper
    {
        static char[] invalid_file_name_chars = Path.GetInvalidFileNameChars();

        public static string MakeFileName(this string str)
        {
            return new string(str.Where(ch => !invalid_file_name_chars.Contains(ch)).ToArray());
        }
    }
}
