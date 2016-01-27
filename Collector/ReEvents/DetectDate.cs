using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ReEvents
{
    //the quarter ended March 31, 2015
    //first quarter 2015
    //1st quarter 
    //the third quarter of 2015
    //the second quarter
    //second-quarter 2015
    //second quarter
    //prior year quarter
    //Second Quarter 2015
    //past quarter
    //2015 third quarter
    //2nd Quarter
    //second quarter ended March 31, 2015
    //quarter end
    //second quarter this year
    public class DetectDate
    {
        List<string> quarterNumbers = new List<string>() { "first", "second", "third", "fourth", "1st", "2nd", "3rd", "4th", "past", "next", "last", "prior" };
        static string monthes = "january|february|march|april|may|june|july|august|september|october|november|december|" +
                        "jan.?|feb.?|mar.?|apr.?|may.?|jun.?|jul.?|aug.?|sep.?|oct.?|nov.?|dec.?";
        List<string> lastNext = new List<string>() { "last", "next", "current", "previous", "this" };

        public void extractDateFromTokenList(List<Token> tokens, ContentSentence sentence)
        {
            if (findQuarter(tokens, sentence))
                return;
            int startDate = -1;
            int endDate = -1;
            if (findDate(sentence.txt, out startDate, out endDate))
            {
                sentence.dateStr = sentence.txt.Substring(startDate, endDate - startDate);
            }
        }

        public bool findQuarter(List<Token> tokens, ContentSentence sentence)
        {
            // try to find quarter
            for (int i = 0; i < tokens.Count; i++)
            {
                int start = -1;
                int end = -1;

                Token t = tokens[i];
                int ts = t.tokenStart;
                int te = t.end_offset;
                //int cnt = 0;

                if (t.token.ToLower() == "quarter")
                {
                    start = ts;
                    end = te;
                    if (checkTokenPatterns(tokens, i - 1, quarterNumbers))
                    {
                        //1st quarter, second quarter
                        start = tokens[i - 1].tokenStart;
                        if (isYear(tokens, i - 2))
                            // 2015 first quarter
                            start = tokens[i - 1].tokenStart;
                    }
                    else if (checkTokenPattern(tokens, i - 1, "year") && checkTokenPatterns(tokens, i - 1, quarterNumbers))
                    {
                        //prior year quarter
                        start = tokens[i - 2].tokenStart;
                    }

                    if (checkTokenPattern(tokens, i + 1, "of"))
                    {
                        //first quarter of ...
                        if (checkTokenPattern(tokens, i + 2, "year"))
                            //first quarter of year
                            end = tokens[i + 2].end_offset;
                        else if (checkTokenPatterns(tokens, i + 2, lastNext) && checkTokenPattern(tokens, i + 3, "year"))
                            //first quarter of next year
                            end = tokens[i + 3].end_offset;
                        else if (isYear(tokens, i + 2))
                            //first quarter of 2015
                            end = tokens[i + 2].end_offset;
                    }
                    else if (isYear(tokens, i + 1))
                    {
                        //first quarter of 2015
                        end = tokens[i + 1].end_offset;
                    }
                    else if (checkTokenPatterns(tokens, i + 1, lastNext) && checkTokenPattern(tokens, i + 2, "year"))
                        //first quarter next year
                        end = tokens[i + 2].end_offset;
                    else if (checkTokenPattern(tokens, i + 1, "ended") && i + 2 < tokens.Count)
                    {
                        // quarter ended March 31, 2015
                        string s = sentence.txt.Substring(tokens[i + 2].tokenStart);
                        int startDate = -1;
                        int endDate = -1;
                        if (findDate(s, out startDate, out endDate) && startDate == 0)
                        {
                            end = tokens[i + 2].tokenStart + endDate;
                        }
                    }
                    if (start == ts && end == te)
                    {
                        start = -1;
                        end = -1;
                    }
                    if (start > -1 && end > -1)
                    {
                        sentence.dateStr = sentence.txt.Substring(start, end - start);
                        return true;
                    }
                } // if quarter ...
            } // for..
            return false;
        }

        bool checkTokenPatterns(List<Token> tokens, int index, List<string> patterns)
        {
            if (index < 0 || index >= tokens.Count)
                return false;
            string s = tokens[index].token.ToLower();
            if (patterns.Contains(s))
                return true;
            return false;
        }

        bool checkTokenPattern(List<Token> tokens, int index, string pattern)
        {
            if (index < 0 || index >= tokens.Count)
                return false;
            string s = tokens[index].token.ToLower();
            if (s == pattern)
                return true;
            return false;
        }

        bool isYear(List<Token> tokens, int index)
        {
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].tag1 != "CD")
                return false;
            if (tokens[index].end_offset - tokens[index].tokenStart != 4)
                return false;
            int res = 0;
            if (!Int32.TryParse(tokens[index].token, out res))
                return false;
            if (res > 2050 || res < DateTime.Now.Year)
                return false;
            return true;
        }

        bool findDate(string s, out int startDate, out int endDate)
        {
            startDate = -1;
            endDate = -1;
            // format March 31, 2015
            string regexpr1 = @"\b(?:" + monthes + @")\s+(?:[1-9]|0[1-9]|[1-2]\d|3[01]),\s*(?:(?:19|20)\d\d)\b";
            Regex regExpr = new Regex(regexpr1, RegexOptions.IgnoreCase);
            MatchCollection foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format MM/DD/YYYY, MM.DD.YYYY
            string regexpr2 = @"\b(?:[1-9]|0[1-9]|1[0-2])[\.\/](?:[1-9]|0[1-9]|[1-2]\d|3[01])[\.\/](?:[1-2]\d{3})\b";
            regExpr = new Regex(regexpr2);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format DD/MM/YYYY, DD.MM.YYYY
            string regexpr3 = @"\b(?:[1-9]|0[1-9]|[1-2]\d|3[01])[\.\/](?:[1-9]|0[1-9]|1[0-2])[\.\/](?:[1-2]\d{3})\b";
            regExpr = new Regex(regexpr3);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format March 31
            string regexpr4 = @"\b(?:" + monthes + @")\s+(?:[1-9]|0[1-9]|[1-2]\d|3[01])\b";
            regExpr = new Regex(regexpr4, RegexOptions.IgnoreCase);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format March, 2015
            string regexpr5 = @"\b(?:" + monthes + @"),\s+(?:(?:19|20)\d\d)\b";
            regExpr = new Regex(regexpr5, RegexOptions.IgnoreCase);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format March of this year
            string regexpr6 = @"\b(?:" + monthes + ") of this year\b";
            regExpr = new Regex(regexpr5, RegexOptions.IgnoreCase);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            // format 2015
            string regexpr7 = @"\b(19|20)\d\d\b";
            regExpr = new Regex(regexpr7);
            foundDate = regExpr.Matches(s);
            if (foundDate.Count > 0)
            {
                startDate = foundDate[0].Index;
                endDate = foundDate[0].Index + foundDate[0].Length;
                return true;
            }
            return false;
        }

        public static int findYearInDateString(string dateStr)
        {
            DateTime dateResult;
            int year = 0;
            if (DateTime.TryParse(dateStr, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult))
            {
                return dateResult.Year;
            }
            string regexpr7 = @"\b(20)\d\d\b";
            Regex regExpr = new Regex(regexpr7);
            MatchCollection foundDate = regExpr.Matches(dateStr);
            if (foundDate.Count > 0)
            {
                string dt = foundDate[0].Value;
                year = Int32.Parse(dt);
            }
            return year;
        }

        public static int findMonthInDateString(string dateStr)
        {
            DateTime dateResult;
            if (DateTime.TryParse(dateStr, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out dateResult))
            {
                return dateResult.Month;
            }
            string regexpr1 = monthes;
            Regex regExpr = new Regex(regexpr1, RegexOptions.IgnoreCase);
            MatchCollection foundDate = regExpr.Matches(dateStr);
            if (foundDate.Count > 0)
            {
                string dt = foundDate[0].Value.ToLower();
                dt = dt.Substring(0, 3);
                switch (dt)
                {
                    case "jan":
                        return 1;
                    case "feb":
                        return 2;
                    case "mar":
                        return 3;
                    case "apr":
                        return 4;
                    case "may":
                        return 5;
                    case "jun":
                        return 6;
                    case "jul":
                        return 7;
                    case "aug":
                        return 8;
                    case "sep":
                        return 9;
                    case "oct":
                        return 10;
                    case "nov":
                        return 11;
                    case "dec":
                        return 12;
                    default:
                        return 0;
                }
            }
            return 0;
        }
    }

}
