using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPackTest;
using HtmlAgilityPackTest.GooglePLay;

namespace GooglePlay
{
    public class DeveloperVote
    {
        public DeveloperData Developer;
        public string Genre;
        public DateTime Date;
        public int Addition;

        public DeveloperVote(DeveloperData developer, string genre, DateTime date, int addition)
        {
            Developer = developer;
            Genre = genre;
            Date = date;
            Addition = addition;
        }
    }
}
