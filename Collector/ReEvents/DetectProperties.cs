using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{
    public class ReEventsPropertyKeyword
    {
        public string propertyName;
        public int propertyId;
    }

    public class ReEventsPropertyName
    {
        public string propertyName;
        public int propertyId;
    }

    public class PropertyDictionary
    {
        public Dictionary<string, PropertyDictionary> keys;
        public int propertyId;
        public string propertyName;
        public bool checkNonStemmed;
        public bool capsLock;
        public PropertyDictionary()
        {
            keys = new Dictionary<string, PropertyDictionary>();
            propertyId = 0;
            propertyName = "";
            checkNonStemmed = false;
            capsLock = false;
        }
    }

    public class DetectProperties
    {
        PropertyDictionary propertyDictionary;
        PropertyDictionary propertyDictionaryNonStemmed;
        TokenParser tokenParser;
        Dictionary<int, string> propertyTypeNames;

        // result
        public List<int> propertyIds;
        string propertyKeywords;
        string properties;

        public DetectProperties()
        {
            propertyDictionary = new PropertyDictionary();
            propertyDictionaryNonStemmed = new PropertyDictionary();
            propertyTypeNames = new Dictionary<int, string>();
            //result
            propertyIds = new List<int>();
            properties = "";
            propertyKeywords = "";
        }

        public void clear()
        {
            propertyIds = new List<int>();
            properties = "";
            propertyKeywords = "";
        }

        public void Init(TokenParser tp, List<ReEventsPropertyKeyword> keywords, List<ReEventsPropertyName> names)
        {
            tokenParser = tp;
            foreach (ReEventsPropertyKeyword cn in keywords)
            {
                addPropertyToDictionary(cn.propertyName, cn.propertyId);
            }
            foreach (ReEventsPropertyName nm in names)
            {
                propertyTypeNames.Add(nm.propertyId, nm.propertyName);
            }
        }

        void addPropertyToDictionary(string pn, int propertyId)
        {
            string propertyName = pn;
            bool checkNonStemmed = false;
            bool capsLock = false;
            int p = pn.IndexOf("(");
            if (p > 0)
            {
                string str1 = pn.Substring(p);
                if (str1.IndexOf("S") > 0)
                    checkNonStemmed = true;
                if (str1.IndexOf("C") > 0)
                    capsLock = true;
                propertyName = pn.Substring(0, p).Trim();
            }

            List<Token> tokens = tokenParser.GetTokensForText(propertyName);
            int k = tokens.Count;
            PropertyDictionary x = propertyDictionary;

            for (int i = 0; i < k; i++)
            {
                string s1 = tokens[i].token.ToLower();
                string s = Stemmer.Convert(s1.ToLower());
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new PropertyDictionary { keys = new Dictionary<string, PropertyDictionary>(), propertyId = 0 };
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.propertyName = propertyName;
                    x.propertyId = propertyId;
                    x.capsLock = capsLock;
                    x.checkNonStemmed = checkNonStemmed;
                }
            }
        }

        public void extractPropertyFromTokenList(List<Token> tokens, List<Token> stemmedTokens, ContentSentence sentence)
        {
            for (int i = 0; i < stemmedTokens.Count; )
            {
                string pn = "";
                bool checkNonStemmed = false;
                bool capsLock = false;
                int lastIndex = i;
                string str = stemmedTokens[i].token.ToLower();
                int s = findInPropertyDictionary(stemmedTokens, i, propertyDictionary, out checkNonStemmed, out capsLock, out pn, out lastIndex);
                if (s > 0)
                {
                    if (checkNonStemmed)
                    {
                        s = findInPropertyDictionary(tokens, i, propertyDictionary, out checkNonStemmed, out capsLock, out pn, out lastIndex);
                        if (s <= 0)
                        {
                            i++;
                            continue;
                        }
                        if (capsLock && !checkCaseSensitive(tokens, sentence.txt, pn, i, lastIndex, false))
                        {
                            i++;
                            continue;
                        }
                    }
                    else if (capsLock && !checkCaseSensitive(tokens, sentence.txt, pn, i, lastIndex, true))
                    {
                        i++;
                        continue;
                    }
                    int n = tokenParser.GetTokensForText(pn).Count;
                    string propertyName = propertyTypeNames[s];
                    if (!propertyIds.Contains(s))
                    {
                        //if (propertyName != "General")
                        //{
                            addToProperties(propertyName);                  // for result log
                            propertyIds.Add(s);                             // for article properties
                        //}
                        addToPropertyKeywords(pn, sentence.number);     // for result log
                    }
                    if (!sentence.propertyIds.Contains(s))
                    {
                        sentence.propertyIds.Add(s);                    // for hierarchical events
                    }
                    i += (n == 0 ? 1 : n);
                }
                else
                {
                    i++;
                }
            }
        }

        private int findInPropertyDictionary(List<Token> t, int index, PropertyDictionary dict,
           out bool checkNonStemmed, out bool capsLock, out string propertyName, out int lastIndex)
        {
            propertyName = "";
            checkNonStemmed = false;
            lastIndex = 0;
            capsLock = false;
            if (t[index].tag1 == "E-ORG")
                return 0;
            if (t[index].tag3 == "E-ORG" || t[index].tag3 == "I-ORG" || t[index].tag3 == "B-ORG")
                return 0;
            if (t[index].tag3 == "B-PER" || t[index].tag3 == "E-PER")
                return 0;
            if (t[index].tag3 == "B-LOC" || t[index].tag3 == "E-LOC")     //Overland Park
                return 0;
            string str = t[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            PropertyDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                propertyName = dict.propertyName;
                capsLock = dict.capsLock;
                checkNonStemmed = dict.checkNonStemmed;
                lastIndex = index;
                return dict.propertyId;
            }
            if (index == t.Count - 1)
            {
                propertyName = y.propertyName;
                capsLock = y.capsLock;
                checkNonStemmed = y.checkNonStemmed;
                lastIndex = index;
                return y.propertyId;
            }
            int s = findInPropertyDictionary(t, index + 1, y, out checkNonStemmed, out capsLock, out propertyName, out lastIndex);

            if (s != 0)
                return s;
            propertyName = y.propertyName;
            capsLock = y.capsLock;
            checkNonStemmed = y.checkNonStemmed;
            lastIndex = index;
            return y.propertyId;
        }

        void addToPropertyKeywords(string keyword, int sentenceNumber)
        {
            if (propertyKeywords.Length > 0)
                propertyKeywords += "|";
            propertyKeywords += "{" + sentenceNumber.ToString() + "} " + keyword;
        }

        void addToProperties(string nm)
        {
            if (properties.Length > 0)
                properties += "|";
            properties += nm;
        }

        public void writePropertiesToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        {
            writeLineToResult("tags keywords", propertyKeywords);
            writeLineToResult("tags - db", properties);
        }

        public string getPropertyName(int id)
        {
            if (propertyTypeNames.ContainsKey(id))
                return propertyTypeNames[id];
            return "";
        }

        bool checkCaseSensitive(List<Token> t, string txt, string fullName, int ind1, int ind2, bool toStem)
        {
            string fullName1 = fullName;
            bool firstIsUpper = Char.IsUpper(fullName[0]);
            bool allAreUpper = true;
            for (int i = 0; i < fullName.Length; i++)
            {
                if (Char.IsLetter(fullName[i]) && !Char.IsUpper(fullName[i]))
                {
                    allAreUpper = false;
                    break;
                }
            }
            if (toStem && !allAreUpper)
            {
                fullName1 = "";
                List<Token> t1 = ReEventsParser.tokenParser.GetTokensForText(fullName);
                foreach (Token n in t1)
                {
                    fullName1 += Stemmer.Convert(n.token);
                    fullName1 += " ";
                }
                fullName1 = fullName1.Trim();
                if (firstIsUpper) //Stemmer converts all symbols to lower case
                    fullName1 = fullName.Substring(0, 1) + fullName1.Substring(1);
            }

            int pos1 = t[ind1].tokenStart;
            int pos2 = t[ind2].end_offset;
            string str = txt.Substring(pos1, pos2 - pos1);
            return (str == fullName1);
        }


     }
}
