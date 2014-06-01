using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using AppStore;
using Extensions;
using GooglePlay;
using HtmlAgilityPack;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;
using HtmlAgilityPackTest.GooglePLay;

namespace HtmlAgilityPackTest
{
    class Program
    {
        static void AggregeteFolder(string folderPath, string fileName, string errorsFileName)
        {
            ProgressCounter progress = new ProgressCounter();
            List<AppData> apps = new List<AppData>();
            List<Exception> exceptions = new List<Exception>();
            foreach (string app_file_name in Directory.EnumerateFiles(folderPath, "*.htm"))
            {
                progress.LogEvery(10);
                progress.Increment();

                try
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(File.ReadAllText(app_file_name));
                    AppData appData = new AppData(document, "");
                    apps.Add(appData);
                }
                catch (Exception e)
                {
                    e.Data["path"] = app_file_name;
                    exceptions.Add(e);
                    Console.WriteLine("errors: {0}", exceptions.Count());
                }
            }

            MultithreadedCrawler.SaveExceptions(errorsFileName, exceptions);
            AppData.save_to_file(apps, fileName);
        }

        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10000;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            WebClient web_client = new WebClient() { Encoding = Encoding.GetEncoding("utf-8") };

            //YoutubeData.Test(web_client);

            GooglePlayCrawler crawler = 
                new GooglePlayCrawler("https://play.google.com/store/apps/", @"e:/Google Play 3/", 50);
            crawler.Run();
        }

        //private static void RunAppStoreCrawler()
        //{
        //    Debug.Assert(Thread.CurrentThread.CurrentCulture == CultureInfo.InvariantCulture);
        //    AppStoreCrawler crawler =
        //        new AppStoreCrawler("", base_path + @"/app_store", 9);
        //    crawler.Run();
        //}
    }
}
