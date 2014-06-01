using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;

namespace Extensions
{
    public static class Watch
    {
        static Stopwatch _stopwatch;

        public static void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public static void Stop(string message)
        {
            _stopwatch.Stop();
            Console.WriteLine(message + " : " + _stopwatch.Elapsed);
        }
    }

    public static class Helper
    {
        public static void MeasureTime(this Action action)
        {
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            action();
            sw1.Stop();
            Console.WriteLine("elapsed " + sw1.Elapsed);
        }

        public static IEnumerable<double> Normalize<T>(this IEnumerable<T> values)
        {
            double sum = values.Cast<double>()
                               .Sum();
            return values.Cast<double>().Select(v => v / sum);
        }

        public static Double Correlation(IList<double> first, IList<double> second)
        {
            double sumX = 0;
            double sumX2 = 0;
            double sumY = 0;
            double sumY2 = 0;
            double sumXY = 0;

            Debug.Assert(first.Count == second.Count);
            int n = first.Count;

            for (int i = 0; i < n; ++i)
            {
                double x = first[i];
                double y = second[i];

                sumX += x;
                sumX2 += x * x;
                sumY += y;
                sumY2 += y * y;
                sumXY += x * y;
            }

            double stdX = Math.Sqrt(sumX2 / n - sumX * sumX / n / n);
            double stdY = Math.Sqrt(sumY2 / n - sumY * sumY / n / n);
            double covariance = (sumXY / n - sumX * sumY / n / n);

            return covariance / stdX / stdY;
        }

        public static decimal GetMedian(this IEnumerable<int> source)
        {
            // Create a copy of the input, and sort the copy
            int[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return (a + b) / 2m;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }

        public static void WriteAllStructs<T>(string fileName, T[] array)
        {
            long size = Marshal.SizeOf(typeof(T)) * (long)array.Length;
            byte[] bytes = new byte[size];
            GCHandle pinnedHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            Marshal.Copy(pinnedHandle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            pinnedHandle.Free();
            File.WriteAllBytes(fileName, bytes);
        }

        public static T[] ReadAllStructs<T>(string fileName)
        {
            var readBytes = File.ReadAllBytes(fileName);

            if (readBytes.Length % Marshal.SizeOf(typeof (T)) != 0)
            {
                throw new Exception("file length % struct size != 0");
            }

            int numVectors = readBytes.Length / Marshal.SizeOf(typeof(T));
            T[] readVectors = new T[numVectors];
            GCHandle pinnedHandle = GCHandle.Alloc(readVectors, GCHandleType.Pinned);
            Marshal.Copy(readBytes, 0, pinnedHandle.AddrOfPinnedObject(), readBytes.Length);
            pinnedHandle.Free();
            return readVectors;
        }

        public static T DeserializeJsonFile<T>(string fileName)
        {
            T result;
            using (StreamReader file = File.OpenText(fileName))
            using (JsonReader reader = new JsonTextReader(file))
            {
                result = new JsonSerializer().Deserialize<T>(reader);
            }

            return result;
        }

        public static IEnumerable<long> Range(long first, long step)
        {
            for (long i = first; i < first + step; ++i)
            {
                yield return i;
            }
        }

        public static void Test64BitMemory()
        {
            List<string> list = Enumerable.Range(0, 10).Select(n => new string((char)('0' + n), 128 * 1024 * 1024)).ToList();
            int count = list[5].Count();
        }

        public static void TestMaxMemory()
        {
            double[] bytes = new double[1000000000]; // 8 gb
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = i;
            }
            Console.WriteLine(bytes.Length.ToString());
            Console.WriteLine(bytes[bytes.Length - 1]);
        }

        public static void ParallelFor(long first, long last, int minThreadsNumber, Action<long> action)
        {
            if(minThreadsNumber <= 0)
            {
                throw new ArgumentException();
            }

            long size = last - first;
            long partSize = size / minThreadsNumber;
            long rest = size % minThreadsNumber;
            Debug.Assert(partSize * minThreadsNumber + rest == last - first);

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < minThreadsNumber; ++i)
            {
                int index = i;
                Thread thread = new Thread(() => For(first + index * partSize, first + (index + 1) * partSize, action));
                thread.Start();
                threads.Add(thread);
            }

            For(last - rest, last, action);

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        public static void For(long first, long last, Action<long> action)
        {
            for (long val = first; val < last; ++val)
            {
                action(val);
            }
        }

        public static int IndexOf<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> condition)
        {
            var index = 0;
            foreach (var item in items)
            {
                if (condition(item))
                {
                    return index;
                }
                ++index;
            }
            return -1;
        }

        public static string ToCsv<T>(this IEnumerable<T> items, bool includeHeader, string[] properties)
        {
            if (items == null)
                return null;

            StringBuilder csv = new StringBuilder();
            if (includeHeader)
            {
                foreach (var property in properties)
                {
                    csv.AppendFormat("{0};", property);
                }
            }

            foreach (T item in items)
            {
                csv.AppendLine();
                foreach (var property in properties)
                {
                    csv.AppendFormat("{0};", typeof(T).GetProperty(property).GetValue(item));
                }
            }

            return csv.ToString();
        }

        public static string ToCsv<T>(this IEnumerable<T> items, bool includeHeader)
        {
            if (items == null)
                return null;

            string[] properties = typeof (T).GetProperties().Select(p => p.Name).ToArray();
            return ToCsv(items, includeHeader, properties);
        }
    }
}
