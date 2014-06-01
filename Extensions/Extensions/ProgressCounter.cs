using System;
using System.Threading;

namespace Extensions
{
    public class ProgressCounter
    {
        string _name;
        private int _counter = 0;
        public int Counter { get { return _counter; } }

        private DateTime _startTime;

        public ProgressCounter(string name = "")
        {
            _name = name;
            Start();
        }

        public void Start()
        {
            _startTime = DateTime.Now;
        }

        public void Increment()
        {
            Interlocked.Increment(ref _counter);
        }

        public double CountPerSecond()
        {
            return _counter / (DateTime.Now - _startTime).TotalSeconds;
        }

        public bool LogEvery(int period)
        {
            if (_counter % period == 0)
            {
                //Console.WriteLine("progress.Counter = {0}, progress.CountPerSecond = {1}", _counter, CountPerSecond());
                Console.WriteLine(Message);
                return true;
            }
            return false;
        }

        public string Message
        {
            get { return String.Format(_name + " ( {0}   {1:#.00} / Sec )", _counter, CountPerSecond()); }
        }
    }
}
