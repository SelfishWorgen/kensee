using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace RegressionApp
{
    class Program
    {
        static StreamWriter logger;

        static void Main(string[] args)
        {
            string fld = "c:\\temp";
            if (!Directory.Exists(fld))
                Directory.CreateDirectory(fld);
            logger = new System.IO.StreamWriter(Path.Combine(fld, "RegressionModel.txt"), false);
            //            model1(args);
            model2(args);
            logger.Close();
        }

        static void model2(string[] args)
        {
            var snmts = fillTableForCountry(40);
            
            Dictionary<DateTime, double> unemploymentRate = new Dictionary<DateTime,double>();
            Dictionary<DateTime, double> consumerPriceIndex = new Dictionary<DateTime,double>();
            readExcelFile(unemploymentRate, consumerPriceIndex);

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);
            string fileName = "Stock_Canada_with_ID 2015-10-19.xlsx"; // Stock_Canada.xlsx";
            string fullFileName = Path.Combine(directory + "\\..\\data", fileName);
            if (!File.Exists(fullFileName))
            {
                fullFileName = Path.Combine(directory + "\\..\\..\\..\\data", fileName);
                if (!File.Exists(fullFileName))
                    return;
            }
            
            Excel.Application excelApp = null;
            Excel.Workbook excelWorkBook = null;
            //       try
            {
                Dictionary<string, Dictionary<DateTime, float>> dict = new Dictionary<string, Dictionary<DateTime, float>>();
                excelApp = new Excel.Application();
                excelWorkBook = excelApp.Workbooks.Open(fullFileName);
                foreach (Excel.Worksheet worksheet in excelWorkBook.Sheets)
                {
                    Excel.Range range = worksheet.UsedRange;
                    object[,] values = (object[,])range.Value2;
                    int num_records = range.Rows.Count;
                    int num_fields = range.Columns.Count;
                    for (int i = 3; i <= num_fields; i++)
                    {
                        string company = values[1, i].ToString();
                        string idStr = values[2, i].ToString();
                        int companyId = Convert.ToInt32(idStr);
                        logger.WriteLine("Company \"" + company + "\"");
                        if (companyId == 0)
                        {
                            logger.WriteLine("Company was not found in Kensee Database");
                            logger.WriteLine("\n");
                            continue;
                        }
                        List<DateValueItem> spd = new List<DateValueItem>();
                        for (int j = 4; j <= num_records; j++)
                        {
                            if (values[j, i] != null)
                            {
                                double d = double.Parse(values[j, 1].ToString());
                                DateTime dt = DateTime.FromOADate(d);
                                spd.Add(new DateValueItem(dt, double.Parse(values[j, i].ToString())));
                            }
                        }
                        spd.Sort(delegate(DateValueItem x, DateValueItem y)
                        {
                            return x.date.CompareTo(y.date);
                        });
                        DateTime dtNow = DateTime.Now;
                        DateTime dt1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);

                        DateTime startDateY3 = dt1.AddYears(-3);
                        startDateY3 = startDateY3.AddDays((8 - (int)startDateY3.DayOfWeek) % 7);
                        var spdw = fillTableForPeriod(spd, startDateY3, true, 36);
                        calculateRegeression(company, snmts, spdw, unemploymentRate, consumerPriceIndex);
                    }
                    break;
                }
                excelWorkBook.Close(false);
                excelApp.Quit();
            }
            //catch (Exception ex)
            //{
            //    logger.WriteLine("error: " + ex.Message);
            //}
        }

        static void readExcelFile(Dictionary<DateTime, double> unemploymentRate, Dictionary<DateTime, double> consumerPriceIndex)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);
            string fileName = "Canada Economic Data.xlsx"; 
            string fullFileName = Path.Combine(directory + "\\..\\data", fileName);
            if (!File.Exists(fullFileName))
            {
                fullFileName = Path.Combine(directory + "\\..\\..\\..\\data", fileName);
                if (!File.Exists(fullFileName))
                    return;
            }

            Excel.Application excelApp = null;
            Excel.Workbook excelWorkBook = null;
            //       try
            {
                excelApp = new Excel.Application();
                excelWorkBook = excelApp.Workbooks.Open(fullFileName);
                foreach (Excel.Worksheet worksheet in excelWorkBook.Sheets)
                {
                    Excel.Range range = worksheet.UsedRange;
                    object[,] values = (object[,])range.Value2;
                    int num_records = range.Rows.Count;
                    int num_fields = range.Columns.Count;

                    for (int j = 4; j <= num_records; j++)
                    {
                        if (values[j, 3] != null)
                        {
                            double d = double.Parse(values[j, 2].ToString());
                            DateTime dt = DateTime.FromOADate(d);
                            unemploymentRate.Add(dt, double.Parse(values[j, 3].ToString()));
                        }
                        if (values[j, 6] != null)
                        {
                            double d = double.Parse(values[j, 5].ToString());
                            DateTime dt = DateTime.FromOADate(d);
                            consumerPriceIndex.Add(dt, double.Parse(values[j, 6].ToString()));
                        }
                    }
                    break;
                }
                excelWorkBook.Close(false);
                excelApp.Quit();
            }
        }

        public static List<DateValueItem> fillTableForCountry(int countryId)
        {
            DBConnect db_obj = new DBConnect("kensee");
            List<DateValueItem> msl = db_obj.readSentiments(countryId);

            DateTime dtNow = DateTime.Now;
            DateTime dt1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);

            DateTime startDateY3 = dt1.AddYears(-3);
            startDateY3 = startDateY3.AddDays((8 - (int)startDateY3.DayOfWeek) % 7);

            var mslResult = fillTableForPeriod(msl, startDateY3, true, 36);
            if (mslResult.Count != 0)
            {
                mslResult[0].sentimemt_index_period_smoothed = mslResult[0].value;
                if (mslResult.Count > 1)
                    mslResult[1].sentimemt_index_period_smoothed = (mslResult[1].value + mslResult[0].value) / 2;
                for (int i = 2; i < mslResult.Count; i++)
                    mslResult[i].sentimemt_index_period_smoothed = (mslResult[i].value + mslResult[i - 1].value + mslResult[i - 2].value) / 3;
            }
            return mslResult;
        }

        public static List<DateValueItem> fillTableForPeriod(List<DateValueItem> msl, DateTime startDate, bool weeks, int period)
        {
            List<DateValueItem> mslResult = new List<DateValueItem>();
            DateTime currDate = startDate;
            DateTime prevDate = startDate;
            double sentiment = 0;
            int count = 0;
            DateTime first = DateTime.MinValue;
            bool toAdd = false;
            for (int jj = 0; jj <= msl.Count; jj++)
            {
                DateValueItem x = null;
                if (jj < msl.Count)
                {
                    x = msl[jj];
                    if (x.date <= startDate)
                        continue;
                }
                while (jj == msl.Count || x.date >= currDate)
                {
                    if (toAdd || prevDate != currDate)
                    {
                        if (!toAdd)
                            first = prevDate;

                        first = first.AddDays(1 - (int)first.DayOfWeek);
                        mslResult.Add(new DateValueItem(first, count == 0 ? 0 : sentiment / count));
                        toAdd = false;
                        sentiment = 0;
                        count = 0;
                    }
                    prevDate = currDate;
                    if (weeks)
                        currDate = currDate.AddDays(7);
                    else
                        currDate = currDate.AddMonths(1);
                    if (jj == msl.Count)
                        break;
                }
                if (jj == msl.Count)
                    break;
                sentiment += x.value;
                count++;
                if (!toAdd)
                    first = x.date;
                toAdd = true;
            }
            return mslResult;
        }

        public static void calculateRegeression(string companyName, List<DateValueItem> snmts, List<DateValueItem> spd, Dictionary<DateTime, double> unemploymentRate, Dictionary<DateTime, double> consumerPriceIndex)
        {
            calculateRegeression(companyName, snmts, spd, spd[0].date, spd[0].date.AddYears(1), unemploymentRate, consumerPriceIndex);
            calculateRegeression(companyName, snmts, spd, spd[0].date.AddYears(1), spd[0].date.AddYears(2), unemploymentRate, consumerPriceIndex);
            calculateRegeression(companyName, snmts, spd, spd[0].date.AddYears(2), spd[0].date.AddYears(3), unemploymentRate, consumerPriceIndex);
        }

        public static void calculateRegeression(string companyName, List<DateValueItem> snmts, List<DateValueItem> spd, DateTime from, DateTime to, 
            Dictionary<DateTime, double> unemploymentRate, Dictionary<DateTime, double> consumerPriceIndex)
        {
            DateTime fd = DateTime.MinValue;
            DateTime ld = DateTime.MinValue;

            List<double> l1 = new List<double>();
            List<double> l2 = new List<double>();
            List<double> l3 = new List<double>();
            List<double> l4 = new List<double>();
            int i1 = 0;
            int i2 = 0;
            while (i1 < spd.Count && i2 < snmts.Count)
            {
                if (spd[i1].date > to && snmts[i2].date > to)
                    break;
                if (spd[i1].date < from)
                {
                    i1++;
                    continue;
                }
                if (snmts[i2].date < from)
                {
                    i2++;
                    continue;
                }
                if (spd[i1].date == snmts[i2].date)
                {
    //                if (snmts[i2].sentimemt_index_period_smoothed != 0)
                    {
                        if (fd == DateTime.MinValue)
                            fd = spd[i1].date;
                        ld = spd[i1].date;
                        l1.Add(spd[i1].value);
                        l2.Add(snmts[i2].sentimemt_index_period_smoothed);
                        double v1 = 0;
                        double v2 = 0;
                        DateTime dt = spd[i1].date;
                        for (int i = 0; i < 7; i++)
                        {
                            DateTime dt1 = new DateTime(dt.Year, dt.Month, 1);
                            if (unemploymentRate.ContainsKey(dt1))
                                v1 += unemploymentRate[dt1] / DateTime.DaysInMonth(dt.Year, dt.Month);
                            if (consumerPriceIndex.ContainsKey(dt1))
                                v2 += consumerPriceIndex[dt1] / DateTime.DaysInMonth(dt.Year, dt.Month);
                            dt = dt.AddDays(1);
                        }
                        l3.Add(v1);
                        l4.Add(v2);
                    }
                    i1++;
                    i2++;
                    continue;
                }
                if (spd[i1].date < snmts[i2].date)
                {
                    i1++;
                    continue;
                }
                i2++;
                continue;
            }
            logger.WriteLine("  \nCalculation for weeks from " + fd.ToString("dd/MM/yyyy") + " to " + ld.ToString("dd/MM/yyyy") );
            logger.WriteLine("  Count of points - " + l1.Count + "\n");

            int n = l1.Count;
            double[] y = new double[n];
            double[,] fmatrix = new double[n, 4];
            for (int i = 0; i < n; i++)
            {
                y[i] = l1[i];
                fmatrix[i, 0] = 1;
                fmatrix[i, 1] = l2[i];
                fmatrix[i, 2] = l3[i];
                fmatrix[i, 3] = l4[i];
            }
            int info = 0;
            double[] c;
            alglib.lsfitreport rep;

            alglib.lsfitlinear(y, fmatrix, out info, out c, out rep);
            outResult(rep, info, c);
        }

        static void model1(string[] args)
        {
            var prices = readPrices(args[0]);
            var sentiments = readSentiments(args[1]);
            prices = convertToWeeks(prices);
            sentiments = convertToWeeks(sentiments);
            int n = prices.Count;
            if (sentiments.Count < n)
                n = sentiments.Count;
            double[] y = new double [n];
            double[,] fmatrix = new double[n, 1];
            for (int i = 0; i < n; i++)
            {
                y[i] = prices[i].value;
                fmatrix[i, 0] = sentiments[i].value;
            }
            int info = 0;
            double[] c;
            alglib.lsfitreport rep;

            alglib.lsfitlinear(y, fmatrix, out info, out c, out rep);
            outResult(rep, info, c);

            double[,] fmatrix1 = new double[n, 2];
            for (int i = 0; i < n; i++)
            {
                fmatrix1[i, 0] = 1;
                fmatrix1[i, 1] = sentiments[i].value;
            }
            alglib.lsfitlinear(y, fmatrix1, out info, out c, out rep);
            outResult(rep, info, c);

            Random rnum = new Random();
            for (int i = 0; i < n; i++)
            {
                fmatrix1[i, 0] = 1;
                fmatrix1[i, 1] = y[i] + rnum.Next(1, 1000) / 1000;
            }
            alglib.lsfitlinear(y, fmatrix1, out info, out c, out rep);
            outResult(rep, info, c);

            logger.WriteLine("\n");
        }

        static void outResult(alglib.lsfitreport rep, int info, double[] c)
        {
            logger.WriteLine("result is:\n");
            logger.WriteLine("  info = " + info);
            for (int k = 0; k < c.Length; k++)
            {
                logger.WriteLine("  c[" + k + "] = " + c[k]);
            }
            logger.WriteLine("  rep.taskrcond = " + rep.taskrcond);
            logger.WriteLine("  rep.r2 = " + rep.r2);
            logger.WriteLine("  rep.rmserror = " + rep.rmserror);
            logger.WriteLine("  rep.avgerror = " + rep.avgerror);
            logger.WriteLine("  rep.avgrelerror = " + rep.avgrelerror);
            logger.WriteLine("  rep.maxerror = " + rep.maxerror);
            logger.WriteLine("\n");
        }

        static List<DateValue> readSentiments(string fileName)
        {
            var res = new List<DateValue>();
            var arr = File.ReadAllLines(fileName);
            foreach (var x in arr)
            {
                var ax = x.Split(new char[] { ',' });
                string txt = ax[2].Replace('\"', ' ').Trim();
                DateTime dateResult;
                if (!DateTime.TryParse(txt, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult))
                    continue;
                res.Add(new DateValue { date = dateResult, value = Convert.ToDouble(ax[3]) });
            }
            return res;
        }

        static List<DateValue> readPrices(string fileName)
        {
            var res = new List<DateValue>();
            var arr = File.ReadAllLines(fileName);
            foreach (var x in arr)
            {
                var ax = x.Split(new char[] { ',' });
                string txt = ax[0].Trim();
                DateTime dateResult;
                if (!DateTime.TryParse(txt, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult))
                    continue;
                res.Add(new DateValue { date = dateResult, value = Convert.ToDouble(ax[4]) });
            }
            res.Sort(delegate(DateValue x, DateValue y)
            {
                return x.date.CompareTo(y.date);
            });
            return res;
        }

        static List<DateValue> convertToWeeks(List<DateValue> dv)
        {
            DateTime dt = new DateTime(2015, 1, 1);
            dt = dt.AddDays(7 - (int)dt.DayOfWeek);
            var res = new List<DateValue>();
            double vl = 0;
            int cnt = 0;
            for (int i = 0; i < dv.Count; i++)
            {
                if (dv[i].date < dt)
                    continue;
                if (dv[i].date >= dt.AddDays(7))
                {
                    if (cnt != 0)
                        vl = vl / cnt;
                    res.Add(new DateValue { date = dt, value = vl });
                    cnt = 0;
                    vl = 0;
                    dt = dt.AddDays(7);
                }
                cnt++;
                vl += dv[i].value;
            }
            if (cnt != 0)
            {
                vl = vl / cnt;
                res.Add(new DateValue { date = dt, value = vl });
            }
            return res;
        }
    }

    public class DateValue
    {
        public DateTime date;
        public double value;
    }
}
