using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using utl;

namespace utl
{
    public partial class Indicator
    {
        public void findInTxt(TextContainer txtCont, MatchCollection numeric, List<YearInText> foundYears,
                     ref List<PersonInText> foundPersons, YearInfo[] years)
        {
            if (txtCont.CurrentStr.Trim().StartsWith(name))
                years[0].id += 0;

            if (Name == "WATER WITHDRAWAL")
                years[0].id += 0;
            if (!toFindName)
            {
                if (foundYears.Count == 0)
                {
                    if (YearInText.CurrentYear == 0)
                        return;
                }
                if (numeric.Count == 0)
                    return;
                if (numeric.Count == foundYears.Count)
                    return;
            }
            if ((conversionsList.Count == 0) && string.IsNullOrWhiteSpace(measurmentUnit) && !toFindName)
                return;
            Regex regexKey = new Regex(keysRegexpr, RegexOptions.IgnoreCase);
            MatchCollection foundKeys = regexKey.Matches(txtCont.CurrentStr);
            if (foundKeys.Count == 0)
                return;
            if (strRegexNegKey.Length != 0)
            {
                Regex regexKeyNeg = new Regex(strRegexNegKey, RegexOptions.IgnoreCase);
                MatchCollection foundKeysNeg = regexKeyNeg.Matches(txtCont.CurrentStr);
                if (foundKeysNeg.Count != 0)
                    return;
            }
            if (name == "BOARD MEMBERS")
            {
                string pos = "";
                var tf = txtCont.textFragment(foundKeys[0].Index, ref pos);
                if (tf != null)
                    makeBoardMember(txtCont.CurrentStr, foundKeys[0].Value, years, tf.fontSize, pos);
                return;
            }
            if (toFindName)
            {
                List<PersonYear> persons = new List<PersonYear>();
                foreach (Match fk in foundKeys)
                {
                    if (foundPersons == null)
                        foundPersons = getPersonList(txtCont.CurrentStr);
                    foreach (PersonInText p in foundPersons)
                    {
                        if (isWrongPerson(txtCont.CurrentStr, fk, p))
                            continue;
                        YearInText.setYearToPerson(txtCont.CurrentStr, persons, p, fk, foundYears);
                    }
                }
                addPersonsToResults(txtCont, years, persons);
                return;
            }
            MatchCollection foundUnits = null;
            string commonUnitTxt = "";
            string commonCurrency = "";
            int commonFactor = 1;
            if (!money)
            {
                Regex regexMeasures = new Regex(strRegexUnit, RegexOptions.IgnoreCase);
                foundUnits = regexMeasures.Matches(txtCont.CurrentStr);
            }
            else
            {
                Currency.findCommonCurrencyAndFactor(txtCont.CurrentStr, ref commonFactor, ref commonCurrency, ref commonUnitTxt);
            }

            List<NumericInText> numValues = money ?
                NumericInText.getNumericListMoney(txtCont.CurrentStr, numeric, foundYears) :
                NumericInText.getNumericList(txtCont.CurrentStr, numeric, foundYears, strRegexUnit);

            List<MatchedYearNum> tmpList = new List<MatchedYearNum>();
            for (int i = 0; i < numValues.Count; i++)
            {
                NumericInText numMatch = numValues[i];
                if (Name == "WATER WITHDRAWAL")
                {
                        years[0].id += 0;
                }
                if (isWrongValue(numMatch, txtCont.CurrentStr)) //exclude possible wrongs
                    continue;
                string unit = money ? findCurrency(txtCont.CurrentStr, numMatch, commonCurrency) :
                                        findUnit(txtCont.CurrentStr, foundUnits, numMatch);
                Match keyMatch = findNearestKey(txtCont.CurrentStr, foundKeys, numMatch);
                if (keyMatch == null) //exclude numbers without keys
                    continue;
                int curYear = findYear(numValues, i, foundYears, txtCont.CurrentStr);
                if (curYear == 0)
                    continue;
                string requiredValue = money ? makeCurrency(txtCont.CurrentStr, numMatch, unit, curYear, commonFactor) 
                    : makeReqValue(unit, numMatch);
                if (string.IsNullOrEmpty(requiredValue))
                    continue;
                if (money && numMatch.undefinedCurrency && !looksLikeMoney(txtCont.CurrentStr, keyMatch.Index, numMatch, requiredValue))
                    continue;
                writeValueToTmpList(tmpList, numMatch, years);
            }

            addToResultFromTmpList(txtCont, tmpList, foundKeys, years);
        }

        string ExtractLineInfo(string text, int pos, int pos1, ref int leneN)
        {
            int first = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    if (pos >= first && pos < i)
                    {
                        if (pos1 >= first && pos1 < i)
                            return text.Substring(first, i - first);
                    }
                    if ((pos1 < first || pos1 >= i) && (pos < first || pos >= i))
                        first = i + 1;
                    leneN++;
                }
            }
            return "";
        }

        bool isWrongValue(NumericInText numMatch, string str)
        {
            for (int i = 0; i < numMatch.vList.Count; i++)
            {
                if (isWrong(str, numMatch.posList[i], numMatch.vList[i]))
                    return true;
            }
            return false;
        }

        bool isWrong(string str, int pos, string strVal)
        {
            if (Name == "TOTAL EMPLOYEES" && pos > 0 && str[pos - 1] == '(' && str[pos + strVal.Length] == ')')
                return true;

            if (pos == 0)       // case "18 CO2 Emissions"
                return true;
            string nextWord = null;
            if (string.IsNullOrEmpty(strRegexUnit) || strRegexUnit.IndexOf("%") < 0)
            {
                //unit is not percens
                if (str.Length > pos + strVal.Length && str[pos + strVal.Length] == '%')
                    //"%" is after value without space
                    return true;
                if (str.Length >= pos + strVal.Length && str[pos + strVal.Length - 1] == '%')
                    //"%" is part of value - it depend on regexpr that define a numeric value
                    return true;
                nextWord = getNextWord(str, pos + strVal.Length).ToLower();
                if (nextWord == "%" || nextWord.IndexOf("percent") >= 0)
                    return true;
            }

            List<string> badPreviousWords = new List<string> { "scope", "page", "pages", "chapter", "chapters" };
            // exclude cases chapter24, scope 1
            string previosWord = getPreviosWord(str, pos).ToLower();
            string cleanPreviosWord = previosWord.Replace("(", "");

            if (badPreviousWords.Contains(cleanPreviosWord))
                return true;

            List<string> punctuationMarks = new List<string> { ":", ".", ";" };
            if (previosWord == "" || punctuationMarks.Contains(previosWord.Substring(previosWord.Length - 1)))
            {
                // exclude cases kkk: 1) kgkgk
                if (str.Length > pos + strVal.Length && str[pos + strVal.Length] == ')')
                    return true;
                if (nextWord == null)
                    nextWord = getNextWord(str, pos + strVal.Length).ToLower();
                if (nextWord == ")")
                    return true;
            }

            if (people)
            {
                double val = 0;
                if (!Double.TryParse(strVal, out val) || val <= 1000)
                    return true;
            }
            return false;
        }

        bool isWrongPerson(string str, Match fk, PersonInText p)
        {
            // exclude case "...., CEO of Wills "
            if (fk.Index + fk.Length < p.pos)
            {
                int n1 = fk.Index + fk.Length;
                int n2 = p.pos;
                string s = str.Substring(n1, n2 - n1);
                if (s.Trim().ToLower() == "of")
                    return true;
            }
            return false;
        }

        string findUnit(string str, MatchCollection foundUnits, NumericInText numMatch)
        {
            if (strRegexUnit.IndexOf("%") > -1)
            {
                for (int j = 0; j < numMatch.vList.Count; j++)
                {
                    if (numMatch.vList[j].Substring(numMatch.vList[j].Length - 1, 1) == "%")
                        return "%";
                }
            }

            if (foundUnits.Count == 0)
            {
                if (people)
                    return "";
                numMatch.unit = "No Unit";
                return "No Unit";
            }

            for (int j = 0; j < numMatch.vList.Count; j++)
            {
                int i = 0;
                for (; i < foundUnits.Count; i++)
                {
                    if (foundUnits[i].Index > numMatch.posList[j])
                        break;
                }
                if (i < foundUnits.Count && near(str, numMatch.posList[j] + numMatch.vList[j].Length - 1, foundUnits[i].Index, 2, false))
                {
                    numMatch.unit = foundUnits[i].Value;
                    return foundUnits[i].Value;
                }
            }
            numMatch.unit = people ? "" : "No Unit"; 
            return numMatch.unit;
        }

        static Match findNearestKey(string strText, MatchCollection keysList, NumericInText numMatch)
        {
            int i = 0;
            int numPosition = numMatch.posList[0];
            for (; i < keysList.Count; i++)
            {
                if (keysList[i].Index > numPosition)
                    break;
            }
            // key is before
            // if we didn't find unit for numeric value, we try to find key in 3 previos words, else in 25 previos words:
            int countWordsBefore = (numMatch.unit == "No Unit" || numMatch.undefinedCurrency) ? 3 : 25;
            if (i > 0 && near(strText, keysList[i - 1].Index + keysList[i - 1].Value.Length, numPosition, countWordsBefore, true))
            {
                return keysList[i - 1];
            }
            // key is after
            if (numMatch.isList)
                numPosition = numMatch.posList.Last<int>();
            // if we didn't find unit for numeric value, we try to find key in 3 following words (include value itself), else in 6:
            int countWordsAfter = (numMatch.unit == "No Unit"  || numMatch.undefinedCurrency) ? 3 : 6;
            if (i < keysList.Count && near(strText, numPosition, keysList[i].Index, countWordsAfter, false))
            {
                return keysList[i];
            }
            return null;
        }

        static bool near(string strText, int pos1, int pos2, int cnt, bool inAnySentenceFragment)
        {
            // detect the count of words between pos1 and pos2
            string str = strText.Substring(pos1, pos2 - pos1 + 1);
            Regex r = new Regex("([ \\t{}()\\-! \"?\n])");
            MatchCollection delimeters = r.Matches(str);
            if (inAnySentenceFragment)
                return delimeters.Count <= cnt + 1;
            Regex r1 = new Regex("[:;._,]");
            MatchCollection delimeters1 = r1.Matches(str);
            return delimeters.Count <= cnt + 1 && delimeters1.Count == 0;
        }

        string getPreviosWord(string strText, int pos)
        {
            string[] arr = strText.Substring(0, pos).Split(new char[] { ' ' });
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i].Length > 0)
                    return arr[i];
            }
            return "";
        }

        string getNextWord(string strText, int pos)
        {
            string[] arr = strText.Substring(pos, strText.Length - pos).Split(new char[] { ' ' });
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Length > 0)
                    return arr[i];
            }
            return "";
        }


        string findCurrency(string strText, NumericInText numMatch, string commonCurrency)
        {
            string strResult = "";
            for (int i = 0; i < numMatch.vList.Count; i++)
            {
                strResult = findCurrencyForValue(strText, numMatch.posList[i], numMatch.vList[i]);
                if (!string.IsNullOrEmpty(strResult))
                    break;
            }

            if (string.IsNullOrWhiteSpace(strResult))
            {
                if (!string.IsNullOrEmpty(commonCurrency))
                {
                    strResult = commonCurrency;
                }
                else
                {
                    numMatch.undefinedCurrency = true;
                    strResult = "USD";
                }
            }
            numMatch.unit = strResult;
            return strResult;
        }

        string findCurrencyForValue(string strText, int pos, string strValue)
        {
            string strResult = Currency.currencyBySign(strValue);
            if (!string.IsNullOrWhiteSpace(strResult))
                return strResult;
            int numPosition = pos;
            if (numPosition >= 4)
            {
                string cur = strText.Substring(numPosition - 4, 4); // case USD 12.2
                if (cur.Substring(2, 1) == " ")
                    strResult = Currency.currencyAbbreviation(cur);
            }
            if (string.IsNullOrWhiteSpace(strResult))
            {
                string[] arr = strText.Substring(numPosition).Split(new char[] { ' ' });
                int num = 0;
                foreach (string word in arr)
                {
                    if (num > 2)
                        break;
                    if (num == 0)
                    {
                        num++;
                        continue;
                    }
                    if (word.Trim().Length == 0)
                        continue;
                    strResult = Currency.currencyByWord(word);
                    if (!string.IsNullOrEmpty(strResult))
                        break;
                    num++;
                }
            }
            return strResult;
        }

        int findYear(List<NumericInText> numerics, int ind, List<YearInText> foundYears, string currentStr)
        {
            NumericInText numMatch = numerics[ind];
            int curYear = 0;
            if (numMatch.isList)
            {
                foreach (YearInText y in foundYears)
                {
                    if (y.isList && y.yList.Count == numMatch.yList.Count)
                    {
                        //suppose that it is only one list; but better to fine nearest
                        for (int i = 0; i < y.yList.Count; i++)
                        {
                            numMatch.yList[i] = y.yList[i];
                        }
                        curYear = numMatch.yList[0];
                    }
                }
            }
            else
            {
                curYear = YearInText.CurrentYear;
                if (foundYears != null && foundYears.Count > 0)
                {
                    if (foundYears.Count == 1)
                    {
                        YearInText y = foundYears[0];
                        if (!y.isRange && !y.isList) //year is part of range or list
                            curYear = y.year;
                    }
                    else
                    {
                        int len = numMatch.vList[0].Length;
                        string nextWord = getNextWord(currentStr, numMatch.posList[0] + len);
                        if (nextWord == numMatch.unit)  // case "127.000 tons in 2008, 146.000 tons in 2009"; findNearestYear will check "in" between value and year0
                            len += numMatch.unit.Length + 1;

                        YearInText y = YearInText.findNearestYear(currentStr, foundYears, numMatch.posList[0], len);
                        if (!y.isRange && !y.isList) //year is part of range or list
                        {
                            int d = numMatch.posList[0] > y.pos ? numMatch.posList[0] - y.pos : y.pos - numMatch.posList[0];
                            if (d > 1000 || y.year == 0)
                                curYear = YearInText.CurrentYear;
                            else
                                curYear = y.year;
                        }
                    }
                }
                numMatch.yList[0] = curYear;
            }
            return curYear;
        }

        string makeCurrency(string strText, NumericInText numValue, string unit, int curYear, int commonFactor)
        {
            int cnt = numValue.vList.Count;
            int factorList = findFactor(strText, numValue.vList[cnt - 1], numValue.posList[cnt - 1], numValue.undefinedCurrency); //case 100, 200, 300 and 400 million USD
            if (factorList == 1 && commonFactor > 1)
                factorList = commonFactor;
            for (int i = 0; i < cnt; i++)
            {
                int factor = cnt == 1 || i == cnt - 1 ? factorList : findFactor(strText, numValue.vList[i], numValue.posList[i], numValue.undefinedCurrency);
                if (factor == 1)
                    factor = factorList;
                numValue.vrList[i] = Currency.makeCurrencyValue(numValue.vList[i], unit, factor, numValue.yList[i]);
            }
            return numValue.vrList[0];
        }

        int findFactor(string strText, string sourceStr, int ind, bool undefinedCurrency)
        {
            string nextWord = "";
            if (sourceStr.Substring(sourceStr.Length - 1, 1) == "B") //case $34.0B
            {
                if (undefinedCurrency)
                    return 1;
                nextWord = "B";
            }
            int pos = ind + sourceStr.Length;
            while (pos < strText.Length && strText[pos] == ' ')
                pos++;
            if (pos < strText.Length)
            {
                Regex r = new Regex("([ \\t{}():;._,\\-! \"?\n\\d])");
                MatchCollection delimeters = r.Matches(strText, pos);
                if (delimeters.Count > 0)
                    nextWord = strText.Substring(pos, delimeters[0].Index - pos);
                else
                    nextWord = strText.Substring(pos);
            }
            return Currency.getCurrencyFactor(nextWord);
        }

        string makeReqValue(string unit, NumericInText numValue)
        {
            for (int i = 0; i < numValue.vList.Count; i++)
            {
                string comment = "";
                numValue.vrList[i] = MakeReqValue(unit, numValue.vList[i], ref comment);
                numValue.cmList[i] = comment;
                if (integerValue)
                    numValue.vrList[i] = numValue.vrList[i].Replace(".", "").Replace(",", "");
            }
            return numValue.vrList[0];
        }

        bool looksLikeMoney(string strText, int posKey, NumericInText numValue, string requiredValue)
        {
            int posNumValue = numValue.posList[0];
            if (numValue.isList)
                return true;
            double v = 0;
            if (!double.TryParse(requiredValue, out v))
                return false;
            if (v < 10000)
                return false;
            if (posNumValue > posKey && near(strText, posKey, posNumValue, 2, true))
                return true;
            if (posNumValue < posKey && near(strText, posNumValue, posKey, 2, false))
                return true;
            return false; 
        }

        void addPersonsToResults(TextContainer txtCont, YearInfo[] years, List<PersonYear> persons)
        {
            for (int i = 0; i < years.Length; i++)
            {
                PersonYear matchPerson = new PersonYear { year = 0, keyValue = "", pos = 0, distanceToKey = -1, name = "" };
                foreach (PersonYear p in persons)
                {
                    if (p.year == years[i].value)
                    {
                        if (matchPerson.distanceToKey == -1 || matchPerson.distanceToKey > p.distanceToKey)
                            matchPerson = p;
                    }
                }
                if (matchPerson.year == 0)
                    continue;
                var foundText = new FoundText(txtCont.position(matchPerson.pos, matchPerson.name),
                    matchPerson.keyValue, "", matchPerson.name, matchPerson.name, "", txtCont.CurrentStr);
                foundText.setPriority(0, false, years[i].value.ToString(), Name);
                addResult(years[i], foundText);
            }
        }

        private class MatchedNumValue
        {
            public int pos;
            public string value;
            public string comment;
            public string reqValue;
            public string unit;
        }

        private class MatchedYearNum
        {
            public int yearId;
            public List<MatchedNumValue> vals;
        }

        private void writeValueToTmpList(List<MatchedYearNum> tmpList, NumericInText numValue, YearInfo[] years)
        {
            if (numValue.isList)
            {
                for (int i = 0; i < numValue.vList.Count; i++)
                {
                    int yId = numValue.yList[i] - years[0].value;
                    if (yId >= years.Length || yId < 0)
                        continue;
                    MatchedYearNum it = tmpList.Find(x => x.yearId == yId);
                    if (it == null)
                    {
                        it = new MatchedYearNum { yearId = yId, vals = new List<MatchedNumValue>() };
                        tmpList.Add(it);
                    }
                    it.vals.Add(new MatchedNumValue
                    {
                        pos = numValue.posList[i],
                        value = numValue.vList[i],
                        reqValue = numValue.vrList[i],
                        comment = numValue.cmList[i],
                        unit = numValue.unit
                    });

                }
            }
            else
            {
                int yId = numValue.yList[0] - years[0].value;
                if (yId >= years.Length || yId < 0)
                    return;
                MatchedYearNum it = tmpList.Find(x => x.yearId == yId);
                if (it == null)
                {
                    it = new MatchedYearNum { yearId = yId, vals = new List<MatchedNumValue>() };
                    tmpList.Add(it);
                }
                it.vals.Add(new MatchedNumValue {
                    pos = numValue.posList[0],
                    value = numValue.vList[0],
                    reqValue = numValue.vrList[0],
                    comment = numValue.cmList[0],
                    unit = numValue.unit });
            }
        }

        // remove unnecessary entities in string: word1 word2 word3 value1 word4 value2 word5 key1
        // value1 is not saved to result because there is value2, that is closer to key
        private void addToResultFromTmpList(TextContainer txtCont, List<MatchedYearNum> tmpList, MatchCollection foundKeys, YearInfo[] years)
        {
            foreach (MatchedYearNum it in tmpList)
            {
                bool unitWasFound = false;
                foreach (MatchedNumValue m in it.vals)
                {
                    if (m.unit != "No Unit")
                    {
                        unitWasFound = true;
                        break;
                    }
                }
                foreach (Match k in foundKeys)
                {
                    int dist = 0;
                    MatchedNumValue res = null;
                    foreach (MatchedNumValue m in it.vals)
                    {
                        if (unitWasFound && m.unit == "No Unit")
                            continue;
                        if (res == null)
                        {
                            res = m;
                            dist = m.pos - k.Index;
                            dist = dist > 0 ? dist : -dist;
                        }
                        else
                        {
                            int newDist = m.pos - k.Index;
                            newDist = newDist > 0 ? newDist : -newDist;
                            if (newDist < dist)
                            {
                                dist = newDist;
                                res = m;
                            }
                        }
                    }
                    if (res != null)
                    {
                        var foundText = new FoundText(txtCont.position(res.pos, res.value),
                            k.Value, res.unit, res.value, res.reqValue, res.comment, txtCont.CurrentStr);
                        int priority = 0;
                        if (txtCont.checkContext(keysTextContext, keysDocContext))
                            priority |= FoundText.FKPriorityContext;
                        foundText.setPriority(priority, false, years[it.yearId].value.ToString(), Name);
                        addResult(years[it.yearId], foundText);
                        it.vals.Remove(res);
                    }
                }
            }
        }

        private List<PersonInText> getPersonList(string str)
        {
            List<PersonInText> foundPersons = new List<PersonInText>();
            var tokens = Indicators.tokenParser.GetTokensForText(str);
            string person = "";
            int personPos = 0;
            string company = "";
            bool companyWas = false;
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].tag3)
                {
                    case "B-PER":
                        person = "";
                        if (i > 0 && tokens[i - 1].token == "Ton")
                            person += tokens[i - 1].token + " ";
                        person += tokens[i].token;
                        personPos = tokens[i].tokenStart;
                        for (int i1 = i + 1; i1 < tokens.Count; i1++)
                        {
                            person += " " + tokens[i1].token;
                            if (tokens[i1].tag3 == "E-PER")
                            {
                                i = i1;
                                break;
                            }
                        }
                        if (string.Compare(Indicators.CompanyName, person, true) != 0)
                            foundPersons.Add(new PersonInText { name = person, pos = personPos });
                        break;
                    case "S-PER":
                        person = "";
                        if (i > 0 && tokens[i - 1].token == "Ton")
                            person += tokens[i - 1].token + " ";
                        person += tokens[i].token;
                        personPos = tokens[i].tokenStart;
                        if (string.Compare(Indicators.CompanyName, person, true) < 0)
                            foundPersons.Add(new PersonInText { name = person, pos = personPos });
                        break;
                    case "B-ORG":
                        company = tokens[i].token.ToLower();
                        for (int i1 = i + 1; i1 < tokens.Count; i1++)
                        {
                            company += " " + tokens[i1].token.ToLower();
                            if (tokens[i1].tag3 == "E-ORG")
                            {
                                i = i1;
                                break;
                            }
                        }
                        companyWas |= company.IndexOf(Indicators.CompanyName.ToLower()) != -1;
                        break;
                    case "S-ORG":
                        string x = tokens[i].token.ToLower();
                        if (x != "board")
                        {
                            company = tokens[i].token.ToLower();
                            companyWas |= company.IndexOf(Indicators.CompanyName.ToLower()) != -1;
                        }
                        break;

                }
            }
            if (company != "" && !companyWas)
                foundPersons.Clear();
                   if (foundPersons.Count != 0)
                       return foundPersons;

            return foundPersons;
        }
    }
}
