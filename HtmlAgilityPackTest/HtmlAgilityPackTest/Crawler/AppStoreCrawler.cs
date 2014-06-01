using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AppStore;
using Extensions;
using GooglePlay;
using HtmlAgilityPack;
using HtmlAgilityPackTest.Crawler;

namespace HtmlAgilityPackTest
{
    class AppStoreCrawler : MultithreadedCrawler
    {
        private readonly string _aggregatedDataFileName;
        private readonly string _errorsFileName;
        HashSet<string> urlsWithErrors = new HashSet<string>();
        int doc_id_counter = 0;
        private string path;

        public AppStoreCrawler(string seed_url, string path, int threads_number)
            : base(iosSeed, threads_number)
        {
            this.path = path;
            Directory.CreateDirectory(path);
            _aggregatedDataFileName = path + ".txt";
            _errorsFileName = path + "_errors.txt";
        }

        ProgressCounter progress = new ProgressCounter();
        List<IAppDataUS> apps = new List<IAppDataUS>(); //\\
        List<Exception> exceptions = new List<Exception>();

        protected override List<string> ProcessUrlAndGetLinks(WebClient webClient, string current_url, int processId)
        {
            //if (progress.Counter > 1000)
            //{
            //    return new List<string>();
            //}

            try
            {
                if (progress.Counter % 10 == 0)
                {
                    Console.WriteLine("progress.Counter = {0}, progress.CountPerSecond = {1}",
                        progress.Counter, progress.CountPerSecond());
                }
                progress.Increment();
                //Console.WriteLine("\n({0}, {1}) ID={3} download   {2}", visited_urls.Count, urls_to_visit.Count, current_url, processId);

                string html = webClient.DownloadString(current_url);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                HtmlNode root = document.DocumentNode;

                List<string> links =
                    root.SelectNodes("//a[@href]")
                        .Select(el => normalize_url(current_url, el.GetAttributeValue("href", null)))
                        .ToList();
                List<string> filtered_links = links.Where(good_url).ToList();

                IAppDataUS app = null;
                try // parse app
                {
                    Console.WriteLine(current_url);
                    app = new IAppDataUS(document, current_url); //\\
                    lock (apps)
                    {
                        apps.Add(app);
                    }
                    //Console.WriteLine(app.name + "   " + app.url);
                }
                catch (Exception e)
                {
                    AddException(e, current_url);
                }

                save_page_to_file(root, html, app);
                return filtered_links;
            }
            catch (Exception e)
            {
                AddException(e, current_url);
                return new List<string>();
            }
        }

        private void AddException(Exception e, string currentUrl)
        {
            lock (exceptions)
            {
                e.Data["url"] = currentUrl;
                exceptions.Add(e);
            }
        }

        protected override void OnFinished()
        {
            IAppDataUS.save_to_file(apps, _aggregatedDataFileName);
            SaveExceptions(_errorsFileName, exceptions);
        }

        protected void save_page_to_file(HtmlNode root, string html, IAppDataUS app)
        {
            string addition = "";
            if (app != null && app.url.StartsWith(iosGenreFilter))
            {
                addition = "GENRE:";
            }
            else if(app != null && app.url.StartsWith(iosAppFilter))
            {
                addition = "APP:";
            }

            int doc_id = Interlocked.Increment(ref doc_id_counter);
            string file_name = doc_id + " " + addition + " " + (app == null ? "" : app.name.MakeFileName()) + ".htm";
            string file_path = Path.Combine(path, file_name);
            File.WriteAllText(file_path, html, Encoding.Default);
        }

        protected string normalize_url(string base_url, string url_to_normalize)
        {
            if (url_to_normalize[0] == '#')
            {
            }

            string url = url_to_normalize == "#" ? "#" : new Uri(new Uri(base_url), url_to_normalize).AbsoluteUri;

            // experiment
            if(url.Contains("&amp;reviewId="))
            {
                string dbg = url.Split(new string[] { "&amp;reviewId=" }, StringSplitOptions.None)[0];
                return dbg;
            }

            return url;
        }

        private static string iosSeed = "http://itunes.apple.com/us/genre/mobile-software-applications/id36";
        private string iosGenreFilter = "https://itunes.apple.com/us/genre/ios";
        private string iosAppFilter = "https://itunes.apple.com/us/app/";

        protected bool good_url(string url)
        {
            if (url[0] == '#')
            {
                
            }
            return (url.StartsWith(iosGenreFilter) || url.StartsWith(iosAppFilter)) && url[0] != '#';
        }
    }
}