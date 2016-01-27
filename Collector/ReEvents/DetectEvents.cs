using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{

    public class ReEventsTagTypeValues
    {
        public int eventId;
        public string eventName;
    }

    public class DetectEvents
    {
        private List<ReEventRule> eventRules;
        private List<ReEventRuleKey> allKeysList;

        //result
        public List<int> eventIds;
        private string resultEvents;
        //public string snippet;
        //public int snippetSentenceNumber;

        public DetectEvents()
        {
            eventRules = new List<ReEventRule>();
            allKeysList = new List<ReEventRuleKey>();
            eventIds = new List<int>();
            resultEvents = "";
            //snippet = "";
            //snippetSentenceNumber = -1;
        }

        public void clear()
        {
            foreach (ReEventRule r in eventRules)
            {
                r.clear();
            }
            eventIds = new List<int>();
            resultEvents = "";
        }

        public string Init(string definitionsFileName, List<ReEventsTagTypeValues> eventNameList)
        {
            string res = readDefinitionFile(definitionsFileName);
            if (res != "")
                return res;
            Dictionary<string, int> evDict = new Dictionary<string, int>();
            foreach (ReEventsTagTypeValues re in eventNameList)
            {
                evDict.Add(re.eventName.ToLower(), re.eventId);
            }

            foreach (ReEventRule r in eventRules)
            {
                if (evDict.ContainsKey(r.name.ToLower()))
                    r.codeEvent = evDict[r.name.ToLower()];
                //else
                    // to delete event? save log ?
            }

            return "";
        }

        private string readDefinitionFile(string definitionsFileName)
        {
            string result = "";
            IEnumerable<ExcelReader.worksheet> sheets = null;
            try
            {
                sheets = ExcelReader.Workbook.Worksheets(definitionsFileName);
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
                                if (string.IsNullOrEmpty(worksheet.Rows[i].Cells[j].Text))
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
                        fillRuleFromCell(worksheet.Rows[i].Cells[2].Text, out eventRule.inWholeText, out eventRule.hasCompany, eventRule.ruleKeys);
                        if (worksheet.Rows[i].Cells.Length >= 3 && worksheet.Rows[i].Cells[3].ColumnIndex == 3)
                        {
                            if (!string.IsNullOrEmpty(worksheet.Rows[i].Cells[3].Text) && worksheet.Rows[i].Cells[3].Text != "NO")
                            {
                                eventRule.haveNegative = true;
                                bool hasCompany = false;
                                fillRuleFromCell(worksheet.Rows[i].Cells[3].Text, out eventRule.inWholeTextNeg, out hasCompany, eventRule.ruleKeysNeg);
                            }
                        }
                        eventRules.Add(eventRule);
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Cannot load definition file " + ex.Message;
            }
            return result;
        }

        private void fillRuleFromCell(string str, out bool inWholeText, out bool hasCompany, List<ReEventRuleKey> ruleKeys)
        {
            str = str.Trim();
            inWholeText = str.IndexOf(":DOC") >= 0;
            hasCompany = false;
            string s = str;
            while (true)
            {
                int i1 = s.IndexOf("$");
                if (i1 < 0)
                    i1 = s.IndexOf("#");
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
                if (key == "#COMPANY")
                    hasCompany = true;
                if (key == "$CURRENCY" && eventRuleKey != null)
                    eventRuleKey.isCurrency = true;
                if (key == "$PROPERTY" && eventRuleKey != null)
                    eventRuleKey.isProperty = true;
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

        public void extractEventFromTokenList(List<Token> tokens, List<Token> stemmedTokens, ContentSentence sentence, bool companyWasFound)
        {
            foreach (ReEventRule eventRule in eventRules)
            {
                if (eventRule.hasCompany && !companyWasFound)
                {
                    eventRule.allRuleText = "No one company was found";
                    continue;
                }
                eventRule.countAllRuleKeys(tokens, stemmedTokens, sentence);
            }
        }

        public void getEvents()
        {
            List<ReEventRule> cntList = new List<ReEventRule>();
            int priority = 0;
            foreach (ReEventRule eventRule in eventRules)
            {
                cntList.Add(eventRule);
                eventRule.fixCounters();
                eventRule.priority = ++priority;
            }
            cntList.Sort();
            if (cntList.Count > 0 && cntList.Last().counter > 0)
            {
                ReEventRule r = cntList.Last();
                //snippetSentenceNumber = r.snippetSentenceNumber();
                eventIds.Add(r.codeEvent);
                resultEvents = r.name;
                cntList.Remove(r);
                if (cntList.Last().counter > 0)
                {
                    r = cntList.Last();
                    eventIds.Add(r.codeEvent);
                    resultEvents += "|";
                    resultEvents += r.name;
                }
            }
        }

        public bool hasProperties()
        {
            foreach (ReEventRule eventRule in eventRules)
            {
                foreach (ReEventRuleKey eventRuleKey in eventRule.ruleKeys)
                {
                    if (eventRuleKey.isProperty && eventRuleKey.ruleKeyFound)
                        return true;
                }
            }
            return false;
        }

        public void writeEventsToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        {
            foreach (ReEventRuleKey ruleKey in allKeysList)
            {
                foreach (ReEventRule rule in eventRules)
                {
                    bool found = false;
                    foreach (ReEventRuleKey rk in rule.ruleKeys)
                    {
                        if (rk.name == ruleKey.name)
                        {
                            writeLineToResult(rk.name, rk.allRuleKeys);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
            foreach (ReEventRule rule in eventRules)
            {
                writeLineToResult(rule.name, rule.allRuleText);
            }
            writeLineToResult("EVENT - DB", resultEvents); 
        }


        public void setFinancialEvent(string txt)
        {
            foreach (ReEventRule rule in eventRules)
            {
                if (rule.name == "Financial update")
                {
                    eventIds.Add(rule.codeEvent);
                    rule.allRuleText = "Information from the title";
                }
            }
            resultEvents = "Financial update";
            //snippet = txt.Trim();
        }

        public List<int> getSentenceNumbersForEvent()
        {
            if (eventIds.Count == 0)
                return null;
            int eventId = eventIds[0];
            foreach (ReEventRule rule in eventRules)
            {
                if (rule.codeEvent == eventId)
                    return rule.sentenceNumbers;
            }
            return null;
        }

        public string getEventName(int id)
        {
            foreach (ReEventRule rule in eventRules)
            {
                if (rule.codeEvent == id)
                    return rule.name;
            }
            return "";
        }
    }
}
