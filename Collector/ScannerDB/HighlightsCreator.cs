using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerDB
{
    public class HighlightsCreator
    {
        List<Tag> topics;

        public HighlightsCreator()
        {
            DBConnect db_obj = new DBConnect("kensee");
            topics = db_obj.readTopics();
            db_obj.cleanTable("tbl_highlights");
        }

        public void fillTable()
        {
            DateTime dt = DateTime.Now.AddDays(-3);
            foreach (var x in topics)
                fillTableForCountry(236, x, dt);
        }

        public void fillTableForCountry(int countryId, Tag topic, DateTime startDate)
        {
            DBConnect db_obj = new DBConnect("kensee");
            List<HighlightsItem> hls = db_obj.readArticles(countryId, topic.id, startDate);
            db_obj.storeHighlightsToDB(hls, countryId, topic.name);
        }
    }

    public class Tag
    {
        public Tag(int i, string nm) { id = i; name = nm; }
        public int id;
        public string name;
    }

    public class HighlightsItem
    {
        public HighlightsItem(DateTime dt, string tt, string u) { date = dt; title = tt; url = u;  }
        public DateTime date;
        public string title;
        public string url;
    }
}
