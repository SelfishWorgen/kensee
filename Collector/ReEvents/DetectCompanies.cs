using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{
    public class ReEventsCompanyName
    {
        public string companyName;
        public int companyId;
        public int countryId;
    }

    public class CompanyDictionary
    {
        public Dictionary<string, CompanyDictionary> keys;
        public int companyId;
        public string companyName;
        public int countryId;
    }

    public class CmpAttr
    {
        public int countryId;
        public string companyName;
    }

    public class DetectCompanies
    {
        CompanyDictionary companyDictionary;
        Dictionary<int, CmpAttr> companyCountryDictionary;
        TokenParser tokenParser;
        string companyKeywords;
        string companies;
        public int predefinedCompanyId;
        public string companyNotInDB;

        // result
        public List<int> companyIds;

        public DetectCompanies()
        {
            companyDictionary = new CompanyDictionary { keys = new Dictionary<string, CompanyDictionary>(), companyId = 0 };
            companyCountryDictionary = new Dictionary<int,CmpAttr>();
            companies = "";
            companyKeywords = "";
            predefinedCompanyId = 0;
            companyNotInDB = "";
        }

        public void clear()
        {
            companyIds = new List<int>();
            companies = "";
            companyKeywords = "";
            predefinedCompanyId = 0;
            companyNotInDB = "";
        }

        public void Init(TokenParser tp, List<ReEventsCompanyName> keywords)
        {
            tokenParser = tp;
            foreach (ReEventsCompanyName cn in keywords)
            {
                addCompanyToDictionary(cn.companyName, cn.companyId, cn.countryId);
                companyCountryDictionary.Add(cn.companyId, new CmpAttr {companyName = cn.companyName, countryId = cn.countryId});
            }
        }

        void addCompanyToDictionary(string companyName, int companyId, int countryId)
        {
            List<Token> tokens = tokenParser.GetTokensForText(companyName);
            int k = tokens.Count;
            CompanyDictionary x = companyDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new CompanyDictionary { keys = new Dictionary<string, CompanyDictionary>(), companyId = 0 };
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.companyName = companyName;
                    x.companyId = companyId;
                    x.countryId = countryId;
                }
            }
        }

        public void extractCompanyFromTokenList(List<Token> tokens, ContentSentence sentence)
        {
            for (int i = 0; i < tokens.Count; )
            {
                string pn = "";
                string str = tokens[i].token.ToLower();
                int s = findInCompanyDictionary(tokens, i, companyDictionary, out pn);
                if (s > 0)
                {
                    if (!companyIds.Contains(s))
                    {
                        addToCompanyKeywords(pn, sentence.number);
                        addToCompanies(pn);
                        companyIds.Add(s);
                    }
                    if (!sentence.companyIds.Contains(s))
                    {
                        sentence.companyIds.Add(s);
                        sentence.companyNames.Add(pn);
                        int cntrId = getCountryId(s);
                        sentence.addLocation(new ResultLocation { cityId = 0, cityName = "", countryId = cntrId, countryName = "", keyword = "", regionId = 0, regionName = "", sent = sentence });
                    }
                    int n = tokenParser.GetTokensForText(pn).Count;
                    n = (n == 0 ? 1 : n);
                    for (int k = 0; k < n; k++)
                    {
                        tokens[i + k].tag1 = "E-ORG";
                    }
                    i += n;
                }
                else
                {
                    i++;
                }
            }
        }

        private int findInCompanyDictionary(List<Token> tokens, int index, CompanyDictionary dict, out string companyName)
        {
            companyName = "";
            string str = tokens[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            CompanyDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                companyName = dict.companyName;
                return dict.companyId;
            }
            if (index == tokens.Count - 1)
            {
                companyName = y.companyName;
                return y.companyId;
            }
            int s = findInCompanyDictionary(tokens, index + 1, y, out companyName);

            if (s != 0)
                return s;
            companyName = y.companyName;
            return y.companyId;
        }

        //public bool foundCompanyNameInAbout(List<Token> tokens)
        //{
        //    string companyName;

        //    if (findInCompanyDictionary(tokens, 1, companyDictionary, out companyName) > 0)
        //        return true;
        //    return false;
        //}

        public void addPredefinedCompany(int companyId)
        {
            if (companyId == 0)
                return;
            if (companyIds.Count == 0 || !companyIds.Contains(companyId))
                companyIds.Add(companyId);
        }

        void addToCompanyKeywords(string keyword, int sentenceNumber)
        {
            if (companyKeywords.Length > 0)
                companyKeywords += "|";
            companyKeywords += "{" + sentenceNumber.ToString() + "} " + keyword;
        }

        void addToCompanies(string nm)
        {
            if (companies.Length > 0)
                companies += "|";
            companies += nm;
        }

        public void writeCompaniesToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        {
            writeLineToResult("company keywords", companyKeywords);
            writeLineToResult("companies - db", companies);
        }

        public int getCountryId(int companyId)
        {
            if (companyCountryDictionary.ContainsKey(companyId))
                return companyCountryDictionary[companyId].countryId;
            return 0;
        }

        public string getCompanyName(int companyId)
        {
            if (companyCountryDictionary.ContainsKey(companyId))
                return companyCountryDictionary[companyId].companyName;
            return "";
        }

        public void extractCompanyNotInDB(List<Token> tokens, ContentSentence sentence)
        {
            if (!String.IsNullOrEmpty(companyNotInDB))
                return;
            int first = -1;
            int last = -1;
            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];
                if (first == -1 && t.tag3.IndexOf("ORG") > 0)
                    first = i;
                if (first >= 0 && t.tag3.IndexOf("ORG") < 0)
                {
                    last = i - 1;
                    break;
                }
            }
            if (first >= 0 && last >= 0)
            {
                int p1 = tokens[first].tokenStart;
                int p2 = tokens[last].end_offset;
                companyNotInDB = sentence.txt.Substring(p1, p2 - p1);
            }
        }
    }
}
