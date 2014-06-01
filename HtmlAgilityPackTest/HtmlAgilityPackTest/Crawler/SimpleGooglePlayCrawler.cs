using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HtmlAgilityPackTest
{
    class SimpleGooglePlayCrawler
    {
        HashSet<string> visited_urls = new HashSet<string>();
        HashSet<string> urls_to_visit = new HashSet<string>();
        WebClient web_client = new WebClient();
        string path;
        string seed_url;
        int doc_id_counter = 0;

        Dictionary<string, List<string>> fileName2Urls = new Dictionary<string, List<string>>();

        void print_doubles()
        {
            foreach (var pair in fileName2Urls)
            {
                if (pair.Value.Count > 1)
                {
                    foreach (string s in pair.Value)
                    {
                        Debug.WriteLine(s);
                    }
                    Debug.WriteLine("");
                }
            }
        }

        public SimpleGooglePlayCrawler(string seed_url, string path)
        {
            this.seed_url = seed_url;
            this.path = path;
            web_client.Proxy = null;
            web_client.Encoding = System.Text.Encoding.GetEncoding("utf-8");
        }

        public void run()
        {
            urls_to_visit.Add(seed_url);
            Directory.CreateDirectory(path);

            Crawling();
        }

        void Crawling()
        {
            int file_rewriting_counter = 0;

            while (urls_to_visit.Count != 0)
            {
                string current_url = urls_to_visit.First();
                urls_to_visit.Remove(current_url);

                if (!visited_urls.Contains(current_url) && filter_url(current_url))
                {
                    Console.WriteLine("\n({0}, {1}) download   {2}", visited_urls.Count, urls_to_visit.Count, current_url);

                    Debug.Print("url_to_visit = " + current_url);
                    try 
                    {
                        // load page
                        string html = web_client.DownloadString(current_url);

                        // parse for links
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(html);
                        HtmlNode root = document.DocumentNode;
                        List<string> links = root.SelectNodes("//a[@href]").Select(el => normalize_url(current_url, el.GetAttributeValue("href", null))).ToList();
                        List<string> filtered_links = links.Where(link => !urls_to_visit.Contains(link) && filter_url(link)).ToList();

                        // save page to file
                        string file_name = get_file_name(root);
                        if(file_name != null)
                        {
                            string file_path = Path.Combine(path, file_name);

                            if (File.Exists(file_path))
                            {
                                ++file_rewriting_counter;
                                Console.WriteLine("\nfile_rewriting_counter: " + file_rewriting_counter);

                                fileName2Urls[file_name].Add(current_url);
                            }
                            else
                            {
                                fileName2Urls.Add(file_name, new List<string> { current_url });
                            }

                            File.WriteAllText(file_path, html, Encoding.Default);
                        }

                        // add links
                        urls_to_visit.UnionWith(filtered_links);
                    }
                    catch (Exception ex) { }

                    visited_urls.Add(current_url);
                }
            }
        }

        private string normalize_url(string base_url, string url_to_normalize)
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

        protected virtual string get_file_name(HtmlNode root)
        {
            ++doc_id_counter;
            HtmlNode name_node = root.SelectSingleNode("//div[@itemprop='name']/div");
            if (name_node == null)
            {
                return null;
            }

            char[] characters_to_delete = Path.GetInvalidFileNameChars();
            string file_name = new string(name_node.InnerText.Where(ch => !characters_to_delete.Contains(ch)).ToArray());
            return doc_id_counter + file_name + ".htm";
        }

        protected virtual bool filter_url(string url)
        {
            return url.StartsWith(seed_url) && url[0] != '#';
        }
    }
}
