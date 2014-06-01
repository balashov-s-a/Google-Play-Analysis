using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GooglePlay;
using HtmlAgilityPack;

namespace AppStore
{
    public class IAppDataUS
    {
        static public string ExampleUrl = @"https://itunes.apple.com/us/app/super-hexagon/id549027629?mt=8";
        static public string ExampleUrl2 = @"https://itunes.apple.com/us/app/doodle-jump/id307727765?mt=8";

        public string name;
        public string seller;

        public string genre;
        public decimal price;
        public DateTime releaseDate;
        public string rated;
        public string currentVersionRating;
        public string allVersionsRating;
        public decimal fileSize;
        public string version;
        public string url;

        public IAppDataUS(HtmlDocument document, string url)
        {
            this.url = url;
            HtmlNode root = document.DocumentNode;

            name = root.SelectSingleNode("//div[@class='left']/h1").InnerText;
            string priceString = root.SelectSingleNode("//div[@class='price']").InnerText;
            genre = root.SelectSingleNode("//li[@class='genre']/a").InnerText;

            string releaseDateString = root.SelectSingleNode("//li[@class='release-date']/text()").InnerText;
            releaseDate = DateTime.Parse(releaseDateString);
            version = root.SelectSingleNode("//li[span = 'Version: ']/text()").InnerText;
            string sizeString =    root.SelectSingleNode("//li[span = 'Size: ']/text()").InnerText;
            seller =  root.SelectSingleNode("//li[span = 'Seller: ']/text()").InnerText;

            rated = root.SelectSingleNode("//div[@class='app-rating']/a").InnerText;

            string[] ratings = root.SelectNodes("//span[@class='rating-count']").Select(el => el.InnerText).ToArray();
            currentVersionRating = ratings[0];
            allVersionsRating = ratings[1];


            
            if (sizeString.Contains("M"))
            {
                fileSize = Convert.ToDecimal(sizeString.Replace("MB", ""));
            }
            //else if (sizeString.Contains("k"))
            //{
            //    fileSize = Convert.ToDecimal(sizeString.Replace("k", "")) / 1024;
            //}
            else
            {
                throw new Exception("invalid size format");
                fileSize = -1;
            }

            if (priceString == "Free")
            {
                price = 0;
            }
            else if(priceString[0] == '$')
            {
                price = Convert.ToDecimal(priceString.Substring(1));
            }
            else
            {
                throw new Exception("currency symbol != $");
            }
        }

        static FieldInfo[] fields = typeof(IAppDataUS).GetFields(BindingFlags.Public | BindingFlags.Instance);

        private IAppDataUS()
        {
        }

        string ToCsv()
        {
            return String.Join(";", fields.Select(fi => fi.GetValue(this).ToString().Replace(";", ":")));
        }
        static string GetCsvHeader()
        {
            return String.Join(";", fields.Select(fi => fi.Name));
        }

        public static void save_to_file(List<IAppDataUS> apps, string file_name)
        {
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            using (StreamWriter file = new StreamWriter(file_name))
            {
                file.WriteLine(GetCsvHeader());
                foreach (IAppDataUS app in apps)
                {
                    file.WriteLine(app.ToCsv());
                }
            }

            Thread.CurrentThread.CurrentCulture = culture;
        }

        public static List<IAppDataUS> load_from_file(string file_name)
        {
            List<IAppDataUS> result = new List<IAppDataUS>();
            using (StreamReader file = new StreamReader(file_name))
            {
                string line, header;
                if ((line = file.ReadLine()) != null)
                {
                    header = line;
                }
                while ((line = file.ReadLine()) != null)
                {
                    IAppDataUS app = new IAppDataUS();
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
    }
}
