using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Extensions;
using ChartHelper = Extensions.ChartHelper;
using HtmlAgilityPackTest;
using HtmlAgilityPackTest.GooglePLay;
using MoreLinq;

namespace GooglePlay
{
    public partial class Form1 : Form
    {
        //public static Form1 Instance;
        public Form1()
        {
            //Instance = this;
            InitializeComponent();
            ChartHelper.Initialize(chart1, this);
        }

        public GooglePlayCategories Genres = new GooglePlayCategories();
        string basePath = @"e:/Google Play 3/";
        string fastBasePath = @"c:/cache/Google Play 3/";
        string fastMiniBasePath = @"c:/cache/Google Play 3/mini_";
        List<AppData> apps;
        List<DeveloperData> developers;
        List<YoutubeData> videos;
        
        private void Form1_Load(object sender, EventArgs e)
        {
            //fastBasePath = fastMiniBasePath; // use part of datas
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(fastBasePath);
            CheckCache("apps.txt");
            CheckCache("developers.txt");
            CheckCache("video.txt");
            
            apps = AppData.load_from_file(fastBasePath + "apps.txt");
            developers = DeveloperData.Deserialize(fastBasePath + "developers.txt");
            videos = YoutubeData.load_from_file(fastBasePath + "video.txt");

            videos = videos.DistinctBy(v => v.videoId).ToList();
            genreComboBox.Items.AddRange(Genres.All.ToArray());

            Connect();
            //SavePart();

            //SymbolsInName();
            //NameWordsCount(false);
            //PopularNameWords(); <---
            //AppsByGenres(true);
            //SimpleDevelopersAnalisis(false);
            //SimpleDevelopersAnalisis(true);
            
            //DevelopersAnalisis("Аркады и экшн");

            //YoutubeAnalisis();

            GenerateReport();
        }

        void GenerateReport()
        {
            
            //SimpleDevelopersAnalisis(true);
            //SimpleDevelopersAnalisis(false);

              CategoriesAnalisys();
              
              List<DeveloperVote> votes = GetVotes();
              DevelopersAnalisis(votes);
              //DevelopersAnalisis(votes, "Аркады");
              DevelopersAnalisis(votes, "Персонализация");

            //SymbolsInName();
            //NameWordsCount(false);
            //PopularNameWords();
            //AppsByGenres(true);
            //YoutubeAnalisis();
        }

        void AppsByGenres(bool withVideo)
        {
            apps.Where(a => !withVideo || a.video != null)
                .GroupBy(a => a.genre)
                .ToChart(g => g.Key, g => g.Count(), "AppsByGenres");
        }

        void YoutubeAnalisis()
        {
            // + paid or not
            // + genre
            //videos.Where(v => v.viewsNumber != -1)
            //    .ToChart(v => v.app.numDownloads + 1, v => v.viewsNumber, 
            //    "key=app.numDownloads value=viewsNumber",
            //    chart => 
            //    {
            //        chart.ChartAreas[0].AxisX.IsLogarithmic = true;
            //        chart.ChartAreas[0].AxisY.IsLogarithmic = true;
            //    });

            //apps.Where(a => a.price == 0)
            //    .GroupBy(a => a.video != null ? "with video" : "no video")
            //    .ToChart(g => g.Key, g => g.Average(a => a.numDownloads),
            //    "среднее количество загрузок бесплатного приложения в зависимости от наличия видео");

            //apps.Where(a => a.price > 0)
            //    .GroupBy(a => a.video != null ? "with video" : "no video")
            //    .ToChart(g => g.Key, g => g.Average(a => a.profit),
            //    "средняя прибыль с продажи платного приложения в зависимости от наличия видео");

            apps.Where(a => a.price == 0)
                .GroupBy(a => a.genre)
                .ToChart(g => g.Key, g => g.Where(a => a.video != null).Average(a => a.numDownloads) 
                                        / g.Where(a => a.video == null).Average(a => a.numDownloads),
                "t",
                chart => chart.ChartAreas[0].AxisY.Interval = 1
                //chart => chart.ChartAreas[0].AxisY.StripLines.Add(new StripLine() {BorderColor = Color.Green, IntervalOffset = 1})
                );


            /////////////////////////////////////////////////////////
            
            //videos.OrderBy(v => v.date)
            //      .Select(v => new DataPoint(1, v.date))
            //      .Scan(DataPoint.Sum)
            //      .ToChart(p => p.Date, p => p.Value, "apps videos count");
            //videos.ToChart(v => v.viewsNumber, v => v.app.numDownloads + 1, "downloads vs youtube views");

            //videos.Where(v => v.viewsNumber != -1)
            //      .Select(v => new
            //{
            //    downloads = v.app.numDownloads,
            //    views = v.viewsNumber,
            //    val = (v.app.numDownloads + 1)/(double) v.viewsNumber
            //}).Dump();

            int allV = videos.Count(v => v.viewsNumber != -1);
            int moreDownloading = videos.Count(v => v.viewsNumber != -1 && v.app.numDownloads > v.viewsNumber);

            //videos.Where(v => v.viewsNumber != -1)
            //      .Select(v => (v.app.numDownloads + 1) / (double)v.viewsNumber)
            //      .OrderBy(d => d)
            //      .ToChart(d => d, "video ratio");

            //videos.OrderByDescending(v => v.viewsNumber).Take(5).Dump();
            //videos.OrderByDescending(v => v.app.numDownloads).Take(5).Dump();
            //videos.OrderByDescending(v => v.app.profit).Take(5).Dump();
        }

        void CategoriesAnalisys()
        {
            apps.GroupBy(a => a.genre)
                .OrderBy(g => Genres.All.IndexOf(g.Key))
                .ToChart(g => g.Key, g => g.Count(), "apps count by categories");

            apps.Where(a => a.IsPaid)
                .GroupBy(a => a.genre)
                .OrderBy(g => Genres.All.IndexOf(g.Key))
                .ToChart(g => g.Key, g => (int)g.Average(a => a.profit), "average paid app profit by categories");
            
            apps.Where(a => !a.IsPaid)
                .GroupBy(a => a.genre)
                .OrderBy(g => Genres.All.IndexOf(g.Key))
                .ToChart(g => g.Key, g => (int)g.Average(a => a.numDownloads), "average free app downloads by categories");
        }

        void DevelopersAnalisis(List<DeveloperVote> votes)
        {
            //int Аркады = apps.Where(a => a.genre == "Аркады").Select(a => a.developer).Distinct().Count();
            //int Персонализация = apps.Where(a => a.genre == "Персонализация").Select(a => a.developer).Distinct().Count();

            votes.GroupBy(v => v.Genre)
                .OrderBy(g => Genres.All.IndexOf(g.Key))
                .ToChart(g => g.Key, g => g.Count(), "votes count by genres");

            //votes.GroupBy(v => v.Genre)
            //     .OrderBy(g => Genres.All.IndexOf(g.Key))
            //     .ToChart(g => g.Key, g => g.Sum(v => Math.Abs(v.Addition)), "DevelopersAnalisis Math.Abs");

            // столбики по всем жанрам 
            votes.GroupBy(v => v.Genre)
                 .OrderBy(g => Genres.All.IndexOf(g.Key))
                 .ToChart(g => g.Key, g => g.Sum(v => v.Addition), "DevelopersAnalisis common");

            int arcadesPlus = votes.Where(v => v.Genre == "Аркады" && v.Addition > 0).Sum(v => v.Addition);
            int arcadesMinus = votes.Where(v => v.Genre == "Аркады" && v.Addition < 0).Sum(v => v.Addition);
            int personalizationPlus = votes.Where(v => v.Genre == "Персонализация" && v.Addition > 0).Sum(v => v.Addition);
            int personalizationMinus = votes.Where(v => v.Genre == "Персонализация" && v.Addition < 0).Sum(v => v.Addition);

            votes.GroupBy(v => v.Genre)
                 .OrderBy(g => Genres.All.IndexOf(g.Key))
                 .ToChart(g => g.Key, 
                          g => ((double)g.Where(v => v.Addition > 0).Sum(v => v.Addition)
                                     / -g.Where(v => v.Addition < 0).Sum(v => v.Addition)).ToString("##.###"), 
                          "DevelopersAnalisis votes ratio");
        }

        void DevelopersAnalisis(List<DeveloperVote> votes, string genre)
        {
            votes.Where(v => v.Genre == genre)
                 .OrderBy(v => v.Date)
                 .Select(v => new DataPoint(v.Addition, v.Date))
                 .Scan(DataPoint.Sum)
                 .ToChart(o => o.Date, o => o.Value, "DevelopersAnalisis " + genre);

            int plusCount = votes.Sum(v => v.Genre == genre && v.Addition > 0 ? v.Addition : 0);
            int minusCount = votes.Sum(v => v.Genre == genre && v.Addition < 0 ? v.Addition : 0);
        }

        List<DeveloperVote> GetVotes()
        {
            List<DeveloperVote> votes = new List<DeveloperVote>(videos.Count);
            foreach (DeveloperData developer in developers)
            {
                votes.AddRange(developer.GetVotes());
            }
            return votes;
        }

        // !!! if all than no order on creation time !!!
        void SimpleDevelopersAnalisis(bool all = true)
        {
            Dictionary<string, int> genres = Genres.All.ToDictionary(g => g, g => 0);

            foreach (DeveloperData developer in developers)
            {
                HashSet<string> experience = new HashSet<string>();

                List<AppData> developerApps = all ? developer.apps : developer.apps.Where(a => a.video != null).OrderBy(a => a.video.date).ToList();
                foreach (AppData app in developerApps)
                {
                    if (experience.Contains(app.genre))
                    {
                        genres[app.genre] += experience.Count - 1;
                        foreach (string expGenre in experience)
                        {
                            if (expGenre != app.genre)
                            {
                                --genres[expGenre];
                            }
                        }
                    }

                    experience.Add(app.genre);
                }
            }

            int sum = genres.Sum(p => p.Value);

            genres.ToChart(p => p.Key, p => p.Value, "genres index" + (all ? " all" : ""));

            //apps.Where(a => a.video != null)
            //    .GroupBy(a => a.video.date.Year)
            //    .ToChart(g => g.Key, g => g.Count(), "apps count by year");
        }

        void PopularNameWords()
        {
            //string[] keyWords =
            //{
            //    "minecraft", "craft", "zombie", "angry",
            //    "Free", "the", "Pro", "Lite", "HD", "Android", "ios", "iphone", "Music", "World", "Full", "Simple", "New", "Plus", "Smart", "Sexy",
            //    "happy", "flappy", "bird"
            //};
            //
            //keyWords.ToChart(k => k,
            //                 k => numDownloadsRatio(apps, a => a.NameWords.Contains(k, StringComparer.CurrentCultureIgnoreCase)),
            //                 "numDownloadsRatio on keywords in app name");
            //// angry top
            ////apps.Where(a => a.NameWords.Contains("angry", StringComparer.CurrentCultureIgnoreCase)).OrderByDescending(a => a.numDownloads).Dump(1);
            //return;


            Dictionary<string, int> wordCount = apps
                .SelectMany(a => a.NameWords)
                .GroupBy(w => w.ToLower())//, StringComparer.CurrentCultureIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count());
            List<KeyValuePair<string, int>> keyValuePairs = wordCount.OrderByDescending(pair => pair.Value).Dump().ToList();
            int popularLimit = keyValuePairs[(int)(keyValuePairs.Count * 0.7)].Value;

            //apps.GroupBy(a => a.genre)
            //    .ToChart(g => g.Key, 
            //             g => numDownloadsRatio(g, a => a.NameWords.Sum(w => wordCount[w.ToLower()]) > popularLimit),
            //             "отношение среднего количества закачек приложения, которое использует популярные слова в названии к среднему" +
            //             "количеству закачек приложения которое использует уникальные слова в названии в зависимости от жанра");

            //apps.Where(a => a.numDownloads < 0).Dump();

            apps.ToChart(a => a.NameWords.Sum(w => wordCount[w.ToLower()]) + 1,
                         a => a.numDownloads + 1,
                         "name words usage count to numDownloads",
                         chart =>
                         {
                             chart.ChartAreas[0].AxisX.IsLogarithmic = true;
                             chart.ChartAreas[0].AxisY.IsLogarithmic = true;
                         });

            //dictionary.ToChart(g => g.Key, g => g.Value, "test");
        }

        // to do app.result
        // если я буду использовать эту функцию, но забуду предварительно отфильтровать?
        Func<IEnumerable<AppData>, Func<AppData, bool>, double> numDownloadsRatio = (list, predicate) =>
             list.Where(predicate).Select(a => a.numDownloads).DefaultIfEmpty(0).Average()
           / list.Where(a => !predicate(a)).Average(a => a.numDownloads);

        void SymbolsInName()
        {
            List<string> pairs = AppData.WordsSeparators.Except(new[] { ';' }).SelectMany(ch => AppData.WordsSeparators, (c1, c2) => "" + c1 + c2).ToList();

            pairs.GroupBy(
                pair => pair,
                pair => numDownloadsRatio(apps, a => pair.All(ch => a.name.Contains(ch)))
                )
                .Where(g => g.First() > 2)
                .ToChart(g => g.Key, 
                         g => g.First(), 
                         "increasing of average downloads on appearing 2x symbols in app name");

            AppData.WordsSeparators.ToChart(
                ch => ch.ToString(),
                ch => numDownloadsRatio(apps, a => a.name.Contains(ch)),
                "increasing of average downloads on appearing symbol in app name"
                );

            apps.GroupBy(a => a.name.Split(new[]{"&amp:"}, StringSplitOptions.None).Count())
                .Dump(3)
                .ToChart(g => g.Key - 1, 
                         g => g.Average(a => a.numDownloads),
                         "average numDownloads on number of \'&\' in app name");

            //apps.GroupBy(a => a.name.Count(ch => AppData.WordsSeparators.Contains(ch)))
            //    .Dump(3)
            //    .ToChart(g => g.Key, g => g.Average(a => a.numDownloads), "t");

            // АГОНЬ !
            //apps.GroupBy(a => a.name.Contains("-") && a.name.Contains("&"))
            //    .Dump(3)
            //    .ToChart(g => g.Key, g => g.Average(a => a.numDownloads), "t");

            //apps.GroupBy(a => a.name.Contains("-") && a.name.Contains(":"))
            //    .Dump(3)
            //    .ToChart(g => g.Key, g => g.Average(a => a.numDownloads), "t");
        }

        void NameWordsCount(bool toProfit)
        {
            apps.Where(a => toProfit ? a.price > 0 : a.price == 0)
                .GroupBy(a => a.NameWords.Length)
                .ToChart(g => g.Key,
                         g => (int)g.Average(a => toProfit ? a.profit : a.numDownloads),
                         "количество слов в названии vs " + (toProfit ? "профит" : "количество закачек"));
        }

        void Connect()
        {
            Dictionary<string, AppData> idToApp = apps.ToDictionary(app => app.Id);
            foreach (DeveloperData developer in developers)
            {
                foreach (string appId in developer.appsIDs)
                {
                    if (idToApp.ContainsKey(appId))
                    {
                        idToApp[appId].developer = developer;
                        developer.apps.Add(idToApp[appId]);
                    }
                }
            }

            Dictionary<string, YoutubeData> idToVideo = videos.ToDictionary(v => v.videoId);
            foreach (AppData app in apps)
            {
                if (idToVideo.ContainsKey(app.videoID))
                {
                    YoutubeData video = idToVideo[app.videoID];
                    app.video = video;
                    video.app = app;
                }
            }

            // sort developer apps
            foreach (DeveloperData developer in developers)
            {
                developer.apps = developer.apps.OrderBy(a => a.video == null ? DateTime.MinValue : a.video.date).ToList();
            }
        }

        void SavePart(int step = 10)
        {
            List<AppData> appsPart = new List<AppData>();
            List<DeveloperData> developersPart = new List<DeveloperData>();
            List<YoutubeData> videosPart = new List<YoutubeData>();
            foreach (DeveloperData developer in developers.TakeEvery(step))
            {
                developersPart.Add(developer);
                foreach (AppData app in developer.apps)
                {
                    appsPart.Add(app);
                    if (app.video != null)
                    {
                        videosPart.Add(app.video);
                    }
                }
            }

            AppData.save_to_file(appsPart, fastMiniBasePath + "apps.txt");
            YoutubeData.save_to_file(videosPart, fastMiniBasePath + "video.txt");
            DeveloperData.Serialize(developersPart, fastMiniBasePath + "developers.txt");
        }

        private void CheckCache(string fileName)
        {
            if (!File.Exists(fastBasePath + fileName))
            {
                File.Copy(basePath + fileName, fastBasePath + fileName);
            }
        }

        private void VerifyGenresButton_Click(object sender, EventArgs e)
        {
            List<string> genres = apps.Select(a => a.genre).Distinct().ToList();
            Genres.VerifyGenres(genres);
        }

        void addDataToChart_genre_sorted(Chart chart, List<AppData> apps, string genre)
        {
            chart.Series.Clear();
            chart.Series.Add("apps profits of genre = " + genre);
            chart.Series[0].ChartType = SeriesChartType.Area;
            chart.ChartAreas[0].AxisY.IsLogarithmic = true;

            List<double> profits = apps
                .Where(a => a.price > 0 && (genre == "" || a.genre == genre))
                .Select(a => a.profit).ToList();
            profits.Sort();

            foreach(double point in profits)
            {
                chart.Series[0].Points.AddY(Math.Max(point, 1));
            }
        }

        void addDataToChart_Pivot(Chart chart, List<AppData> apps)
        {
            chart.ChartAreas[0].AxisX.Interval = 1;
            chart.Series.Clear();
            chart.Series.Add("averageProfit");
            chart.Series[0].IsValueShownAsLabel = true;
            chart.ChartAreas[0].AxisY.IsLogarithmic = false;

            Dictionary<string, Dictionary<bool, double>> res =
                apps.Pivot(a => a.genre, a => a.price > 0, lst => lst.Count() * 1.0); //lst.Average(a => a.profit)));
            foreach (string genre in res.Keys)
            {
                chart.Series[0].Points.AddXY(genre, (int)res[genre][true]);
            }
        }

        void CreateAndSetTable(List<AppData> apps)
        {
            DataGridView table = new DataGridView();

        }

        void WorstBestsellers(List<AppData> apps, int elements)
        {
            List<AppData> topApps = apps
                .Where(a => a.price > 0)
                .OrderByDescending(a => a.price * a.numDownloads / (a.score + 1))
                .Take(elements)
                .ToList().Dump();
        }

        void WorstBestsellersFixed(List<AppData> apps, int elements, double maxScore)
        {
            List<AppData> topApps = apps
                .Where(a => a.price > 0 && a.score <= maxScore)
                .OrderByDescending(a => a.profit)
                .Take(elements)
                .ToList().Dump();
        }

        void Top(List<AppData> apps, int elements, bool paid, List<string> genres)
        {
            List<AppData> topApps = apps
                .Where(a => paid == a.price > 0)
                .Where(a => genres.Contains(a.genre))
                .OrderByDescending(a => paid ? a.profit : a.numDownloads)
                .Take(elements)
                .ToList().Dump();
        }

        void Words(List<AppData> apps)
        {
            var word2money = apps.Select(a => 
                new {word = a.name.Split(),
                     money = a.profit / a.name.Split().Length}).Dump();
        }

        private void commonChartButton_Click(object sender, EventArgs e)
        {
            addDataToChart_Pivot(chart1, apps);
        }

        private void genreChartButton_Click(object sender, EventArgs e)
        {
            addDataToChart_genre_sorted(chart1, apps, genreComboBox.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //WorstBestsellersFixed(apps, 200, 10);

            Top(apps, 200, false, Genres.Apps);

            //var o = new {a = 1, b = 2};
            //o.Dump();


            //Words(apps);
        }

        //void NameWordsCount(Chart chart, bool toProfit)
        //{
        //    // количество слов в названии -> профит
        //    IEnumerable<IGrouping<int, AppData>> wordsNumberToApp = apps.GroupBy(a => a.NameWords.Length);
        //
        //    chart.ChartAreas[0].AxisX.Interval = 1;
        //    chart.Series.Clear();
        //    chart.Series.Add("averageProfit");
        //    chart.Series[0].IsValueShownAsLabel = true;
        //    chart.ChartAreas[0].AxisY.IsLogarithmic = false;
        //
        //    foreach (IGrouping<int, AppData> grouping in wordsNumberToApp)
        //    {
        //        chart.Series[0].Points.AddXY(grouping.Key, grouping.Average(a => toProfit ? a.profit : a.numDownloads));
        //    }
        //
        //    chart.SaveImage("количество слов в названии vs профит" + ".png", ChartImageFormat.Png);
        //}
    }

    public class DataPoint
    {
        public int Value;
        public DateTime Date;

        public DataPoint(int value, DateTime date)
        {
            Value = value;
            Date = date;
        }

        public static DataPoint Sum(DataPoint first, DataPoint second)
        {
            return new DataPoint(first.Value + second.Value, ChartHelper.Max(first.Date, second.Date));
        }
    }
}
