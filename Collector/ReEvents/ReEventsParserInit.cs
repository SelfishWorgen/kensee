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
        public void Init()
        {
            if (opt.optionsResult.Length > 0)
            {
                status = opt.optionsResult; // files are missing 
                return;
            }
            tokenParser = new TokenParser(opt.FilesPath);
            readDefinitionFile();
            readCountryFile();
            readCompanyFile();
            downLoadFileName = Path.Combine(opt.FilesPath, "TempPdf" + ".pdf");
        }

        private void readDefinitionFile()
        {
            IEnumerable<ExcelReader.worksheet> sheets = null;
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(opt.definitionsFileName);
                foreach (var worksheet in sheets)
                {
                    int columnCount = worksheet.Rows[0].Cells.Length;
                    for (int i = 0; i < worksheet.Rows.Length; i++)
                    {
                        var row = worksheet.Rows[i];
                        if (i == 0)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                if (worksheet.Rows[i].Cells[j].ColumnIndex < 4)
                                    continue;
                                ReEventRuleKey eventRuleKey = new ReEventRuleKey(worksheet.Rows[i].Cells[j].Text);
                                allKeysList.Add(eventRuleKey);
                            }
                        }
                        else
                        {
                            int cnt = worksheet.Rows[i].Cells.Length < columnCount ? worksheet.Rows[i].Cells.Length : columnCount;
                            for (int j = 0; j < cnt; j++)
                            {
                                if (worksheet.Rows[i].Cells[j].ColumnIndex < 4)
                                    continue;
                                ReEventRuleKey eventRuleKey = allKeysList[worksheet.Rows[i].Cells[j].ColumnIndex - 4];
                                eventRuleKey.addKey(worksheet.Rows[i].Cells[j].Text.Trim());
                            }
                        }
                    }
                    for (int i = 1; i < worksheet.Rows.Length; i++)
                    {
                        if (worksheet.Rows[i].Cells[0].ColumnIndex > 0)
                            break;
                        if (worksheet.Rows[i].Cells.Length <= 1 || worksheet.Rows[i].Cells[1].ColumnIndex > 1)
                            break;
                        if (worksheet.Rows[i].Cells.Length <= 2 || worksheet.Rows[i].Cells[2].ColumnIndex > 2)
                            break;
                        ReEventRule eventRule = new ReEventRule { name = worksheet.Rows[i].Cells[1].Text };
                        fillRuleFromCell(worksheet.Rows[i].Cells[2].Text, out eventRule.inWholeText, out eventRule.checkCompany, eventRule.ruleKeys);
                        if (worksheet.Rows[i].Cells.Length >= 3 && worksheet.Rows[i].Cells[3].ColumnIndex == 3)
                        {
                            eventRule.haveNegative = true;
                            fillRuleFromCell(worksheet.Rows[i].Cells[3].Text, out eventRule.inWholeTextNeg, out eventRule.checkCompanyNeg, eventRule.ruleKeysNeg);
                        }
                        eventRules.Add(eventRule);
                    }
                }
            }
            catch (Exception ex)
            {
                status = "Cannot load definition file " + ex.Message;
            }
        }

        private void fillRuleFromCell(string str, out bool inWholeText, out bool checkCompany, List<ReEventRuleKey> ruleKeys)
        {
            str = str.Trim();
            checkCompany = str.IndexOf("#COMPANY") >= 0;
            inWholeText = str.IndexOf(":DOC") >= 0;
            string s = str;
            while (true)
            {
                int i1 = s.IndexOf("$");
                if (i1 < 0)
                    break;
                int i2 = s.IndexOf(" ", i1);
                if (i2 < 0)
                    i2 = s.IndexOf(")", i1);
                if (i2 < 0)
                    i2 = s.IndexOf(":", i1);
                if (i2 < 0)
                    break;
                string key = s.Substring(i1, i2 - i1);
                ReEventRuleKey eventRuleKey = getRuleKeyByName(key);
                if (eventRuleKey != null)
                    ruleKeys.Add(new ReEventRuleKey(eventRuleKey));
                s = s.Substring(i2);
            }
        }

        private ReEventRuleKey getRuleKeyByName(string nm)
        {
            foreach (ReEventRuleKey eventRuleKey in allKeysList)
            {
                if (nm == eventRuleKey.name)
                {
                    return eventRuleKey;
                }
            }
            return null;
        }


        private void clearResults()
        {
            sentences.Clear();
            companies.Clear(); //exctracted by Senna
            companiesList.Clear(); //extracted by Dictionary
            money.Clear();
            sentenceStrings.Clear();
            reEvent = "";
            textEventConditions = "";
            countryWasFound = false;
            countryList.Clear();
            countryByCityList.Clear();
            countryDebugInfoList1.Clear();
            countryDebugInfoList2.Clear();
            nBoilerPipeUsed = false;
            stringContent = "";
            stringTitle = "";
            foreach (ReEventRule r in eventRules)
            {
                r.clear();
            }
            FileInfo fi = new FileInfo(downLoadFileName);
            if (fi.Exists)
            {
                fi.Delete();
            }
            DirectoryInfo di = new DirectoryInfo(Path.Combine(opt.FilesPath, "TempPdf"));
            if (di.Exists)
            {
                FileInfo[] fs = di.GetFiles();
                foreach (FileInfo f in fs)
                {
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                try
                {
                    di.Delete();
                }
                catch (Exception ex)
                {
                }
            }
        }



    }


}

