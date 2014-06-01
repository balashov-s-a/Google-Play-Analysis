using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using HtmlAgilityPackTest;

namespace GooglePlay
{
    public class AppRawData
    {
        public string name;
        public string genre;
        
        public string score;
        public List<string> rating_histogram;
        public string reviews_num;
        
        public string datePublished;
        public string fileSize;
        public string numDownloads;
        public string softwareVersion;
        public string operatingSystems;
        public string contentRating;
        public string price;
        
        public string developer_site = "";
        public string developer_email = "";
        public string videoID = "";

        public AppRawData(HtmlDocument document)
        {
            HtmlNode root = document.DocumentNode;
            name = root.SelectSingleNode("//div[@itemprop='name']/div").InnerText;
            genre = root.SelectSingleNode("//span[@itemprop='genre']").InnerText;

            score = root.SelectSingleNode("//div[@class='score']").InnerText;
            reviews_num = root.SelectSingleNode("//span[@class='reviews-num']").InnerText;
            rating_histogram = root.SelectNodes("//div[@class='rating-histogram']/div/span[@class='bar-number']").Select(el => el.InnerText).ToList();

            datePublished = root.SelectSingleNode("//div[@itemprop='datePublished']").InnerText;
            fileSize = root.SelectSingleNode("//div[@itemprop='fileSize']").InnerText;
            softwareVersion = root.SelectSingleNode("//div[@itemprop='softwareVersion']").InnerText;
            operatingSystems = root.SelectSingleNode("//div[@itemprop='operatingSystems']").InnerText;
            contentRating = root.SelectSingleNode("//div[@itemprop='contentRating']").InnerText;
            price = root.SelectSingleNode("//meta[@itemprop='price']").GetAttributeValue("content", null);   //<meta itemprop="price" content="Бесплатно">

            HtmlNode numDownloads_node = root.SelectSingleNode("//div[@itemprop='numDownloads']");
            numDownloads = numDownloads_node == null ? "0" : numDownloads_node.InnerText;

            HtmlNodeCollection developer_references = root.SelectNodes("//a[@class='dev-link']");
            foreach (HtmlNode ref_node in developer_references)
            {
                string link = ref_node.GetAttributeValue("href", null);
                if (link.StartsWith("mailto:"))
                {
                    developer_email = link;
                }
                else
                {
                    developer_site = link;
                }
            }

            //    //span[@class='play-action-container']               data-video-url
            string videoUrl = null;
            try
            {
                videoUrl = root.SelectSingleNode("//span[@data-video-url]").GetAttributeValue("data-video-url", null);
            }
            catch
            {
            }

            if (videoUrl != null)
            {
                if (new Uri(videoUrl).Host != "www.youtube.com")
                {
                    throw new Exception("video host : " + new Uri(videoUrl).Host);
                }
                videoID = YoutubeData.GetVideoID(videoUrl);
                if (videoID.Contains(":") || videoID.Contains(";"))
                {
                    throw new Exception(": or ; in videoId");
                }
            }
        }
    }
}
