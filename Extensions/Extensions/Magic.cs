using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQPad;

namespace Extensions
{
    public static class Magic
    {
        // Linq dump in browser
        // http://stackoverflow.com/questions/6032908/is-there-a-library-that-provides-a-formatted-dump-function-like-linqpad
        public static T Dump<T>(this T o, int maxDepth = 5)
        {
            var localUrl = Path.GetTempFileName() + ".html";
            using (var writer = LINQPad.Util.CreateXhtmlWriter(true, maxDepth))
            {
                writer.Write(o);
                File.WriteAllText(localUrl, writer.ToString());
            }
            Process.Start(localUrl);
            return o;
        }
    }
}