using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace ScannerDB
{
    public class MacroCreator
    {
        public MacroCreator()
        {
            DBConnect db_obj = new DBConnect("kensee");
            db_obj.cleanTable("tbl_macro");
            db_obj.cleanTable("tbl_general_information");
        }

        public void fillTable(Dictionary<int, InfoForSentimentGraph> newResidentialConstructionDict, Dictionary<int, InfoForSentimentGraph> nationalHomePriceIndex)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);
            string fileName = "Dashboard_Economy.xlsx";
            string fullFileName = Path.Combine(directory + "\\..\\data", fileName);
            if (!File.Exists(fullFileName))
            {
                fullFileName = Path.Combine(directory + "\\..\\..\\..\\data", fileName);
                if (!File.Exists(fullFileName))
                   return;
            }
            fillTable(fullFileName, newResidentialConstructionDict);

            fileName = "Case_SHiller_Home_Price_2004-2015.xls";
            fullFileName = Path.Combine(directory + "\\..\\data", fileName);
            if (!File.Exists(fullFileName))
            {
                fullFileName = Path.Combine(directory + "\\..\\..\\..\\data", fileName);
                if (!File.Exists(fullFileName))
                    return;
            }
            fillTable1(fullFileName, 236, nationalHomePriceIndex);
        }

        public void fillTable(string fileName, Dictionary<int, InfoForSentimentGraph> newResidentialConstructionDict)
        {
            Excel.Application excelApp = null;
            Excel.Workbook excelWorkBook = null;
     //       try
            {
                DBConnect db_obj = new DBConnect("kensee");
                Dictionary<DateTime, MacroItem> dict = new Dictionary<DateTime, MacroItem>();
                excelApp = new Excel.Application();
                excelWorkBook = excelApp.Workbooks.Open(fileName);
                int nsheet = 0;
                int countryId = 0;
                DateTime dt;
                string str;
                foreach (Excel.Worksheet worksheet in excelWorkBook.Sheets)
                {
                    Excel.Range range = worksheet.UsedRange;
                    object[,] values = (object[,])range.Value2;
                    int num_records = range.Rows.Count;
                    int num_fields = range.Columns.Count;

                    switch (nsheet)
                    {
                        case 0: // General information
                            countryId = db_obj.getCountryId(values[1, 1].ToString());
                            for (int k = 3; k <= 12; k++)
                                db_obj.storeGeneralInformationToDB(values[k, 1].ToString().Replace(":","").Trim(), values[k, 2].ToString(), countryId);
                            break;
                        case 1: // General Data 
                            str = values[1, 1].ToString();
                            if (str.StartsWith("Country == "))
                                countryId = db_obj.getCountryId(str.Substring("Country == ".Length));
                            int i = 3;
                            if (values[i, 1].ToString() == "Inflation")
                            {
                                i += 2;
                                while (values[i, 1].ToString() != "")
                                {
                                    int year = Convert.ToInt32(values[i, 1].ToString());
                                    for (int i1 = 1; i1 <= 12; i1++)
                                    {
                                        dt = new DateTime(year, i1, 1);
                                        if (values[i, i1 + 1] != null)
                                            dict.Add(dt, new MacroItem { Inflation = Convert.ToSingle(values[i, i1 + 1].ToString()) * 100 });
                                        else
                                            dict.Add(dt, new MacroItem { Inflation = dict[dt.AddMonths(-1)].Inflation });
                                        if (i1 == DateTime.Now.Month && year == DateTime.Now.Year)
                                            break;
                                    }
                                    i++;
                                    if (year == DateTime.Now.Year)
                                        break;
                                }
                            }
                            i++;
                            while (i < num_records)
                            {
                                if (values[i, 1].ToString() == "Unemployment rate")
                                {
                                    i += 2;
                                    while (values[i, 1].ToString() != "")
                                    {
                                        int year = Convert.ToInt32(values[i, 1].ToString());
                                        for (int i1 = 1; i1 <= 12; i1++)
                                        {
                                            dt = new DateTime(year, i1, 1);
                                            if (values[i, i1 + 1] != null)
                                                dict[dt].UnemploymentRate = Convert.ToSingle(values[i, i1 + 1].ToString());
                                            else
                                                dict[dt].UnemploymentRate = dict[dt.AddMonths(-1)].UnemploymentRate;
                                            if (i1 == DateTime.Now.Month && year == DateTime.Now.Year)
                                                break;
                                        }
                                        i++;
                                        if (year == DateTime.Now.Year)
                                            break;
                                    }
                                    break;
                                }
                                i++;
                            }
                            break;
                        case 2: // Housing Data
                            str = values[1, 1].ToString();
                            if (str.StartsWith("Country == "))
                                countryId = db_obj.getCountryId(str.Substring("Country == ".Length));
                            InfoForSentimentGraph newResidentialConstruction = new InfoForSentimentGraph();
                            newResidentialConstructionDict.Add(countryId, newResidentialConstruction);
                            int j = 5;
                            dt = new DateTime(2006, 1, 1);
                            newResidentialConstruction.firstYear = 2006;
                            List<float> newResidentialConstructionYear = null;
                            while (true)
                            {
                                if (dt.Month == 1)
                                {
                                    newResidentialConstructionYear = new List<float>();
                                    newResidentialConstruction.values.Add(newResidentialConstructionYear);
                                }
                                if (values[j, 2] != null)
                                    newResidentialConstructionYear.Add(Convert.ToSingle(values[j, 2].ToString()));

                                if (values[j, 5] != null)
                                    dict[dt].HousePriceIndexChange = Convert.ToSingle(values[j, 5].ToString()) * 100;
                                else
                                    dict[dt].HousePriceIndexChange = dict[dt.AddMonths(-1)].HousePriceIndexChange;
                                if (values[j, 8] != null) 
                                    dict[dt].RateForLodging = Convert.ToSingle(values[j, 8].ToString());
                                else
                                    dict[dt].RateForLodging = dict[dt.AddMonths(-1)].RateForLodging; 
                                if (values[j, 11] != null) 
                                    dict[dt].RateForOffice = Convert.ToSingle(values[j, 11].ToString());
                                else
                                    dict[dt].RateForOffice = dict[dt.AddMonths(-1)].RateForOffice; 
                                if (values[j, 14] != null) 
                                    dict[dt].RateForCommercial = Convert.ToSingle(values[j, 14].ToString());
                                else
                                    dict[dt].RateForCommercial = dict[dt.AddMonths(-1)].RateForCommercial; 
                                if (values[j, 17] != null) 
                                    dict[dt].RateForHealthcare = Convert.ToSingle(values[j, 17].ToString());
                                else
                                    dict[dt].RateForHealthcare = dict[dt.AddMonths(-1)].RateForHealthcare; 
                                if (values[j, 20] != null) 
                                    dict[dt].RateForLeasure = Convert.ToSingle(values[j, 20].ToString());
                                else
                                    dict[dt].RateForLeasure = dict[dt.AddMonths(-1)].RateForLeasure; 
                                if (values[j, 23] != null) 
                                    dict[dt].RateForNonResidential = Convert.ToSingle(values[j, 23].ToString());
                                else
                                    dict[dt].RateForNonResidential = dict[dt.AddMonths(-1)].RateForNonResidential; 
                                if (values[j, 26] != null) 
                                    dict[dt].RateForResidential = Convert.ToSingle(values[j, 26].ToString());
                                else
                                    dict[dt].RateForResidential = dict[dt.AddMonths(-1)].RateForResidential;
                                if (dt.Year == DateTime.Now.Year && dt.Month == DateTime.Now.Month)
                                    break;
                                dt = dt.AddMonths(1);
                                j++;
                            }
                            break;
                    }
                    nsheet++;
                }
                excelWorkBook.Close(false);
                excelApp.Quit();
                db_obj.storeMacroToDB(dict, countryId);
            }
            //catch (Exception ex)
            //{
            //    System.Console.WriteLine("error: " + ex.Message);
            //}
        }

        public void fillTable1(string fileName, int countryId, Dictionary<int, InfoForSentimentGraph> nationalHomePriceIndexDict)
        {
            Excel.Application excelApp = null;
            Excel.Workbook excelWorkBook = null;
            try
            {
                excelApp = new Excel.Application();
                excelWorkBook = excelApp.Workbooks.Open(fileName);
                foreach (Excel.Worksheet worksheet in excelWorkBook.Sheets)
                {
                    InfoForSentimentGraph nationalHomePriceIndex = new InfoForSentimentGraph();
                    nationalHomePriceIndexDict.Add(countryId, nationalHomePriceIndex);
                    DateTime dt = new DateTime(2004, 1, 1);
                    nationalHomePriceIndex.firstYear = 2004;
                    List<float> nationalHomePriceIndexYear = null;
                    Excel.Range range = worksheet.UsedRange;
                    object[,] values = (object[,])range.Value2;
                    int num_records = range.Rows.Count;
                    int num_fields = range.Columns.Count;
                    for (int i = 2; i <= num_records; i++)
                    {
                        float vl = Convert.ToSingle(values[i,2].ToString());
                        if (dt.Month == 1)
                        {
                            nationalHomePriceIndexYear = new List<float>();
                            nationalHomePriceIndex.values.Add(nationalHomePriceIndexYear);
                        }
                        nationalHomePriceIndexYear.Add(vl);
                        dt = dt.AddMonths(1);
                    }
                    break;
                }
                excelWorkBook.Close(false);
                excelApp.Quit();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Cannot load file " + ex.Message);
            }
        }
    }

    public class MacroItem
    {
        public float UnemploymentRate;
        public float Inflation;
        public float HousePriceIndexChange;
        public float RateForLodging;
        public float RateForOffice;
        public float RateForCommercial;
        public float RateForHealthcare;
        public float RateForLeasure;
        public float RateForNonResidential;
        public float RateForResidential;
    }

    public class InfoForSentimentGraph
    {
        public int firstYear;
        public List<List<float>> values;

        public InfoForSentimentGraph() { firstYear = 0; values = new List<List<float>>(); }
    }
}
