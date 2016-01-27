using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using utl;

namespace ReEvents
{

    public class ReEventKeys
    {
        public Dictionary<string, ReEventKeys> keys;
        public string fullName;
        public bool isVerb;
        public bool checkNonStemmed;
        public bool capsLock;
        public ReEventKeys()
        {
            keys = new Dictionary<string, ReEventKeys>();
            fullName = "";
            isVerb = false;
            checkNonStemmed = false;
            capsLock = false;
        }
    }

    public class ReEventRuleKey
    {
        public string name;
        public bool ruleKeyFound;
        ReEventKeys positiveKeys;
        ReEventKeys negativeKeys;
        ReEventKeys positiveKeysNonStemmed;
        ReEventKeys negativeKeysNonStemmed;
        public string allRuleKeys;
        public bool isCurrency;
        public bool isProperty;

        public ReEventRuleKey(ReEventRuleKey r)
        {
            name = r.name;
            positiveKeys = r.positiveKeys;
            negativeKeys = r.negativeKeys;
            positiveKeysNonStemmed = r.positiveKeysNonStemmed;
            negativeKeysNonStemmed = r.negativeKeysNonStemmed;
            ruleKeyFound = false;
            allRuleKeys = "";
            isCurrency = r.isCurrency;
            isProperty = r.isProperty;
        }

        public ReEventRuleKey(string n)
        {
            name = n;
            positiveKeys = new ReEventKeys();
            negativeKeys = new ReEventKeys();
            positiveKeysNonStemmed = new ReEventKeys();
            negativeKeysNonStemmed = new ReEventKeys();
            ruleKeyFound = false;
            allRuleKeys = "";
            isCurrency = false;
        }

        public void clear()
        {
            ruleKeyFound = false;
            allRuleKeys = "";
        }

        public void addKey(string str)
        {
            bool isNegative = false;
            bool isVerb = false;
            bool checkNonStemmed = false;
            bool capsLock = false;
            if (str[0] == '-')
            {
                isNegative = true;
                str = str.Substring(1);
            }
            int p = str.IndexOf("(");
            if (p > 0)
            {
                string str1 = str.Substring(p);
                if (str1.IndexOf("V") > 0)
                    isVerb = true;
                if (str1.IndexOf("S") > 0)
                    checkNonStemmed = true;
                if (str1.IndexOf("C") > 0)
                    capsLock = true;
                str = str.Substring(0, p).Trim();
            }

            string[] arrs = str.Split(new char[] { ' ' });
            int k = arrs.Length;

            ReEventKeys x = isNegative ? negativeKeys : positiveKeys;
            for (int i = 0; i < k; i++)
            {
                string s1 = arrs[i];
                string s = Stemmer.Convert(s1.ToLower());
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                    if (checkNonStemmed)
                        x.checkNonStemmed = true;
                    if (capsLock)
                        x.capsLock = true;
                }
                else
                {
                    var z = new ReEventKeys { keys = new Dictionary<string, ReEventKeys>() }; 
                    x.keys.Add(s, z);
                    x = z;
                }
                if (i == k - 1)
                {
                    x.fullName = str;
                    x.isVerb = isVerb;
                    if (checkNonStemmed)
                        x.checkNonStemmed = checkNonStemmed;
                    if (capsLock)
                        x.capsLock = capsLock;
                }
            }
            if (checkNonStemmed)
            {
                x = isNegative ? negativeKeysNonStemmed : positiveKeysNonStemmed;
                for (int i = 0; i < k; i++)
                {
                    string s = arrs[i].ToLower();
                    if (x.keys.ContainsKey(s))
                    {
                        x = x.keys[s];
                    }
                    else
                    {
                        var z = new ReEventKeys { keys = new Dictionary<string, ReEventKeys>() };
                        x.keys.Add(s, z);
                        x = z;
                    }
                    if (i == k - 1)
                    {
                        x.fullName = str;
                        x.isVerb = isVerb;
                        x.checkNonStemmed = checkNonStemmed;
                        x.capsLock = capsLock;
                    }
                }
            }
        }

        public bool foundRuleKey(List<Token> tokens, List<Token> stemmedTokens, ContentSentence sentence)
        {
            string fullName = "";
            bool checkNonStemmed = false;
            bool capsLock = false;
            int lastIndex = -1;
            // following text is commented because currently we have
            // "-officer(S)" - it means that we need to ignore this word, not detect it as negative
            //if (negativeKeys.keys.Count > 0)
            //{
            //    for (int i = 0; i < stemmedTokens.Count; i++)
            //    {
            //        if (checkNonStemmed)
            //        {
            //            if (!checkRuleKey(tokens, i, negativeKeysNonStemmed, true, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
            //                continue;
            //            if (capsLock && !checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, false))
            //                continue;
            //        }
            //        else if (capsLock && checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, true))
            //            continue;
            //        addToAllRuleKeys("-" + fullName, sentence.number);
            //        return false;
            //    }
            //}
            if (isCurrency)
            {
                Regex regex = new Regex(@"\$|¥|£|€|₪|₽|₴|₩|₹");
                MatchCollection foundKeys = regex.Matches(sentence.txt);
                if (foundKeys.Count != 0)
                {
                    addToAllRuleKeys(foundKeys[0].Value, sentence.number);
                    ruleKeyFound = true;
                    return true;
                }
            }
            for (int i = 0; i < stemmedTokens.Count; i++)
            {
                // temporary solution about "-officer(S)"
                if (negativeKeys.keys.Count > 0 && checkRuleKey(stemmedTokens, i, negativeKeys, false, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
                {
                    if (checkNonStemmed || capsLock) // is not real negatiove, only to ignore
                    {
                        if (checkNonStemmed)
                        {
                            if (checkRuleKey(tokens, i, negativeKeysNonStemmed, true, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
                                continue;
                            if (capsLock && checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, false))
                                continue;
                        }
                        else if (capsLock && checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, true))
                            continue;
                    }
                    else
                    {
                        addToAllRuleKeys("-" + fullName, sentence.number);
                        return false;
                    }
                }
                // END temporary solution about "-officer(S)"
                if (checkRuleKey(stemmedTokens, i, positiveKeys, false, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
                {
                    if (checkNonStemmed)
                    {
                        if (!checkRuleKey(tokens, i, positiveKeysNonStemmed, true, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
                            continue;
                        if (capsLock && !checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, false))
                            continue;
                    }
                    else if (capsLock && !checkCaseSensitive(tokens, sentence.txt, fullName, i, lastIndex, true))
                        continue;
                    ruleKeyFound = true; 
                    addToAllRuleKeys(fullName, sentence.number);
                    return true;
                }
            }
            return false;
        }

        public bool checkRuleKey(List<Token> t, int index, ReEventKeys x, bool toLower,
            out string fullName, out bool checkNonStemmed, out bool capsLock, out int lastIndex)
        {
            checkNonStemmed = false;
            fullName = "";
            capsLock = false;
            lastIndex = -1;
            string s = t[index].token;
            if (toLower)
                s = s.ToLower();
            if (t[index].tag1 == "E-ORG")
                return false;
            if (t[index].tag3 == "E-ORG" || t[index].tag3 == "I-ORG" || t[index].tag3 == "B-ORG")
                return false;
            if (t[index].tag3 == "B-PER" || t[index].tag3 == "E-PER")
                return false;
            if (t[index].tag3 == "B-LOC" || t[index].tag3 == "E-LOC")
                return false;    
            
            if (!x.keys.ContainsKey(s))
                return false;
            ReEventKeys y = x.keys[s];
            if (y == null || y.keys == null)
            {
                if (x.isVerb && !tokenIsVerb(t[index]))
                    return false;
                fullName = x.fullName;
                checkNonStemmed = x.checkNonStemmed;
                capsLock = x.capsLock;
                lastIndex = index;
                return true;
            }
            if (y.keys.Count == 0)
            {
                if (y.isVerb && !tokenIsVerb(t[index]))
                    return false;
                fullName = y.fullName;
                checkNonStemmed = y.checkNonStemmed;
                capsLock = y.capsLock;
                lastIndex = index;
                return true;
            }
            if (index == t.Count - 1)
            {
                if (y.isVerb && !tokenIsVerb(t[index]))
                    return false;
                if (!string.IsNullOrEmpty(y.fullName))
                {
                    fullName = y.fullName;
                    checkNonStemmed = y.checkNonStemmed;
                    capsLock = y.capsLock;
                    lastIndex = index;
                    return true;
                }
                return false;
            }
            if (checkRuleKey(t, index + 1, y, toLower, out fullName, out checkNonStemmed, out capsLock, out lastIndex))
                return true;
            if (!string.IsNullOrEmpty(y.fullName))
            {
                if (y.isVerb && !tokenIsVerb(t[index]))
                    return false;
                fullName = y.fullName;
                capsLock = y.capsLock;
                checkNonStemmed = y.checkNonStemmed;
                lastIndex = index;
                return true;
            }
            return false;
        }

        void addToAllRuleKeys(string keyName, int sentenceNumber)
        {
            if (string.IsNullOrEmpty(keyName))
                return;
            if (!string.IsNullOrEmpty(allRuleKeys))
                allRuleKeys += "|";
            allRuleKeys += "{" + sentenceNumber.ToString() + "} " + keyName;
        }

        bool tokenIsVerb(Token token)
        {
            if (token.tag1.Length >= 2 && token.tag1.Substring(0, 2) == "VB")
                return true;
            return false;
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
                foreach(Token n in t1)
                {
                    fullName1 += Stemmer.Convert(n.token);
                    fullName1 += " ";
                }
                fullName1 = fullName1.Trim();
                if (firstIsUpper) //Stemmer converts all symbols to lower case
                    fullName1 = fullName.Substring(0,1) + fullName1.Substring(1);
            }

            int pos1 = t[ind1].tokenStart;
            int pos2 = t[ind2].end_offset;
            string str = txt.Substring(pos1, pos2 - pos1);
            return (str == fullName1);
        }
    }
}

//OLD CODE FROM FORM
//void extractHrefAndTitle2(HtmlAgilityPack.HtmlNode node, out string hRef, out string title)
//{
//    hRef = "";
//    title = "";
//    if (node == null)
//        return;
//    foreach (HtmlAgilityPack.HtmlNode child in node.ChildNodes)
//    {
//        if (child.NodeType == HtmlNodeType.Element && child.Name == "a")
//        {
//            if (child.Attributes["href"] != null)
//                hRef = child.Attributes["href"].Value;
//            if (child.Attributes["title"] != null)
//                title = child.Attributes["title"].Value;
//            if (title == "")
//                title = child.InnerText.Trim();
//        }
//    }
//}

//public void GetNews_2()
//{
//    string url = "http://finance.yahoo.com/news";

//    this.Invoke((MethodInvoker)delegate
//    {
//        /*progressBar1.Maximum = cnt;
//        progressBar1.Value = 0;
//        lbl_message.Text = "0 of " + cnt + " articles were fetched.";*/

//        lbl_message.Text = "Connecting...";
//    });

//    HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
//    HtmlAgilityPack.HtmlDocument doc = web.Load(url);

//    string json = doc.DocumentNode.InnerHtml;
//    string[] pieces = json.Split(new string[] { "YMedia.namespace('dali').config = " }, StringSplitOptions.None);
//    json = pieces[1];

//    pieces = json.Split(new string[] { "}()" }, StringSplitOptions.None);
//    json = pieces[0];
//    json = json.Replace(";\n", "");

//    pieces = json.Split(new string[] { "\\\"items\\\":" }, StringSplitOptions.None);
//    json = pieces[1];

//    pieces = json.Split(new string[] { "]" }, StringSplitOptions.None);
//    json = pieces[0] + "]";

//    json = "{\"items\":" + json.Replace("\\", "") + "}";
//    var items_json = new JavaScriptSerializer().Deserialize<dynamic>(json);

//    int cnt = 0;
//    foreach (var item in items_json["items"])
//        cnt++;

//    //POSTING AJAX FIELDS
//    string postData = "_crumb=hv8uOq9I1/t&_mode=json&_txnid=1427830074406&_json=[{\"_action\":\"show\",\"cat\":\"YCT:001000327,001000349,001000166,001000348,001000186,001000192,001000193,001000216,001000225,001000233,001000241,001000243,001000261,$$YMEDIA:000000406\",\"catName\":\"Business News\",\"sb\":0,\"ccode\":\"grandSlam_finance\",\"woeid\":2147678,\"_subAction\":\"more\",\"items\":[";

//    foreach (var item in items_json["items"])
//    {
//        string u_val = item["u"];
//        string i_val = "";
//        try
//        {
//            i_val = item["i"];
//        }
//        catch (Exception ex)
//        {
//        }
//        if (i_val != "")
//            postData += "{\"u\":\"" + u_val + "\",\"i\":\"" + i_val + "\"},";
//    }

//    postData = postData.Substring(0, postData.Length - 1) + "],";
//    postData += "\"listid\":\"\",\"blogtype\":\"\",\"key\":\"1\",\"prid\":\"5ijjkedahldeg\",\"_container\":0,\"_id\":\"u_30345786\",\"_txnid\":1427830074406}]";

//    string url_ = "http://finance.yahoo.com/hjsal?m_mode=multipart&site=finance&region=US&lang=en-US&pagetype=minihome";
//    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url_);
//    req.Method = "POST";
//    string Data = postData;
//    byte[] postBytes = Encoding.UTF8.GetBytes(Data);
//    req.ContentType = "application/x-www-form-urlencoded";
//    req.ContentLength = postBytes.Length;
//    //req.Referer = "http://finance.yahoo.com/news/";

//    //COOKIE : Y=v=1&n=53v1fbqki4snh;
//    req.CookieContainer = new CookieContainer();
//    Cookie ck1 = new Cookie();
//    ck1.Domain = "finance.yahoo.com";
//    ck1.Name = "Y";
//    ck1.Value = "v=1&n=53v1fbqki4snh";
//    //req.CookieContainer.Add(ck);
//    req.CookieContainer.Add(ck1);

//    Stream requestStream = req.GetRequestStream();
//    requestStream.Write(postBytes, 0, postBytes.Length);
//    requestStream.Close();

//    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
//    Stream resStream = response.GetResponseStream();

//    var sr = new StreamReader(response.GetResponseStream());
//    string responseText = sr.ReadToEnd();

//    var json_obj = new JavaScriptSerializer().Deserialize<dynamic>(responseText);
//    string html = json_obj[0]["html"];

//    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
//    doc2.LoadHtml(html);

//    HtmlAgilityPack.HtmlNodeCollection lis = doc2.DocumentNode.SelectNodes("//li[contains(@class,'content ')]");
//    if (lis != null)
//    {
//        this.Invoke((MethodInvoker)delegate
//        {
//            progressBar1.Maximum = lis.Count;
//            progressBar1.Value = 0;
//            lbl_message.Text = "0 of " + lis.Count + " articles were fetched.";
//        });

//        foreach (HtmlAgilityPack.HtmlNode li in lis)
//        {
//            HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
//            doc3.LoadHtml(li.InnerHtml);

//            HtmlAgilityPack.HtmlNode aNode = doc3.DocumentNode.SelectSingleNode("//a");
//            if (aNode != null)
//            {
//                string href = aNode.Attributes["href"].Value;

//                if (!href.Contains("http://"))
//                {
//                    if (!href.Contains("https://"))
//                        href = "http://finance.yahoo.com" + href;
//                }
//                string title = aNode.InnerText.Trim();

//                article obj = new article();
//                obj.dt = "N/A";
//                obj.content = "";
//                obj.href = href;
//                obj.title = title;
//                obj.type = 2;

//                articleData.Add(obj);

//                this.Invoke((MethodInvoker)delegate
//                {
//                    progressBar1.Value = articleData.Count;
//                    lbl_message.Text = articleData.Count + " of " + lis.Count + " articles were fetched.";
//                });
//            }
//        }
//    }

//    storeMySQL();
//}
