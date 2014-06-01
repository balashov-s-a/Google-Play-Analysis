using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPackTest;
using HtmlAgilityPackTest.GooglePLay;

namespace GooglePlay
{
    public class AppData
    {
        public string Id;
        public string name;
        public string genre;

        public double score;
        public int reviews_num;

        public double fileSize; //in megabytes
        public int numDownloads; // average
        public double price;

        public string url;
        public string videoID;

        public DeveloperData developer { get; set; }
        public YoutubeData video { get; set; }

        public double profit { get { return price * numDownloads; } }
        public bool IsPaid { get { return price > 0.001; } }

        public static char[] WordsSeparators = new char[] { ' ', ',', ';', '.', '!', '"', '(', ')', '?', ':', '-', '&'};
        public string[] NameWords 
        { 
            get{return name.Split(WordsSeparators, StringSplitOptions.RemoveEmptyEntries); }
        }

        static int to_int(string str)
        {
            return Convert.ToInt32(Regex.Replace(str, @"[^0-9]", ""));
        }

        private AppData() {}

        public AppData(HtmlDocument document, string url)
        {
            AppRawData app = new AppRawData(document);
            this.url = url;
            this.Id = url.Replace(AppUrlStart, "");
            name = app.name;
            genre = app.genre;

            score = Convert.ToDouble(app.score);
            reviews_num = to_int(app.reviews_num);

            if (app.fileSize.Contains("M"))
            {
                fileSize = Convert.ToDouble(app.fileSize.Replace("M", ""));
            }
            else if (app.fileSize.Contains("k"))
            {
                fileSize = Convert.ToDouble(app.fileSize.Replace("k", "")) / 1024;
            }
            else
            {
                fileSize = -1;
            }

            numDownloads = (int)app.numDownloads.Split('–').Select(num => to_int(num)).Average();

            // "Бесплатно" / "95,37 руб."
            string[] priceParts = app.price.Split();

            price = priceParts.Length == 1 ? 0 : Convert.ToDouble(priceParts[0]);
            videoID = app.videoID;
        }

        static FieldInfo[] fields = typeof(AppData).GetFields(BindingFlags.Public | BindingFlags.Instance); //
        string ToCsv() //
        {
            return String.Join(";", fields.Select(fi => fi.GetValue(this).ToString().Replace(";", ":")));
        }
        static string GetCsvHeader() //
        {
            return String.Join(";", fields.Select(fi => fi.Name));
        }

        public static void save_to_file(List<AppData> apps, string file_name) //
        {
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            using (StreamWriter file = new StreamWriter(file_name))
            {
                file.WriteLine(GetCsvHeader());
                foreach (AppData app in apps)
                {
                    file.WriteLine(app.ToCsv());
                }
            }

            Thread.CurrentThread.CurrentCulture = culture;
        }

        public static List<AppData> load_from_file(string file_name)
        {
            List<AppData> result = new List<AppData>();
            using (StreamReader file = new StreamReader(file_name))
            {
                string line, header;
                if ((line = file.ReadLine()) != null)
                {
                    header = line;
                }
                while ((line = file.ReadLine()) != null)
                {
                    AppData app = new AppData();
                    string[] parts = line.Split(';');
                    for (int i = 0; i < fields.Count(); ++i)
                    {
                        fields[i].SetValue(app, Convert.ChangeType(parts[i], fields[i].FieldType));
                    }
                    result.Add(app);
                }
            }
            return result;
        }

        const string AppUrlStart = @"https://play.google.com/store/apps/details?id=";
        public static bool IsAppUrl(string url)
        {
            return url.StartsWith(AppUrlStart);
        }
    }
}