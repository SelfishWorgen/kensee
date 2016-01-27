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
        public void initForTransform()
        {
            eventRules.Clear();
            ReEventRule rule;
            ReEventRuleKey ruleKey;
            rule = new ReEventRule
            {
                name = "BUY/SELL",
                inWholeText = true,
            };

            ruleKey = getRuleKeyByName("$BUY/SELL");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                eventRules.Add(rule);
            }

            rule = new ReEventRule
            {
                name = "RENT",
                inWholeText = true,
            };
            ruleKey = getRuleKeyByName("$RENT");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                eventRules.Add(rule);
            }

            rule = new ReEventRule
            {
                name = "CONSTRUCT",
                inWholeText = true,
            };
            ruleKey = getRuleKeyByName("$CONSTRUCT");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                eventRules.Add(rule);
            }

            rule = new ReEventRule
            {
                name = "PROPERTY",
                inWholeText = true,
            };
            ruleKey = getRuleKeyByName("$PROPERTY");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                eventRules.Add(rule);
            }

            rule = new ReEventRule
            {
                name = "FINANCIAL",
                inWholeText = true,
            };
            ruleKey = getRuleKeyByName("$FINANCIAL");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                eventRules.Add(rule);
            }

            rule = new ReEventRule
            {
                name = "BUY/SELL&PROPERTY:SENT",
                inWholeText = false,
            };
            ruleKey = getRuleKeyByName("$BUY/SELL");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                ruleKey = getRuleKeyByName("$PROPERTY");
                if (ruleKey != null)
                {
                    rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                    eventRules.Add(rule);
                }
            }

            rule = new ReEventRule
            {
                name = "RENT&PROPERTY:SENT",
                inWholeText = false,
            };
            ruleKey = getRuleKeyByName("$RENT");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                ruleKey = getRuleKeyByName("$PROPERTY");
                if (ruleKey != null)
                {
                    rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                    eventRules.Add(rule);
                }
            }
                        
            rule = new ReEventRule
            {
                name = "CONSTRUCT&PROPERTY:SENT",
                inWholeText = false,
            };
            ruleKey = getRuleKeyByName("$CONSTRUCT");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                ruleKey = getRuleKeyByName("$PROPERTY");
                if (ruleKey != null)
                {
                    rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                    eventRules.Add(rule);
                }
            }

            rule = new ReEventRule
            {
                name = "FINANCIAL&PROPERTY:DOC",
                inWholeText = true,
            };
            ruleKey = getRuleKeyByName("$FINANCIAL");
            if (ruleKey != null)
            {
                rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                ruleKey = getRuleKeyByName("$PROPERTY");
                if (ruleKey != null)
                {
                    rule.ruleKeys.Add(new ReEventRuleKey(ruleKey));
                    eventRules.Add(rule);
                }
            }
        }


        public void transformFile(string fileName)
        {
            IEnumerable<ExcelReader.worksheet> sheets = null;
            string dir = new FileInfo(fileName).Directory.FullName;
            string outFileName = Path.Combine(dir, "manualEventClsf.xlsx");
            List<tempClass> tc = new List<tempClass>();
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(fileName);
                int sheetNumber = -1;

                foreach (ExcelReader.worksheet worksheet in sheets)
                {
                    sheetNumber++;
                    int columnCount = worksheet.Rows[0].Cells.Length;
                    var row = worksheet.Rows[0];
                    int iId = -1;
                    int iUrl = -1;
                    int iHeadLine = -1;
                    int iEventManual = -1;
                    for (int j = 0; j < columnCount; j++)
                    {
                        string s = worksheet.Rows[0].Cells[j].Text;
                        if (s == "ID")
                            iId = j;
                        if (s == "URL")
                            iUrl = j;
                        if (s == "Title")
                            iHeadLine = j;
                        if (s == "Event - manual (by hanan)")
                            iEventManual = j;
                        if (s == "Manual Tag")
                            iEventManual = j;
                        if (s == "Manual tag")
                            iEventManual = j;
                    }
                    for (int i = 1; i < worksheet.Rows.Length; i++)
                    {
                        row = worksheet.Rows[i];
                        string id = "";
                        string url = "";
                        string headline = "";
                        string eventmanual = "";
                        foreach (var c in row.Cells)
                        {
                            if (c.ColumnIndex == iId)
                                id = c.Text;
                            if (c.ColumnIndex == iUrl)
                                url = c.Text;
                            if (c.ColumnIndex == iHeadLine)
                                headline = c.Text;
                            if (c.ColumnIndex == iEventManual)
                                eventmanual = c.Text;
                        }
                        if (string.IsNullOrEmpty(id))
                            id = i.ToString();
                        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(headline) && !string.IsNullOrEmpty(eventmanual))
                        {
                            tc.Add(new tempClass { ID = id, URL = url, EVENT_MANUAL = eventmanual, HEADLINE = headline, CONTENT = "" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status += ex.Message;
                return;
            }

            foreach (tempClass t in tc)
            {
                clearResults();
                stringContent = "";
                stringTitle = "";
                string url = t.URL.Trim();
                string ext = Path.GetExtension(url).ToLower();
                if (ext == ".pdf")
                    continue;
                nBoilerPipeUsed = true;
                WebResponse response = null;
                try
                {
                    WebRequest request = WebRequest.Create(t.URL.Trim());
                    response = request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    response.Close();
                    HtmlConverter.ConvertToTextAndTitle(responseFromServer, 0, out stringContent, out stringTitle);
                }
                catch (Exception ex)
                {
                    stringContent = ex.Message;
                    if (response != null)
                        response.Close();
                    stringTitle = "";
                    continue;
                }

                string cleanText = "";
                if (!string.IsNullOrEmpty(stringTitle))
                {
                    stringTitle = WebUtility.HtmlDecode(stringTitle);
                    stringTitle = stringTitle.Replace("\"", "");
                    stringTitle = stringTitle.Replace("'", "");
                    stringTitle = stringTitle.Replace("\n", " ");
                    stringTitle = stringTitle.Trim();
                    if (stringTitle != "")
                        cleanText = stringTitle + " ";
                }
                if (!string.IsNullOrEmpty(stringContent))
                {
                    stringContent = WebUtility.HtmlDecode(stringContent);
                    stringContent = stringContent.Replace("\"", "");
                    stringContent = stringContent.Replace("'", "");
                    stringContent = stringContent.Replace("\n", " ");
                    if (stringContent != "")
                        cleanText += stringContent;

                    sentenceStrings.InsertRange(0, stringContent.Split(new char[] { '.', '?' }));
                    if (!string.IsNullOrEmpty(stringTitle))
                    {
                        sentenceStrings.Insert(0, stringTitle);
                    }
                }
                if (string.IsNullOrEmpty(cleanText))
                    continue;
                t.CONTENT = cleanText;
                analyzeForTransform();
                foreach (ReEventRule eventRule in eventRules)
                {
                    fillFieldByRule(eventRule, t);
                }
            }

            ExcelWriter.ExcelWriter excelWriter = new ExcelWriter.ExcelWriter(outFileName);
            excelWriter.newSheet("1", columnSizeList());

            var newRow = excelWriter.NewRow();
            excelWriter.AddCellToCurrentRow("ID", 1);
            excelWriter.AddCellToCurrentRow("URL", 1);
            excelWriter.AddCellToCurrentRow("HEADLINE", 1);
            excelWriter.AddCellToCurrentRow("CONTENT", 1);
            excelWriter.AddCellToCurrentRow("EVENT_MANUAL", 1);
            excelWriter.AddCellToCurrentRow("BUY/SELL", 1);
            excelWriter.AddCellToCurrentRow("RENT", 1);
            excelWriter.AddCellToCurrentRow("CONSTRUCT", 1);
            excelWriter.AddCellToCurrentRow("PROPERTY", 1);
            excelWriter.AddCellToCurrentRow("FINANCIAL", 1);
            excelWriter.AddCellToCurrentRow("BUY/SELL&PROPERTY:SENT", 1);
            excelWriter.AddCellToCurrentRow("RENT&PROPERTY:SENT", 1);
            excelWriter.AddCellToCurrentRow("CONSTRUCT&PROPERTY:SENT", 1);
            excelWriter.AddCellToCurrentRow("FINANCIAL&PROPERTY:DOC", 1);
            foreach (tempClass t in tc)
            {
                if (!string.IsNullOrEmpty(t.CONTENT))
                {
                    excelWriter.NewRow();
                    excelWriter.AddCellToCurrentRow(t.ID, 1);
                    excelWriter.AddCellToCurrentRow(t.URL, 1);
                    excelWriter.AddCellToCurrentRow(t.HEADLINE, 1);
                    excelWriter.AddCellToCurrentRow(t.CONTENT, 1);
                    excelWriter.AddCellToCurrentRow(t.EVENT_MANUAL, 1);
                    excelWriter.AddCellToCurrentRow(t.BUY_SELL, 1);
                    excelWriter.AddCellToCurrentRow(t.RENT, 1);
                    excelWriter.AddCellToCurrentRow(t.CONSTRUCT, 1);
                    excelWriter.AddCellToCurrentRow(t.PROPERTY, 1);
                    excelWriter.AddCellToCurrentRow(t.FINANCIAL, 1);
                    excelWriter.AddCellToCurrentRow(t.BUY_SELL_PROPERTY_SENT, 1);
                    excelWriter.AddCellToCurrentRow(t.RENT_PROPERTY_SENT, 1);
                    excelWriter.AddCellToCurrentRow(t.CONSTRUCT_PROPERTY_SENT, 1);
                    excelWriter.AddCellToCurrentRow(t.FINANCIAL_PROPERTY_DOC, 1);
                    excelWriter.addRow();
                }
            }
            excelWriter.Save();
        }

        private List<int> columnSizeList()
        {
            var l = new List<int>();
            l.Add(15); //ID
            l.Add(180); //URL
            l.Add(150); //HEADLINE
            l.Add(150); //CONTENT
            l.Add(15); //EVENT_MANUAL
            l.Add(20); //BUY_SELL;
            l.Add(15); //RENT;
            l.Add(15); //CONSTRUCT;
            l.Add(15); //PROPERTY;
            l.Add(15); //FINANCIAL;
            l.Add(25); //BUY_SELL_PROPERTY_SENT;
            l.Add(25); //RENT_PROPERTY_SENT;
            l.Add(25); //CONSTRUCT_PROPERTY_SENT;
            l.Add(25); //FINANCIAL_PROPERTY_DOC;
            return l;
        }

        private void analyzeForTransform()
        {
            for (int i = 0; i < sentenceStrings.Count; i++)
            {
                string sentence = sentenceStrings[i];
                List<Token> tokens = tokenParser.GetTokensForText(sentence);
                List<string> strList = new List<string>();
                foreach (Token t in tokens)
                {
                    string s = Stemmer.Convert(t.token.ToLower());
                    strList.Add(s);
                }
                foreach (ReEventRule eventRule in eventRules)
                {
                    eventRule.fireRule(strList, true, "", sentence);
                }
            }
        }

        private void fillFieldByRule(ReEventRule eventRule, tempClass t)
        {
            string prefix = eventRule.checkRule() == "" ? "NO_" : "";
            switch (eventRule.name)
            {
                case "BUY/SELL":
                    t.BUY_SELL = prefix + eventRule.name;
                    break;
                case "RENT":
                    t.RENT = prefix + eventRule.name;
                    break;
                case "CONSTRUCT":
                    t.CONSTRUCT = prefix + eventRule.name;
                    break;
                case "PROPERTY":
                    t.PROPERTY = prefix + eventRule.name;
                    break;
                case "FINANCIAL":
                    t.FINANCIAL = prefix + eventRule.name;
                    break;
                case "BUY/SELL&PROPERTY:SENT":
                    t.BUY_SELL_PROPERTY_SENT = prefix + eventRule.name;
                    break;
                case "RENT&PROPERTY:SENT":
                    t.RENT_PROPERTY_SENT = prefix + eventRule.name;
                    break;
                case "CONSTRUCT&PROPERTY:SENT":
                    t.CONSTRUCT_PROPERTY_SENT = prefix + eventRule.name;
                    break;
                case "FINANCIAL&PROPERTY:DOC":
                    t.FINANCIAL_PROPERTY_DOC = prefix + eventRule.name;
                    break;
            }
        }


    }

    public class tempClass
    {
        public string ID;
        public string URL;
        public string HEADLINE;
        public string CONTENT;
        public string EVENT_MANUAL;
        public string BUY_SELL;
        public string RENT;
        public string CONSTRUCT;
        public string PROPERTY;
        public string FINANCIAL;
        public string BUY_SELL_PROPERTY_SENT;
        public string RENT_PROPERTY_SENT;
        public string CONSTRUCT_PROPERTY_SENT;
        public string FINANCIAL_PROPERTY_DOC;
    }
}