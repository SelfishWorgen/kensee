using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Net;
using utl;

namespace ReEvents
{
    public partial class ReEventsParser
    {
        private void readCountryFile()
        {
            Dictionary<string, string> countryCodes = new Dictionary<string, string>();
            IEnumerable<ExcelReader.worksheet> sheets = null;
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(opt.countriesFileName);
                int sheetNumber = -1;

                foreach (ExcelReader.worksheet worksheet in sheets)
                {
                    sheetNumber++;
                    if (sheetNumber == 0)
                    {
                        int columnCount = worksheet.Rows[0].Cells.Length;
                        var row = worksheet.Rows[0];
                        int indexOfCountry = -1;
                        int indexOfCode = -1;
                        int j = 0;
                        for (j = 0; j < columnCount; j++)
                        {
                            if (worksheet.Rows[0].Cells[j].Text == "Country")
                                indexOfCountry = j;
                            if (worksheet.Rows[0].Cells[j].Text == "Country ISO Code")
                                indexOfCode = j;
                        }
                        for (int i = 1; i < worksheet.Rows.Length; i++)
                        {
                            row = worksheet.Rows[i];
                            string country = row.Cells[indexOfCountry].Text;
                            string code = row.Cells[indexOfCode].Text;
                            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(country))
                                break;
                            allCountries.Add(country.ToLower(), country);
                            if (countryCodes.ContainsKey(code))
                                continue;
                            countryCodes.Add(code, country.ToLower());
                        }
                    }
                    else
                    {
                        int columnCount = worksheet.Rows[0].Cells.Length;
                        var row = worksheet.Rows[0];
                        int indexOfCity = -1;
                        int indexOfCode = -1;
                        int j = 0;
                        for (j = 0; j < columnCount; j++)
                        {
                            if (worksheet.Rows[0].Cells[j].Text == "NameWoDiacritics")
                                indexOfCity = j;
                            if (worksheet.Rows[0].Cells[j].Text == "Country Code")
                                indexOfCode = j;
                        }
                        for (int i = 1; i < worksheet.Rows.Length; i++)
                        {
                            row = worksheet.Rows[i];
                            string city = row.Cells[indexOfCity].Text.ToLower();
                            string code = row.Cells[indexOfCode].Text;
                            string country = "";
                            if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(code))
                                continue;
                            if (!countryCodes.ContainsKey(code))
                                continue;
                            if (allCountries.ContainsKey(city))
                                continue;
                            country = countryCodes[code];
                            if (cityCountries.Count == 0 || !cityCountries.ContainsKey(city))
                            {
                                cityCountries.Add(city, country);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status += ex.Message;
            }
        }

        private void extractCountryFromTokenList(List<Token> tokens, string snt)
        {
            if (countryWasFound)
                return;
            for (int i = 0; i < tokens.Count; i++)
            {
                string str = tokens[i].token.ToLower();
                if (allCountries.ContainsKey(str) && countryList.FindIndex(x => x.Equals(str, StringComparison.OrdinalIgnoreCase)) < 0)
                {
                    countryList.Add(allCountries[str]);
                    countryDebugInfoList1.Add(new countryDebugInfo { country = allCountries[str], sentence = snt, city = "", });
                }
                if (i < tokens.Count - 1)
                {
                    string str2 = str + " " + tokens[i + 1].token.ToLower();
                    if (cityCountries.ContainsKey(str2))
                    {
                        string str1 = cityCountries[str2];
                        if (allCountries.ContainsKey(str1) &&
                                countryByCityList.FindIndex(x => x.Equals(str, StringComparison.OrdinalIgnoreCase)) < 0)
                        {
                            countryByCityList.Add(allCountries[str1]);
                            countryDebugInfoList2.Add(new countryDebugInfo { country = allCountries[str1], sentence = snt, city = str2, });
                        }
                        i++;
                        continue;
                    }
                }
                if (cityCountries.ContainsKey(str))
                {
                    string str1 = cityCountries[str];
                    if (allCountries.ContainsKey(str1) &&
                            countryByCityList.FindIndex(x => x.Equals(str, StringComparison.OrdinalIgnoreCase)) < 0)
                    {
                        countryByCityList.Add(allCountries[str1]);
                        countryDebugInfoList2.Add(new countryDebugInfo { country = allCountries[str1], sentence = snt, city = str, });
                    }
                }
            }
        }
    }

    public class countryDebugInfo
    {
        public string country { get; set; }
        public string city { get; set; }
        public string sentence { get; set; }
        public countryDebugInfo()
        {
            country = "";
            city = "";
            sentence = "";
        }
    }

}

