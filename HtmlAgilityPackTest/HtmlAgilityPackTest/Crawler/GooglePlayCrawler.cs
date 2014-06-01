using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using Extensions;
using GooglePlay;
using HtmlAgilityPack;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPackTest.Crawler;
using HtmlAgilityPackTest.GooglePLay;

namespace HtmlAgilityPackTest
{
    class GooglePlayCrawler : MultithreadedCrawler
    {
        string _appsFileName;
        string _developersFileName;
        string _appsPath;
        string _developersPath;
        string _exceptionsPath;
        string _videoPath;
        string _videoFileName;
        string _otherPagesPath;
        HashSet<string> urlsWithErrors = new HashSet<string>();
        int doc_id_counter = 0;

        public GooglePlayCrawler(string seed_url, string path, int threads_number) 
            : base(seed_url, threads_number)
        {
            _appsPath = path + "apps";
            _appsFileName = path + "apps.txt";
            _developersPath = path + "developers";
            _developersFileName = path + "developers.txt";
            _exceptionsPath = path + @"Exceptions/";
            _videoPath = path + "video";
            _videoFileName = path + "video.txt";
            _otherPagesPath = path + "other";
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(_appsPath);
            Directory.CreateDirectory(_developersPath);
            Directory.CreateDirectory(_exceptionsPath);
            Directory.CreateDirectory(_videoPath);
            Directory.CreateDirectory(_otherPagesPath);
        }

        ProgressCounter progress = new ProgressCounter();
        List<AppData> apps = new List<AppData>();
        List<DeveloperData> developers = new List<DeveloperData>();
        List<Exception> _exceptions = new List<Exception>();
        List<YoutubeData> _videos = new List<YoutubeData>();

        protected override List<string> ProcessUrlAndGetLinks(WebClient webClient, string current_url, int processId)
        {
            progress.Increment();
            progress.LogEvery(100);
            //Console.WriteLine("\n({0}, {1}) ID={3} download   {2}", visited_urls.Count, urls_to_visit.Count, current_url, processId);
            string stage = null;
            string html = null;
            List<string> filtered_links = null;
            try
            {
                stage = "downloading";
                html = webClient.DownloadString(current_url);
                HtmlDocument document = new HtmlDocument();

                stage = "loading_html_document";
                document.LoadHtml(html);
                HtmlNode root = document.DocumentNode;
                
                stage = "get_links";
                List<string> links =
                    root.SelectNodes("//a[@href]")
                        .Select(el => normalize_url(current_url, el.GetAttributeValue("href", null)))
                        .ToList();
                filtered_links = links.Where(good_url).ToList();

                if (AppData.IsAppUrl(current_url))
                {
                    stage = "parse_app";
                    AppData app = new AppData(document, current_url);
                    lock (apps)
                    {
                        apps.Add(app);
                    }
                    SavePage(_appsPath, html, app.Id);
                    stage = "parse_youtube_page";
                    ProcessVideoId(webClient, app.videoID);
                }
                else if (DeveloperData.IsDeveloperUrl(current_url))
                {
                    stage = "parse_developer";
                    DeveloperData developer = new DeveloperData(document, current_url);
                    lock (developers)
                    {
                        developers.Add(developer);
                    }
                    SavePage(_developersPath, html, developer.name);
                }
                else
                {
                    SavePage(_otherPagesPath, html);
                }
            }
            catch (Exception e)
            {
                AddException(e, current_url, stage);
                SaveFaultyPage(html, stage, e);
            }

            return filtered_links ?? new List<string>();
        }

        int _successfulVideoReloadings;
        int _unsuccessfulVideoReloadings;
        private void ProcessVideoId(WebClient webClient, string videoId)
        {
            if (videoId != "")
            {
                string url = YoutubeData.ConvertIdToUrl(videoId);
                string html = webClient.DownloadString(url);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                YoutubeData video = new YoutubeData(document, videoId);
                if (video.publishDate == "" || video.viewsNumber == -1)
                {
                    html = webClient.DownloadString(url);
                    document = new HtmlDocument();
                    document.LoadHtml(html);
                    video = new YoutubeData(document, videoId);
                    if (!(video.publishDate == "" || video.viewsNumber == -1))
                    {
                        Interlocked.Increment(ref _successfulVideoReloadings);
                    }
                    else
                    {
                        Interlocked.Increment(ref _unsuccessfulVideoReloadings);
                    }
                }

                lock (_videos)
                {
                    _videos.Add(video);
                }
                SavePage(_videoPath, html, videoId);
            }
        }

        void SaveFaultyPage(string html, string stage, Exception e)
        {
            string folderName = Path.Combine(_exceptionsPath, stage);
            try
            {
                SavePage(folderName, html);
            }
            catch (Exception)
            {
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                    SavePage(folderName, html);
                }
                else
                {
                    throw;
                }
            }
        }

        private void AddException(Exception e, string currentUrl, string stage)
        {
            lock (_exceptions)
            {
                e.Data["url"] = currentUrl;
                e.Data["stage"] = stage;
                _exceptions.Add(e);
            }
        }

        protected override void OnFinished()
        {
            AppData.save_to_file(apps, _appsFileName);
            DeveloperData.Serialize(developers, _developersFileName);
            SaveExceptions(_exceptionsPath, _exceptions);
            YoutubeData.save_to_file(_videos, _videoFileName);
        }

        public void SaveExceptions(string filePath, List<Exception> exceptions)
        {
            IEnumerable<IGrouping<string, Exception>> groups = exceptions.GroupBy(e => e.Data["stage"].ToString());

            foreach (IGrouping<string, Exception> group in groups)
            {
                using (StreamWriter file = new StreamWriter(Path.Combine(filePath, group.Key) + ".txt"))
                {
                    foreach (Exception exception in group)
                    {
                        file.WriteLine(exception.Message);
                        file.WriteLine(exception.StackTrace);

                        foreach (DictionaryEntry pair in exception.Data)
                        {
                            file.WriteLine(pair.Key + " = " + pair.Value);
                        }
                        file.WriteLine();
                        file.WriteLine();
                    }
                }
            }
        }

        //protected virtual string get_file_name_addition(HtmlNode root)
        //{
        //    HtmlNode name_node = root.SelectSingleNode("//div[@itemprop='name']/div");
        //    return name_node == null ? "" : name_node.InnerText;
        //}

        void SavePage(string path, string html, string name = "")
        {
            int doc_id = Interlocked.Increment(ref doc_id_counter);

            //string file_name_addition = get_file_name_addition(root).MakeFileName();
            string file_name = doc_id + " " + name + ".htm";
            string file_path = Path.Combine(path, file_name);

            File.WriteAllText(file_path, html, Encoding.Default);
        }

        protected string normalize_url(string base_url, string url_to_normalize)
        {
            string url = url_to_normalize == "#" ? "#" : new Uri(new Uri(base_url), url_to_normalize).AbsoluteUri;

            // experiment
            if(url.Contains("&amp;reviewId="))
            {
                string dbg = url.Split(new string[] { "&amp;reviewId=" }, StringSplitOptions.None)[0];
                return dbg;
            }

            return url;
        }

        protected bool good_url(string url)
        {
            return url.StartsWith(seed_url) && url[0] != '#';
        }
    }
}
