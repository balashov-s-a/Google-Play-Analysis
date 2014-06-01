using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GooglePlay
{
    static class YoutubeHelper
    {
        public static string TransformSimple(string url)
        {
            return "https://www.youtube.com/watch?v=" + new Uri(url).Segments.Last() + "#t=0&autoplay=0";
        }
    }
}
