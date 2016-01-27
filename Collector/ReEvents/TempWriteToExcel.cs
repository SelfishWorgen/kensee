using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ReEvents
{
    public class TempWriteToExcel
    {
        class RowInExcel
        {
            public int mon;
            public int year;
            public List<int> evs;
            public string priceIndex;
            public float sentiment;
            public int articleCount;

            public RowInExcel(int m, int y, int n)
            {
                mon = m;
                year = y;
                evs = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    evs.Add(0);
                }
                priceIndex = "";
                sentiment = 0;
            }
        }
        Dictionary<int, int> eventIndex;
        Dictionary<DateTime, int> dateIndex;
        List<RowInExcel> resList;
        List<string> eventNames;
        int startYear;
        int startMonth;
        int endYear;
        int endMonth;
        string economicDataFile;
        string outResultFile;
        string errorMessages;

        public void buildExcelGraph(List<GraphEventData> evs,
            List<GraphSentimentData> sts,
            List<ReEventsTagTypeValues> tps,
            string baseFolder)
        {
            errorMessages = "";
            economicDataFile = Path.Combine(baseFolder, "Canada Economic Data.xlsx");
            outResultFile = Path.Combine(baseFolder, "Events For Graphs.xlsx");
            initResult(tps);
            //if (File.Exists(economicDataFile))
            //    readEconomicData();
            //else
            //    errorMessages += " Cannot find econimic data file";
            countEvents(evs);
            countSentiments(sts);
            writeToResult();
        }

        private void initResult(List<ReEventsTagTypeValues> tps)
        {
            eventIndex = new Dictionary<int, int>();
            dateIndex = new Dictionary<DateTime, int>();
            resList = new List<RowInExcel>();
            eventNames = new List<string>();
            int ind = 0;
            foreach (ReEventsTagTypeValues t in tps)
            {
                eventIndex.Add(t.eventId, ind);
                eventNames.Add(t.eventName);
                ind++;
            }
            int n = tps.Count;
            endMonth = DateTime.Now.Month;
            endYear = DateTime.Now.Year;
            DateTime startDate = DateTime.Now.AddMonths(-35);
            startYear = startDate.Year;
            int year = startYear;
            startMonth = startDate.Month;
            int month = startMonth;
            DateTime dt = new DateTime(year, month, 1);

            for (int i = 0; i < 36; i++)
            {
                RowInExcel r = new RowInExcel(month, year, n);
                dateIndex.Add(dt, i);
                dt = dt.AddMonths(1);
                year = dt.Year;
                month = dt.Month;
                resList.Add(r);
            }
        }

        private void countEvents(List<GraphEventData> evs)
        {
            foreach (GraphEventData e in evs)
            {
                int month = 0;
                int year = DetectDate.findYearInDateString(e.date);
                if (year != 0)
                {
                    month = DetectDate.findMonthInDateString(e.date);
                }
                if (month == 0)
                {
                    year = e.articleDate.Year;
                    month = e.articleDate.Month;
                }
                int rowInd = getRowIndex(year, month);
                if (rowInd >= 0)
                {
                    RowInExcel row = resList[rowInd];
                    int evInd = eventIndex[e.eventId];
                    row.evs[evInd]++;
                }
            }
        }

        private void countSentiments(List<GraphSentimentData> sts)
        {
            foreach (GraphSentimentData st in sts)
            {
                int year = st.date.Year;
                int month = st.date.Month;
                int rowInd = getRowIndex(year, month);
                if (rowInd >= 0)
                {
                    RowInExcel row = resList[rowInd];
                    row.sentiment = st.sentiment;
                }
            }
        }

        private void readEconomicData()
        {
            int startRow = 0;
            int startColumn = -1;

            IEnumerable<ExcelReader.worksheet> sheets = null;
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(economicDataFile);
                foreach (var worksheet in sheets)
                {
                    if (startColumn > 0)
                        break;
                    for (int i = 0; i < worksheet.Rows.Length; i++)
                    {
                        int columnCount = worksheet.Rows[i].Cells.Length;
                        if (i == 0)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                string s = worksheet.Rows[i].Cells[j].Text;
                                if (s == "New Housing Price Index")
                                {
                                    startColumn = worksheet.Rows[i].Cells[j].ColumnIndex;
                                    startRow = i;
                                }
                            }
                        }
                        int year = 0;
                        int month = 0;
                        if (i > 1)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                if (worksheet.Rows[i].Cells[j] == null || String.IsNullOrEmpty(worksheet.Rows[i].Cells[j].Text))
                                    continue;
                                if (worksheet.Rows[i].Cells[j].ColumnIndex == startColumn)
                                {
                                    getYearAndMonthFromExcel(worksheet.Rows[i].Cells[j].Text, out year, out month);
                                }
                                if (worksheet.Rows[i].Cells[j].ColumnIndex == startColumn + 1 && year > 0)
                                {
                                    int rowInd = getRowIndex(year, month);
                                    if (rowInd >= 0)
                                    {
                                        resList[rowInd].priceIndex = worksheet.Rows[i].Cells[j].Text;
                                    }
                                    year = 0;
                                }
                            }
                        }
                    }
                    if (startRow == 0)
                        startRow = 0;
                    if (startColumn == 0)
                        startColumn = 0;

                }
            }
            catch (Exception ex)
            {
                errorMessages += " Cannot load economic data file " + ex.Message;
            }
        }

        private static void getYearAndMonthFromExcel(string s1, out int year, out int month)
        {
            year = 0;
            month = 0;
            string s2 = "";
            for (int ii = 0; ii < s1.Length; ii++)
            {
                if (Char.IsDigit(s1[ii]))
                    s2 += s1[ii];
            }
            long k;
            bool a = Int64.TryParse(s2, out k);
            if (a && k > 0)
            {
                DateTime dt = new DateTime(1900, 1, 1);
                dt = dt.AddDays(k);
                year = dt.Year;
                month = dt.Month;
            }
        }

        private int getRowIndex(int year, int month)
        {
            if (year < startYear || year == startYear && month < startMonth)
                return -1;
            if (year > endYear || year == endYear && month > endMonth)
                return -1;
            DateTime dt = new DateTime(year, month, 1);
            int rowInd = dateIndex[dt];
            return rowInd;
        }

        private void writeToResult()
        {
            ExcelWriter.ExcelWriter excelWriter = new ExcelWriter.ExcelWriter(outResultFile);
            excelWriter.newSheet("1", columnSizeList());
            var newRow = excelWriter.NewRow();
            excelWriter.AddCellToCurrentRow("Year", 1);
            excelWriter.AddCellToCurrentRow("Month", 1);
            foreach (string s in eventNames)
            {
                excelWriter.AddCellToCurrentRow(s, 1);
            }
            excelWriter.AddCellToCurrentRow("Price Index", 1);
            excelWriter.AddCellToCurrentRow("Sentiment", 1);
            excelWriter.addRow();
            foreach (RowInExcel r in resList)
            {
                newRow = excelWriter.NewRow();
                excelWriter.AddCellToCurrentRow(r.year.ToString(), 1);
                excelWriter.AddCellToCurrentRow(r.mon.ToString(), 1);
                foreach (int qn in r.evs)
                {
                    excelWriter.AddCellToCurrentRow(qn.ToString(), 1);
                }
                excelWriter.AddCellToCurrentRow(r.priceIndex, 1);
                excelWriter.AddCellToCurrentRow(r.sentiment.ToString(), 1);
                excelWriter.addRow();
            }
            excelWriter.Save();
        }


        private List<int> columnSizeList()
        {
            var l = new List<int>();
            l.Add(10); //year
            l.Add(10); //month
            foreach (string s in eventNames)
            {
                l.Add(15); //eventName
            }
            l.Add(10); //priceIndex
            l.Add(10); //sentiment
            return l;
        }


    }
    public class GraphEventData
    {
        public DateTime articleDate;
        public int eventId;
        public string date;
    }

    public class GraphSentimentData
    {
        public DateTime date;
        public float sentiment;
    }
}
