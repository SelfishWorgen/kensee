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
    public class CompanyKeys
    {
        public Dictionary<string, CompanyKeys> keys;
        public string name;
    }

    public partial class ReEventsParser
    {
        public CompanyKeys additionalCompanies;

        private void readCompanyFile()
        {
            additionalCompanies = new CompanyKeys { keys = new Dictionary<string, CompanyKeys>(), name = "" };

            IEnumerable<ExcelReader.worksheet> sheets = null;
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(opt.companiesFileName);
                int sheetNumber = -1;

                foreach (ExcelReader.worksheet worksheet in sheets)
                {
                    sheetNumber++;
                    if (sheetNumber == 0)
                    {
                        int columnCount = worksheet.Rows[0].Cells.Length;
                        var row = worksheet.Rows[0];
                        int indexOfCompany = -1;
                        int j = 0;
                        for (j = 0; j < columnCount; j++)
                        {
                            if (worksheet.Rows[0].Cells[j].Text == "Company Name")
                                indexOfCompany = j;
                        }
                        for (int i = 1; i < worksheet.Rows.Length; i++)
                        {
                            row = worksheet.Rows[i];
                            string companyName = row.Cells[indexOfCompany].Text.Trim();
                            addCompanyNameToDictionary(companyName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status += ex.Message;
            }
        }

        private void addCompanyNameToDictionary(string str)
        {
            List<Token> tokens = tokenParser.GetTokensForText(str);
            int k = tokens.Count;
            CompanyKeys x = additionalCompanies;
            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new CompanyKeys { keys = new Dictionary<string, CompanyKeys>(), name = "" };
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                    x.name = str;
            }
        }

        private void extractCompaniesFromTokenList(List<Token> tokens)
        {
            for (int i = 0; i < tokens.Count; )
            {
                string s = findCompany(tokens, i, additionalCompanies);
                if (!string.IsNullOrEmpty(s))
                {
                    if (!companiesList.Contains(s))
                        companiesList.Add(s);
                    i += tokenParser.GetTokensForText(s).Count;
                }
                else
                {
                    i++;
                }
            }
        }

        private string findCompany(List<Token> tokens, int index, CompanyKeys companyKeys)
        {
            string str = tokens[index].token.ToLower();
            if (!companyKeys.keys.ContainsKey(str))
                return "";
            CompanyKeys y = companyKeys.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
                return companyKeys.name;

            if (index == tokens.Count - 1)
                return companyKeys.name;

            string s = findCompany(tokens, index + 1, y);

            if (!string.IsNullOrEmpty(s))
                return s;
            return companyKeys.name;
        }

    }

}

