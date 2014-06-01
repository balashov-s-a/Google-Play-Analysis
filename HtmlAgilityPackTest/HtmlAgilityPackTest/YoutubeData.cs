using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GooglePlay;
using HtmlAgilityPack;

namespace HtmlAgilityPackTest
{
    public class YoutubeData
    {
        public YoutubeData()
        {
        }

        public string videoId = "";
        public string name = "";
        public string publishDate = "";
        public int viewsNumber = -1;
        public int likesCount = -1;
        public int dislikesCount = -1;
        public AppData app { get; set; }

        static CultureInfo ruCulture = CultureInfo.CreateSpecificCulture("ru");
        public DateTime date
        {
            get
            {
                if (publishDate == "")
                {
                    return DateTime.MinValue;
                }

                // "04 Фев 2014 г."
                // "12 янв. 2014 г."
                // "27 нояб. 2012 г."
                // "23 июля 2009 г."
                // "30 марта 2012 г."
                // DateTime test = DateTime.ParseExact("27 нояб 2012 г", "dd MMMM yyyy г", ruCulture); // cant parse!

                string str = publishDate.Replace(".", "");
                string format = "dd MMM yyyy г";

                if(str.Length > format.Length)
                {
                    int startIndex = 6;
                    int count = str.Length - format.Length;
                    str = str.Remove(startIndex, count);
                }

                str = str.Replace("мая", "Май");

                return DateTime.ParseExact(str, format, ruCulture);
            }
        }
        public YoutubeData(HtmlDocument document, string videoId)
        {
            this.videoId = videoId;
            HtmlNode root = document.DocumentNode;

            try
            {
                name = root.SelectSingleNode("id('eow-title')").InnerText.Trim();
                publishDate = root.SelectSingleNode("id('eow-date')").InnerText; // "ch-description-clip\">\n          <p id=\"watch-uploader-info\">\n              <strong>Опубликовано: 04 янв. 2013 г.</strong>\n          </p>\n          <di"
            }
            catch (Exception e)
            {
            }
            //"905 просмотров"
            try // &nbsp;
            {
                string viewsString = root.SelectSingleNode("//span[@class='watch-view-count ']").InnerText.Trim();
                viewsString = new string(viewsString.Where(char.IsDigit).ToArray());
                //viewsString = viewsString.Replace("просмотров", "").viewsString.Replace("просмотра", "").Replace(" ", "");
                viewsNumber = Convert.ToInt32(viewsString);
            }
            catch (Exception e)
            {
            }

            try
            {
                likesCount = Convert.ToInt32(root.SelectSingleNode("//span[@class='likes-count']").InnerText.Replace(" ", ""));
                dislikesCount = Convert.ToInt32(root.SelectSingleNode("//span[@class='dislikes-count']").InnerText.Replace(" ", ""));
            }
            catch
            {
            }
        }

        public static string ConvertIdToUrl(string id)
        {
            return "https://www.youtube.com/watch?v=" + id + "#t=0";
        }

        public static string TransformSimple(string url)
        {
            return "https://www.youtube.com/watch?v=" + GetVideoID(url) + "#t=0";
        }

        public static string GetVideoID(string url)
        {
            return new Uri(url).Segments.Last();
        }

        static FieldInfo[] fields = typeof(YoutubeData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        string ToCsv() //
        {
            return String.Join(";", fields.Select(fi => fi.GetValue(this).ToString().Replace(";", ":")));
        }
        static string GetCsvHeader() //
        {
            return String.Join(";", fields.Select(fi => fi.Name));
        }

        public static void save_to_file(List<YoutubeData> videos, string file_name)
        {
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            using (StreamWriter file = new StreamWriter(file_name))
            {
                file.WriteLine(GetCsvHeader());
                foreach (YoutubeData video in videos)
                {
                    file.WriteLine(video.ToCsv());
                }
            }

            Thread.CurrentThread.CurrentCulture = culture;
        }

        public static List<YoutubeData> load_from_file(string file_name)
        {
            List<YoutubeData> result = new List<YoutubeData>();
            using (StreamReader file = new StreamReader(file_name))
            {
                string line, header;
                if ((line = file.ReadLine()) != null)
                {
                    header = line;
                }
                while ((line = file.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == fields.Count())
                    {
                        YoutubeData video = new YoutubeData();

                        for (int i = 0; i < fields.Count(); ++i)
                        {
                            fields[i].SetValue(video, Convert.ChangeType(parts[i], fields[i].FieldType));
                        }
                        result.Add(video);
                    }
                    else
                    {
                        Debug.Print("YoutubeData: parts.Length != fields.Count()");
                    }
                }
            }
            return result;
        }

        public static void Test(WebClient webClient)
        {
            string url = @"https://www.youtube.com/watch?v=9bZkp7q19f0";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webClient.DownloadString(url));
            YoutubeData youtubeData = new YoutubeData(doc, url);
        }
    }
}
