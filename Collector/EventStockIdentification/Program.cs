using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace EventStockIdentification
{
    class Program
    {
        static void Main(string[] args)
        {
            readStocks();
        }

        static void readStocks()
        {
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
            
            DBConnect db_obj = new DBConnect("kensee");
            var revenueIncreaseArticles = db_obj.readArticleIdsForEvent(19);
            var revenueDecreaseArticles = db_obj.readArticleIdsForEvent(20);

            System.Console.WriteLine("All articles with Revenue Increase event - " + revenueIncreaseArticles.Count);
            System.Console.WriteLine("All articles with Revenue Decrease event - " + revenueDecreaseArticles.Count);
            System.Console.WriteLine("\n");

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
                        System.Console.WriteLine("Company \"" + company + "\"");
                    //    int companyId = db_obj.readCompanyId(company);
                        if (companyId == 0)
                        {
                            System.Console.WriteLine("Company was not found in Kensee Database");
                            System.Console.WriteLine("\n");
                            continue;
                        }
                        var x = new Dictionary<DateTime, float>();
                        dict.Add(company, x);
                        var companyArticles = db_obj.readArticleIdsForCompany(companyId);
                        System.Console.WriteLine("Articles count - " + companyArticles.Count);
                        List<DateTime> reIncreaseArt = new List<DateTime>();
                        List<DateTime> reDecreaseArt = new List<DateTime>();
                        foreach (var y in revenueIncreaseArticles)
                        {
                            foreach (var y1 in companyArticles)
                            {
                                if (y == y1)
                                {
                                    reIncreaseArt.Add(db_obj.readDateTime(y));
                                    break;
                                }
                            }
                        }
                        foreach (var y in revenueDecreaseArticles)
                        {
                            foreach (var y1 in companyArticles)
                            {
                                if (y == y1)
                                {
                                    reDecreaseArt.Add(db_obj.readDateTime(y));
                                    break;
                                }
                            }
                        }
                        System.Console.WriteLine("Articles with Revenue Increase event - " + reIncreaseArt.Count);
                        System.Console.WriteLine("Articles with Revenue Decrease event - " + reDecreaseArt.Count);
                        for (int j = 4; j <= num_records; j++)
                        {
                            if (values[j, i] != null)
                            {
                                string dts = values[j, 1].ToString();
                                double d = double.Parse(values[j, 1].ToString());
                                DateTime dt = DateTime.FromOADate(d);
                                x.Add(dt, Convert.ToSingle(values[j, i].ToString()));
                            }
                        }
                        calculateList(reIncreaseArt, "Revenue Increase", x);
                        calculateList(reDecreaseArt, "Revenue Decrease", x);                
                        System.Console.WriteLine("\n");
                    }
                    break;
                }
                excelWorkBook.Close(false);
                excelApp.Quit();
            }
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine("error: " + ex.Message);
            //}
        }

        static void calculateList(List<DateTime> list, string eventName, Dictionary<DateTime, float> dict)
        {
            foreach (var dt in list)
            {
                System.Console.WriteLine("\nEvent " + eventName + " at " + dt.ToString("dd/MM/yyyy"));
                calculateDate(dt.AddDays(-1), dt, dict);
                calculateDate(dt.AddDays(-1), dt.AddDays(1), dict);
                calculateDate(dt.AddDays(-1), dt.AddDays(5), dict);
            }
        }

        static void calculateDate(DateTime from, DateTime to, Dictionary<DateTime, float> dict)
        {
            if (!dict.ContainsKey(from))
                from = from.AddDays(-1);
            if (!dict.ContainsKey(from))
                from = from.AddDays(-1);
            System.Console.WriteLine("\nCalculation for period " + from.ToString("dd/MM/yyyy") + " to " + to.ToString("dd/MM/yyyy"));
            float value = dict[from];
            int n_up = 0;
            int n_down = 0;
            for (var dt = from.AddDays(1); dt <= to; dt = dt.AddDays(1))
            {
                if (!dict.ContainsKey(dt))
                    continue;
                float value1 = dict[dt];
                if (value1 > value)
                {
                    n_up++;
                    System.Console.WriteLine("Stock went up at " + dt.ToString("dd/MM/yyyy") + " from " + value + " to " + value1);
                }
                else if (value1 < value)
                {
                    n_down++;
                    System.Console.WriteLine("Stock went down at " + dt.ToString("dd/MM/yyyy") + " from " + value + " to " + value1);
                }
                value = value1;
            }
            float up_score = (float)(n_up) / (float)(n_up + n_down);
            float down_score = (float)n_down / (float)(n_up + n_down);
            System.Console.WriteLine("Up_Score = " + up_score);
            System.Console.WriteLine("Down_Score = " + down_score);
        }
    }
}
