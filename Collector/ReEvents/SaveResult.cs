using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ReEvents
{
    public class Article
    {
        public string title;
        public string href;
        //public string content = "";
        public string dt;
        public int type;
        public string companyName;
        public string country;
        public string sector;
        public string eventType;
    }

    class SaveResult
    {
        public List<Article> articleList;

        public void addArticle(string title, string href, string dt, int type, string companyName, string country,
            string sector, string eventType)
        {
            if (articleList == null)
                articleList = new List<Article>();
            articleList.Add(new Article
                {
                    title = title,
                    href = href,
                    dt = dt,
                    type = type,
                    companyName = companyName,
                    country = country,
                    sector = sector,
                    eventType = eventType
                });
        }

        public void writeToExcel(string fileName)
        {
            if (articleList == null || articleList.Count == 0)
                return;
            ExcelWriter.ExcelWriter excelWriter = new ExcelWriter.ExcelWriter(fileName);
            excelWriter.newSheet("1", columnSizeList());

            var newRow = excelWriter.NewRow();
            excelWriter.AddCellToCurrentRow("Title", 1);
            excelWriter.AddCellToCurrentRow("URL", 1);
            excelWriter.AddCellToCurrentRow("Date", 1);
            excelWriter.AddCellToCurrentRow("Type", 1);
            excelWriter.AddCellToCurrentRow("Company name", 1);
            excelWriter.AddCellToCurrentRow("Country", 1);
            excelWriter.AddCellToCurrentRow("Sector", 1);
            excelWriter.AddCellToCurrentRow("Event", 1);
            excelWriter.addRow();
            foreach (Article a in articleList)
            {
                if (a.eventType != "")
                {
                    excelWriter.NewRow();
                    excelWriter.AddCellToCurrentRow(a.title, 1);
                    excelWriter.AddCellToCurrentRow(a.href, 1);
                    excelWriter.AddCellToCurrentRow(a.dt, 1);
                    excelWriter.AddCellToCurrentRow(a.type.ToString("D3"), 1);
                    excelWriter.AddCellToCurrentRow(a.companyName, 1);
                    excelWriter.AddCellToCurrentRow(a.country, 1);
                    excelWriter.AddCellToCurrentRow(a.sector, 1);
                    excelWriter.AddCellToCurrentRow(a.eventType, 1);
                    excelWriter.addRow();
                }
            }
            articleList.Clear();
            excelWriter.Save();
        }

        private List<int> columnSizeList()
        {
            var l = new List<int>();
            l.Add(130); //title
            l.Add(180); //href
            l.Add(20); //dt
            l.Add(10); //type
            l.Add(20); //companyName
            l.Add(20); //country
            l.Add(20); //sector
            l.Add(15); //eventType
            return l;
        }

    }
}
