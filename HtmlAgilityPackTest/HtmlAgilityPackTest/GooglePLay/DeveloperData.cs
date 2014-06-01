using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GooglePlay;
using HtmlAgilityPack;
using MoreLinq;

namespace HtmlAgilityPackTest.GooglePLay
{
    [Serializable]
    public class DeveloperData
    {
        public DeveloperData()
        {
        }

        public string name;
        public string url;
        public List<string> appsNames;
        public List<string> appsIDs;

        [XmlIgnore]
        public List<AppData> apps = new List<AppData>();
        public DeveloperData(HtmlDocument document, string url)
        {
            this.url = url;
            HtmlNode root = document.DocumentNode;

            name = root.SelectSingleNode("//h1[@class='cluster-heading']").InnerText.Trim();

            HtmlNodeCollection nodes = root.SelectNodes("//div[@class='card-list']//a[@class='title']");
            appsNames = nodes.Select(el => el.InnerText.Trim()).ToList();
            List<string> shortenUrls = nodes.Select(el => el.GetAttributeValue("href", null)).ToList();

            if (shortenUrls.All(el => el.StartsWith(@"/store/apps/details?id=")))
            {
                appsIDs = shortenUrls.Select(el => el.Replace(@"/store/apps/details?id=", "")).ToList();
            }
            else if (shortenUrls.All(el => el.StartsWith(@"https://play.google.com/store/apps/details?id=")))
            {
                appsIDs = shortenUrls.Select(el => el.Replace(@"https://play.google.com/store/apps/details?id=", "")).ToList();
            }
            else
            {
                throw new Exception("incorrect developer apps IDs");
            }
        }

        public static XmlSerializer serializer = new XmlSerializer(typeof(List<DeveloperData>));

        public static void Serialize(List<DeveloperData> developers, string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(file, developers);
            }
        }

        public static List<DeveloperData> Deserialize(string fileName)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return (List<DeveloperData>)serializer.Deserialize(file);
            }
        }

        const string DeveloperUrlStart = @"https://play.google.com/store/apps/developer?id=";
        public static bool IsDeveloperUrl(string url)
        {
            return url.StartsWith(DeveloperUrlStart);
        }

        public List<DeveloperVote> GetVotes()
        {
            List<DeveloperVote> votes = new List<DeveloperVote>();
            HashSet<string> familiarGenres = new HashSet<string>();

            IOrderedEnumerable<IGrouping<DateTime, AppData>> appGroups = apps.Where(a => a.video != null && a.video.date != DateTime.MinValue)
                                                                             .GroupBy(a => a.video.date)
                                                                             .OrderBy(g => g.Key);
            foreach (IGrouping<DateTime, AppData> group in appGroups)
            {
                HashSet<string> newGenres = group.Select(a => a.genre).ToHashSet();
                HashSet<string> positiveGenres = familiarGenres.Intersect(newGenres).ToHashSet();
                HashSet<string> negativeGenres = familiarGenres.Except(positiveGenres).ToHashSet();
                DateTime groupDate = group.Key;

                if (positiveGenres.Count > 0 && negativeGenres.Count > 0)
                {
                    foreach (string positiveGenre in positiveGenres)
                    {
                        votes.Add(new DeveloperVote(this, positiveGenre, groupDate, negativeGenres.Count));
                    }

                    foreach (string negativeGenre in negativeGenres)
                    {
                        votes.Add(new DeveloperVote(this, negativeGenre, groupDate, -positiveGenres.Count));
                    }
                }

                familiarGenres.UnionWith(newGenres);
            }

            //if (votes.Count > 0)
            //{
            //    appGroups.GroupJoin(votes, g => g.Key, v => v.Date,
            //    (appGroup, groupVotes) => new
            //    {
            //        appGroup.Key,
            //        Apps = appGroup.Select(a => new { a.genre, a.numDownloads}),
            //        Votes = groupVotes.Select(v => new { v.Genre, v.Addition})
            //    }).Dump();
            //}

            return votes;
        }
    }
}
