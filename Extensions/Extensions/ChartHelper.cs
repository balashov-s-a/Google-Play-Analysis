using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Extensions
{
    public static class ChartHelper
    {
        static Chart _chart;
        static Form _form;
        public static void Initialize(Chart chart, Form form)
        {
            _chart = chart;
            _form = form;

            _chart.ChartAreas[0].Position = new ElementPosition(0, 0, 100, 100); //(-1, 0, 102, 102);
        }

        static Font _chartFont = new Font("Serif", 10, FontStyle.Regular);
        static void SetFont()
        {
            _chart.Series[0].Font = _chartFont;
            _chart.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = 14;
            _chart.ChartAreas[0].AxisY.LabelAutoFitMinFontSize = 14;
        }

        public static void ToHist<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> valueSelector,
            int partitionsNumber,
            string description,
            Action<Chart> configureChart = null)
        {
            if (_form != null)
            {
                _form.Text = description;
            }

            _chart.ChartAreas[0].AxisX.IsLogarithmic = false;
            _chart.ChartAreas[0].AxisY.IsLogarithmic = false;
            _chart.Series.Clear();
            _chart.Series.Add("");
            _chart.Legends[0].Enabled = false;

            _chart.Series[0].ChartType = SeriesChartType.RangeColumn;
            _chart.Series[0].IsValueShownAsLabel = false;

            List<double> values = source.Select(valueSelector).OrderBy(e => e).ToList();
            double min = values.First();
            double max = values.Last();
            double size = (max - min) / partitionsNumber;
            _chart.ChartAreas[0].AxisX.Interval = size;
            _chart.ChartAreas[0].AxisX.IntervalOffset = size * 0.5;

            if (configureChart != null)
            {
                configureChart(_chart);
            }

            Dictionary<int, int> columns = values.GroupBy(v => Math.Min(partitionsNumber - 1, (int)((v - min) / size))).ToDictionary(g => g.Key, g => g.Count());
            for (int i = 0; i < partitionsNumber; ++i)
            {
                _chart.Series[0].Points.AddXY((i + 0.5) * size + min, columns.ContainsKey(i) ? columns[i] : 0);
            }

            _chart.SaveImage(description + ".png", ChartImageFormat.Png);
        }

        public static void ToChart<TSource, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TElement> valueSelector,
            string description)
        {
            if (_form != null)
            {
                _form.Text = description;
            }
            _chart.ChartAreas[0].AxisX.IsLogarithmic = false;
            _chart.ChartAreas[0].AxisY.IsLogarithmic = false;
            _chart.Series.Clear();
            _chart.Series.Add("");
            _chart.Legends[0].Enabled = false;

            _chart.Series[0].ChartType = SeriesChartType.Line;
            _chart.Series[0].IsValueShownAsLabel = false;

            foreach (TSource item in source)
            {
                _chart.Series[0].Points.AddY(valueSelector(item));
            }

            _chart.SaveImage(description + ".png", ChartImageFormat.Png);
        }


        public static void ToChart<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> valueSelector,
            string description,
            Action<Chart> configureChart = null) where TKey : IComparable
        {
            if (_form != null)
            {
                _form.Text = description;
            }

            _chart.ChartAreas[0].AxisX.IsLogarithmic = false;
            _chart.ChartAreas[0].AxisY.IsLogarithmic = false;

            _chart.Series.Clear();
            _chart.Series.Add("");
            SetFont();
            _chart.Legends[0].Enabled = false;

            if (typeof (TKey) == typeof (DateTime))
            {
                _chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy.MM.dd";
            }

            var sorted = IsSorted<TSource, TKey>(source, keySelector);

            int distinctCount = source.Select(keySelector).Distinct().Count();

            if (distinctCount <= 50 && source.Count() == distinctCount)
            {
                _chart.Series[0].ChartType = SeriesChartType.Column;
                _chart.Series[0].IsValueShownAsLabel = true;
                _chart.ChartAreas[0].AxisX.Interval = 1;
            }
            else if(sorted)
            {
                _chart.Series[0].ChartType = SeriesChartType.Line;
                _chart.Series[0].BorderWidth = 2;
                _chart.Series[0].IsValueShownAsLabel = false;
                _chart.ChartAreas[0].AxisX.Interval = 0;
            }
            else
            {
                _chart.Series[0].ChartType = SeriesChartType.Point;
                _chart.Series[0].IsValueShownAsLabel = false;
                _chart.ChartAreas[0].AxisX.Interval = 0;
            }
            if (configureChart != null)
            {
                configureChart(_chart);
            }

            foreach (TSource item in source)
            {
                _chart.Series[0].Points.AddXY(keySelector(item), valueSelector(item));
            }

            _chart.ChartAreas[0].RecalculateAxesScale();

            _chart.Dock = DockStyle.None;
            _chart.Height = 1000;
            _chart.Width = 1000;

            string callerMethodName = new StackTrace().GetFrame(1).GetMethod().Name;
            string folderName = @"report\" + callerMethodName;
            Directory.CreateDirectory(folderName);
            _chart.SaveImage(folderName + @"\" + description + ".png", ChartImageFormat.Png);
            _chart.Dock = DockStyle.Fill;
        }

        static bool IsSorted<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
        {
            TKey lastKey = default(TKey);
            bool isFirst = true;
            bool sorted = true;
            foreach (TSource item in source)
            {
                TKey key = keySelector(item);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    if (lastKey.CompareTo(key) > 0)
                    {
                        sorted = false;
                        break;
                    }
                }
                lastKey = key;
            }
            return sorted;
        }

        public static DateTime Max(DateTime first, DateTime second)
        {
            return first > second ? first : second;
        }

        public static void ToPowHist<TSource>(this IEnumerable<TSource> source, Func<TSource, double> valueSelector, string message)
        {
            source.Select(v => (int)Math.Log10(valueSelector(v)))
                  .GroupBy(v => v)
                  .OrderBy(v => v.Key).Dump()
                  .ToChart(v => String.Format("{0}-{1}", Math.Pow(10, v.Key), Math.Pow(10, v.Key + 1)), v => v.Count(), message);
        }
    }
}
