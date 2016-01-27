using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using utl;

namespace ReEvents
{
    public class ContentSentence
    {
        public string txt;
        public bool isIgnored;
        public bool isStartIgnoredText;
        public bool isStartAboutSection;
        public int number;
        public bool hasArticleDate;
        public bool eventWasFound;
        public bool firstSensible;
        public int parentListNumber; //??
        public int listElementsCount;

        //New Staff
        public List<int> eventIds;
        public List<string> eventNames;
        //public List<int> countryIds;
        public List<ResultLocation> locations;

        //public List<string> countryNames;
        public List<int> propertyIds;
        //public List<string> propertyNames;
        public List<int> companyIds;
        public List<string> companyNames;
        public string areaStr;
        public string priceStr;
        public string dateStr;

        static string[] Monthes = { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static string[] ShortMonthes = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }; 

        public ContentSentence(string s, int num, DateTime articleDate)
        {
            s = s.Trim();
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(s);
            txt = System.Text.Encoding.UTF8.GetString(tempBytes);

            s = s.Replace("\"", "").Trim();
            isStartIgnoredText = false;
            isStartAboutSection = false;
            if (s.Length > 8 && s.Trim().ToLower().Substring(0, 5) == "about")
            {
                isStartIgnoredText = true;
                isStartAboutSection = true;
            }

            if (txt.IndexOf("biography", StringComparison.OrdinalIgnoreCase) >= 0)
                isStartIgnoredText = true;
            if (txt.IndexOf("Sponsored by", StringComparison.OrdinalIgnoreCase) == 0)
                isStartIgnoredText = true;
            if (txt.IndexOf("See also:", StringComparison.OrdinalIgnoreCase) == 0)
                isStartIgnoredText = true;
            if (txt.IndexOf("BOOKMARK THIS PAGE", StringComparison.OrdinalIgnoreCase) == 0)
                isStartIgnoredText = true;
            
            isIgnored = false;
            if (txt.ToLower().IndexOf("not for distribution") >= 0)
                isIgnored = true;
            if (txt.ToLower().IndexOf("not for release") >= 0)
                isIgnored = true;
            if (txt.ToLower().IndexOf("not for publication") >= 0)
                isIgnored = true;
            if (txt.ToLower().IndexOf("logo:") == 0)
                isIgnored = true;

            string regForYear = @"\b(19|20)\d\d\b";
            Regex regYear = new Regex(regForYear);
            MatchCollection foundYears = regYear.Matches(txt);
            foreach (Match fe in foundYears)
            {
                if (Int32.Parse(fe.Value) < articleDate.Year)
                {
                    isIgnored = true;
                    break;
                }
            }
            hasArticleDate = false;

            if (foundYears.Count > 0 && !isIgnored)
            {
                int indOfDay = txt.IndexOf(articleDate.Day.ToString());
                if (indOfDay >= 0)
                {
                    int indOfMonth = txt.IndexOf(articleDate.Month.ToString());
                    if (indOfMonth < 0)
                    {
                        indOfMonth = txt.IndexOf(Monthes[articleDate.Month], StringComparison.OrdinalIgnoreCase);
                        if (indOfMonth < 0)
                            indOfMonth = txt.IndexOf(ShortMonthes[articleDate.Month], StringComparison.OrdinalIgnoreCase);
                    }
                    if (indOfMonth >= 0)
                        hasArticleDate = true;
                }
            }
            
            number = num;
            eventWasFound = false;
            firstSensible = false;
            parentListNumber = -1;
            listElementsCount = 0;

            // New staff
            eventIds = new List<int>();
            eventNames = new List<string>();
            //countryIds = new List<int>();
            locations = new List<ResultLocation>();
            //countryNames = new List<string>();
            propertyIds = new List<int>();
            //propertyNames = new List<string>();
            //propertyKeywords = new List<string>();
            companyIds = new List<int>();
            companyNames = new List<string>();
            areaStr = "";
            priceStr = "";
            dateStr = "";
        }

        public bool hasSpecialTitleKeywords()
        {
            string regExprStr = "General Meeting|Announces Results|Board of Directors|Annual Report";
            regExprStr += "|Quarter Ended|Year Ended|rights issue";
            regExprStr += "|((First|Second|Third|Fourth) Quarter)";

            Regex regex = new Regex(regExprStr, RegexOptions.IgnoreCase);
            MatchCollection keywords = regex.Matches(txt);
            return keywords.Count > 0;
        }

        public bool isSensible()
        {
            return isSensibleText(txt);
        }

        public static bool isSensibleText(string text)
        {
            List<Token> tokens = ReEventsParser.tokenParser.GetTokensForText(text);
            if (tokens.Count < 4)
                return false;
            foreach (Token t in tokens)
            {
                if (t.tag1.Length >= 2 && t.tag1.Substring(0, 2) == "VB") // has verb
                    return true;
            }
            return false;
        }

        //public void writeHierarchicalEventToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        //{
        //    foreach (string eventName in eventNames)
        //    {
        //        string res = "{" + number.ToString() + "} ";
        //        res += eventName;

        //        //Property
        //        bool first = true;
        //        if (propertyNames.Count == 0) //should not be
        //        {
        //            res += " | Property type: N/A";
        //        }
        //        else
        //        {
        //            foreach (string propertyName in propertyNames)
        //            {
        //                if (first)
        //                    res += " | Property type: ";
        //                else
        //                    res += ", ";
        //                first = false;
        //                res += propertyName;
        //            }
        //        }
        //        //Location
        //        first = true;
        //        if (countryNames.Count == 0) 
        //        {
        //            res += " | Country: N/A";
        //        }
        //        else
        //        {
        //            foreach (string countryName in countryNames)
        //            {
        //                if (first)
        //                    res += " | Country: ";
        //                else
        //                    res += ", ";
        //                first = false;
        //                res += countryName;
        //            }
        //        }
        //        //Price:
        //        res += " | Price: ";
        //        res += priceStr == "" ? "N/A" : priceStr;
        //        //Area:
        //        res += " | Area: ";
        //        res += areaStr == "" ? "N/A" : areaStr;
        //        //Date:
        //        res += " | Date: ";
        //        res += dateStr == "" ? "N/A" : dateStr;
        //        //Company
        //        first = true;
        //        if (companyNames.Count == 0)
        //        {
        //            res += " | Company: N/A";
        //        }
        //        else
        //        {
        //            foreach (string companyName in companyNames)
        //            {
        //                if (first)
        //                    res += " | Company: ";
        //                else
        //                    res += ", ";
        //                first = false;
        //                res += companyName;
        //            }
        //        }
        //        writeLineToResult("Hierarchical event - db", res);
        //    }
        //}

        public void addLocation(ResultLocation location)
        {
            foreach (ResultLocation loc in locations)
            {
                if (loc.cityId == location.cityId && loc.countryId == location.countryId)
                    return;
                if (loc.cityId == 0 && loc.countryId == location.countryId)
                {
                    loc.cityId = location.cityId;
                    return;
                }
            }
            locations.Add(location);
        }



    }
}
