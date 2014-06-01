using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace HtmlAgilityPackTest
{
    abstract class MultithreadedCrawler
    {
        protected HashSet<string> visited_urls = new HashSet<string>();
        protected HashSet<string> urls_to_visit = new HashSet<string>();
        protected string seed_url;
        private int threads_number;

        public MultithreadedCrawler(string seed_url, int threads_number)
        {
            this.seed_url = seed_url;
            this.threads_number = threads_number;
        }

        public void Run()
        {
            urls_to_visit.Add(seed_url);

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threads_number; ++i)
            {
                WebClient web_client = new WebClient() { Proxy = null, Encoding = Encoding.GetEncoding("utf-8") };
                int processID = i;
                Thread thread = new Thread(() => Crawling(web_client, processID));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            OnFinished();
        }
        
        private volatile bool finished = false;
        private Object _o = new object();
        private void Crawling(WebClient webClient, int processId)
        {
            while (true)
            {
                if (finished)
                {
                    break;
                }

                bool isLastUrlPopped = false;
                string current_url = null;
                int urls_to_visit_count;
                lock (_o)
                {
                    urls_to_visit_count = urls_to_visit.Count;
                    if (urls_to_visit_count > 0)
                    {
                        current_url = urls_to_visit.First();
                        urls_to_visit.Remove(current_url);
                        visited_urls.Add(current_url);

                        if (urls_to_visit.Count == 0)
                        {
                            isLastUrlPopped = true;
                        }
                    }
                }

                if (urls_to_visit_count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                List<string> linksList = ProcessUrlAndGetLinks(webClient, current_url, processId);
                HashSet<string> links = new HashSet<string>(linksList);

                lock (_o)
                {
                    links.ExceptWith(visited_urls);
                    urls_to_visit.UnionWith(links);
                    if (isLastUrlPopped && urls_to_visit.Count == 0)
                    {
                        finished = true;
                    }
                }
            }
        }

        private void OldCrawlingWithBugs(WebClient webClient, int processId)
        {
            while (true)
            {
                if (finished)
                {
                    break;
                }

                bool isLastUrlPopped = false;
                string current_url;
                lock (urls_to_visit)
                {
                    if (urls_to_visit.Count == 0)
                    {
                        continue;
                    }

                    current_url = urls_to_visit.First();
                    urls_to_visit.Remove(current_url);

                    if (urls_to_visit.Count == 0)
                    {
                        isLastUrlPopped = true;
                    }
                }

                if (!visited_urls.Contains(current_url))
                {
                    List<string> filteredLinks = ProcessUrlAndGetLinks(webClient, current_url, processId);
                    if (filteredLinks != null)
                    {
                        lock (urls_to_visit)
                        {
                            //if (isLastUrlPopped && urls_to_visit.Intersect(filteredLinks).Count() == 0)
                            //{
                            //    finished = true;
                            //}

                            urls_to_visit.UnionWith(filteredLinks);
                        }
                    }

                    lock (visited_urls)
                    {
                        visited_urls.Add(current_url);
                    }
                }
                Thread.Sleep(1);
            }
        }

        protected abstract List<string> ProcessUrlAndGetLinks(WebClient webClient, string current_url, int processId);
        protected abstract void OnFinished();

        public static void SaveExceptions(string fileName, List<Exception> exceptions)
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                foreach (var exception in exceptions)
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
}