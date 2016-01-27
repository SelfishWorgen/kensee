using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HtmlAgilityPack;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;
using ReEvents;
using ExcelReader;
using System.Diagnostics;

namespace Collector
{
    public class WebSiteInfo
    {
        public WebSiteInfo(string u, int n, string nm, string r, string pr, int ia, int ci, int pi, int bs) 
        {
            url = u; name = nm; id = n; rule = r; pagingRule = pr; isActive = ia; countryId = ci; period = pi; buildSentiments = bs;
        }
        public string url;
        public string name;
        public int id;
        public string rule;
        public string pagingRule;
        public int isActive;
        public int buildSentiments;
        public string[] baseUrl;
        public string[] filterUrl;
        public int countryId;
        public int period;
    }

    public partial class Form1 : Form
    {
        List<WebSiteInfo> webSiteInfo = null;
        Dictionary<string, bool> url_dict;

        void readNewsWebsite()
        {
            DBConnect db_obj = new DBConnect("kensee");
            webSiteInfo = db_obj.readNewsWebsite();
            url_dict = new Dictionary<string, bool>();
        }

        void scrapeWebSites()
        {
            int stepNumber = 20;
            foreach (var st in webSiteInfo)
            {
                stepNumber++;
                if (!beforeStep(stepNumber, 1, st.id, st.period))
                    continue;
                if (st.isActive == 1)
                {
                    if (st.rule.StartsWith("POST "))
                    {
                        string url = st.rule.Substring(5);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                        req.Method = "POST";

                        Stream requestStream = req.GetRequestStream();
                        requestStream.Close();

                        HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                        Stream resStream = response.GetResponseStream();

                        var sr = new StreamReader(response.GetResponseStream());
                        string responseText = sr.ReadToEnd();

                        var result_json = new JavaScriptSerializer().Deserialize<dynamic>(responseText);

                        foreach (var item in result_json["results"])
                        {
                            string dt = item["date"];
                            DateTime enteredDate = dateFromString(dt);
                            if (enteredDate == DateTime.MinValue)
                                continue;

                            string title = item["title"];
                            string link = item["link"];

                            if (title != "" && link != "")
                            {
                                article obj = new article();
                                obj.dt = enteredDate;
                                obj.title = title;
                                obj.href = link;
                                obj.content = "";
                                obj.sourceId = st.id;
                                obj.sourceTypeId = 1;
                                obj.companyname = "Choice Properties REIT";
                                articleData.Add(obj);
                            }
                        }
                        continue;
                    }
                    if (string.IsNullOrEmpty(st.url))
                    {
                        scrapSpecialFiles(st);
                    }
                    else
                    {
                        string[] attr = st.rule.Split(new char[] { ';' });
                        int pagesCount = string.IsNullOrWhiteSpace(st.pagingRule) ? 1 : 525;
                        HtmlNodeCollection findclasses0 = null;
                        if (attr.Length > 7)
                        {
                            string html0 = loadHTML(st.url);
                            if (html0 == null)
                                break;
                            HtmlAgilityPack.HtmlDocument doc0 = new HtmlAgilityPack.HtmlDocument();
                            doc0.LoadHtml(html0);
                            findclasses0 = doc0.DocumentNode.SelectNodes(attr[7]);
                            if (findclasses0 == null || findclasses0.Count == 0)
                                break;
                            pagesCount = findclasses0.Count;
                        }
                        if (pagesCount != 1)
                            this.Invoke((MethodInvoker)delegate
                            {
                                progressBar1.Maximum = pagesCount;
                                progressBar1.Value = 0;
                                lbl_message2.Text = "Scraping...";
                            }); 
                        bool toBreak = false;
                        string nextUrl = "";
                        for (int i = 1; i <= pagesCount; i++)
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            string url = "";
                            if (attr.Length > 7)
                            {
                                var node = findclasses0[i - 1];
                                string title = "";
                                DateTime dt = DateTime.Now;
                                extractHrefTitleDate(findclasses0[i - 1], ref url, ref title, ref dt);
                                if (string.IsNullOrWhiteSpace(url))
                                    continue;
                            }
                            else
                            {
                                if (nextUrl != "")
                                    url = nextUrl;
                                else
                                {
                                    url = st.url;
                                    if (pagesCount != 1 && i != 1)
                                    {
                                        if (st.pagingRule.StartsWith("http"))
                                            url = string.Format(st.pagingRule, i);
                                        else
                                            url = url + string.Format(st.pagingRule, i);
                                    }
                                }
                            }
                            string html = loadHTML(url);
                            if (html == null)
                            {
                                try
                                {
                                    var www = new HtmlWeb();
                                    doc = www.Load(url);
                                }
                                catch (Exception ex)
                                {
                                    System.Console.WriteLine("exception " + ex.Message);
                                    toBreak = true;
                                }
                            }
                            else
                            {
                                doc.LoadHtml(html);
                            }
                            HtmlNodeCollection findclasses;
                            var baseRef = "";
                            var articleDivs = attr[0].Split(new char[] { ',' });
                            for (int iad = 0; iad < articleDivs.Length; iad++)
                            {
                                var articleDiv = articleDivs[iad];
                                bool useParent = false;
                                bool use2 = false;
                                if (articleDiv[0] == 'p')
                                {
                                    useParent = true;
                                    articleDiv = articleDiv.Substring(1);
                                }
                                else if (articleDiv[0] == '2')
                                {
                                    use2 = true;
                                    articleDiv = articleDiv.Substring(1);
                                }
                                if ((findclasses = doc.DocumentNode.SelectNodes(articleDiv)) != null)
                                {
                                    var uri = new Uri(st.url);
                                    baseRef = uri.GetLeftPart(UriPartial.Authority);
                                    if (pagesCount == 1)
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            progressBar1.Maximum = findclasses.Count;
                                            progressBar1.Value = 0;
                                            lbl_message2.Text = "Scraping...";
                                        });
                                    for (int j = 0; j < findclasses.Count; j++)
                                    {
                                        var x1 = findclasses[j];
                                        var x = x1;
                                        if (useParent)
                                            x = x1.ParentNode;
                                        if (use2)
                                            j++;
                                        toBreak = scrapeWebSite(x, st, st.id, baseRef, use2 ? findclasses[j] : null, attr) && i > 1;
                                        if (attr.Length > 7)
                                            toBreak = false; // RSS
                                        if (pagesCount == 1)
                                            this.Invoke((MethodInvoker)delegate
                                            {
                                                progressBar1.Value++;
                                            });
                                        if (toBreak)
                                            break;
                                    }
                                    if (toBreak)
                                        break;
                                }
                            }
                            if (toBreak)
                                break;
                            if (pagesCount != 1)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    progressBar1.Value++;
                                });
                                if (st.pagingRule == "#1")
                                {
                                    if (i == 1)
                                    {
                                        var y = doc.DocumentNode.SelectNodes("//div");
                                        if (y == null)
                                            break;

                                        foreach (var y1 in y)
                                        {
                                            if (y1.InnerText == "&laquo;&nbsp;Previous&nbsp;Issue")
                                            {
                                                if (y1.ChildNodes != null && y1.ChildNodes.Count == 1)
                                                {
                                                    var y2 = y1.ChildNodes[0];
                                                    if (y2 == null || y2.Attributes.Count == 0 || y2.Attributes["href"] == null)
                                                        continue;
                                                    nextUrl = y2.Attributes["href"].Value;
                                                    if (nextUrl.StartsWith("/"))
                                                        nextUrl = baseRef + nextUrl;
                                                }
                                                break;
                                            }
                                        }
                                        if (string.IsNullOrWhiteSpace(nextUrl))
                                            break;
                                    }
                                    else
                                    {
                                        int n = nextUrl.LastIndexOf("_");
                                        if (n == -1)
                                            break;
                                        string str1 = nextUrl.Substring(n + 1);
                                        int n2 = 0;
                                        if (!int.TryParse(str1, out n2) || n2 <= 1)
                                            break;
                                        nextUrl = nextUrl.Substring(0, n + 1) + (n2 - 1).ToString();
                                        if (i == 2)
                                            pagesCount = n2;
                                    }
                                }
                                else if (st.pagingRule.StartsWith("//div"))
                                {
                                    var y = doc.DocumentNode.SelectSingleNode(st.pagingRule);
                                    if (y == null)
                                        break;
                                    HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                                    doc1.LoadHtml(y.InnerHtml);
                                    var y1 = doc1.DocumentNode.SelectSingleNode("//a");
                                    if (y1 == null || y1.Attributes.Count == 0 || y1.Attributes["href"] == null)
                                        break;
                                    nextUrl = y1.Attributes["href"].Value;
                                    if (nextUrl.StartsWith("/"))
                                        nextUrl = baseRef + nextUrl;
                                }
                            }
                        }
                    }
                    if (articleData.Count > 0)
                    {
                        storeMySQL();
                        DBConnect db_obj = new DBConnect("kensee");
                        db_obj.endScraping(st.id);
                    }
                }
                afterStep(stepNumber);
            }
        }

        // scraping rule:
        // 1 - articles
        // 2 - short content (starting * - must be)
        // 3 - title
        // 4 - date
        // 5 - href
        // 6 - content of artice:
        //     #c<x>-<y>   // between x and y
        bool scrapeWebSite(HtmlAgilityPack.HtmlNode node1, WebSiteInfo st, int sourceId, string baseRef, HtmlAgilityPack.HtmlNode node2, string[] attr)
        {
            HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
            doc1.LoadHtml(node1.InnerHtml);
            if (node2 != null)
                doc2.LoadHtml(node2.InnerHtml);
            DateTime enteredDate = DateTime.MinValue;
            string href = "";
            string title = "";
            string shortContent = "";
            if (attr.Length > 1 && attr[1] != "") // short content
            {
                bool mustBe = false;
                bool last = false;
                string str = attr[1].Trim();
                if (str == "#1")
                {
                    str = doc1.DocumentNode.InnerHtml;
                    int n = str.IndexOf("</span>");
                    if (n != -1)
                    {
                        str = str.Substring(n + 7);
                        n = str.IndexOf("</span>");
                        if (n != -1)
                        {
                            str = str.Substring(n + 7);
                            if (str.StartsWith(" - "))
                                str = str.Substring(3);
                            n = str.IndexOf("<span");
                            if (n != -1)
                            {
                                shortContent = str.Substring(0, n);
                            }
                        }
                    }
                }
                else
                {
                    if (str.StartsWith("*"))
                    {
                        mustBe = true;
                        str = str.Substring(1);
                    }
                    if (str.StartsWith("l"))
                    {
                        last = true;
                        str = str.Substring(1);
                    }
                    string[] attr1 = str.Split(new char[] { ',' });
                    var y = node2 == null ? doc1.DocumentNode.SelectNodes(attr1[0]) : doc2.DocumentNode.SelectNodes(attr1[0]); ;
                    if (y != null)
                    {
                        var yy = y[0];
                        if (last)
                            yy = y[y.Count - 1];
                        if (attr1.Length > 1 && yy.Attributes.Count > 0)
                            shortContent = yy.Attributes[attr1[1]].Value;
                        if (string.IsNullOrEmpty(shortContent))
                        {
                            if (string.IsNullOrWhiteSpace(yy.InnerText))
                                yy = yy.NextSibling;
                            if (yy != null)
                                shortContent = yy.InnerText;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(shortContent))
                {
                    shortContent = shortContent.Replace("�", " ");
                    shortContent = shortContent.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                    shortContent = WebUtility.HtmlDecode(shortContent).Trim();
                }
                if (string.IsNullOrWhiteSpace(shortContent) && mustBe)
                    return false;
            }
            if (attr.Length > 2 && attr[2] != "") // title
            {
                string[] attr1 = attr[2].Split(new char[] { ',' });
                var y = doc1.DocumentNode.SelectSingleNode(attr1[0]);
                if (y != null)
                {
                    if (attr1.Length == 2)
                    {
                        title = y.Attributes[attr1[1]].Value;
                    }
                    else
                    {
                        title = y.InnerText;
                        if (y.Attributes["href"] != null)
                            href = y.Attributes["href"].Value;
                    }
                    title = title.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                    title = WebUtility.HtmlDecode(title).Trim();
                }
            }
            if (attr.Length > 3 && attr[3] != "") // date
            {
                string[] attr1 = attr[3].Split(new char[] { ',' });
                var yy = node2 == null ? doc1.DocumentNode.SelectNodes(attr1[0]) : doc2.DocumentNode.SelectNodes(attr1[0]); ;
                if (yy != null)
                {
                    foreach (var y in yy)
                    {
                        if (y.Attributes["data-datetime-monthshort"] != null && y.Attributes["data-datetime-day"] != null && y.Attributes["data-datetime-year"] != null)
                        {
                            string str = y.Attributes["data-datetime-monthshort"].Value + " " + y.Attributes["data-datetime-day"].Value + " " + y.Attributes["data-datetime-year"].Value;
                            enteredDate = extractDateTime1(str);
                        }
                        else if (attr1.Length == 2)
                        {
                            string str = y.Attributes[attr1[1]].Value;
                            enteredDate = extractDateTime1(str);
                        }
                        else
                            enteredDate = extractDateTime1(y);
                        if (enteredDate != DateTime.MinValue)
                            break;
                    }
                }
            }
            if (attr.Length > 4 && attr[4] != "") // href
            {
                string[] attr1 = attr[4].Split(new char[] { ',' });
                var y = doc1.DocumentNode.SelectSingleNode(attr1[0]);
                if (y != null)
                {
                    if (attr1.Length == 2)
                        href = y.Attributes[attr1[1]].Value;
                    else if (!string.IsNullOrWhiteSpace(y.InnerText))
                        href = y.InnerText;
                    else
                    {
                        int n1 = node1.InnerHtml.IndexOf("<link>");
                        if (n1 != -1)
                        {
                            int n2 = node1.InnerHtml.IndexOf(" \n ", n1 + 6);
                            if (n2 == -1)
                                n2 = node1.InnerHtml.IndexOf("<", n1 + 6);
                            if (n2 != -1)
                            {
                                href = node1.InnerHtml.Substring(n1 + 6, n2 - n1 - 6).Trim();
                            }
                        }
                    }
                }
            }
            if (href == "" && title != "")
            {
                if (node1.Attributes != null && node1.Attributes["href"] != null)
                    href = node1.Attributes["href"].Value;
            }
            if (href == "" || title == "")
                extractHrefTitleDate(doc1.DocumentNode, ref href, ref title, ref enteredDate);
            if (href == "")
            {
                if (node1.Attributes["href"] != null)
                {
                    href = node1.Attributes["href"].Value;
                    if (title == "")
                    {
                        title = node1.InnerText.Trim();
                        title = WebUtility.HtmlDecode(title);
                    }
                }
            }
            if (href == "" || title == "" || title.EndsWith(" news in brief"))
                return false;
            if (enteredDate != DateTime.MinValue && enteredDate < start_date)
                return true;
            int nn = title.IndexOf("<!--");
            if (nn != -1)
            {
                title = title.Substring(0, nn);
                title = title.Trim();
            }
            title = title.Replace("�", " ");
            if (title.StartsWith("<![CDATA[") && title.EndsWith("]]>"))
            {
                title = title.Substring(9, title.Length - 9 - 3);
            }
            if (title.Length < 20 && (title.EndsWith("video") || title.EndsWith("videos") || title.EndsWith("comments") || title.EndsWith("comment")))
                return false;
            if (href.StartsWith("/"))
                href = baseRef + href;
            else if (href.StartsWith("news/"))
                href = baseRef + "/" + href;
            if (url_dict.ContainsKey(href))
                return false;
            url_dict.Add(href, false);
            bool toAdd = false;
            foreach (var x in st.baseUrl)
            {
                if (href.StartsWith(x))
                {
                    toAdd = true;
                    break;
                }
            }
            if (!toAdd)
                return false;
            if (st.filterUrl != null)
            {
                foreach (var x in st.filterUrl)
                {
                    if (href.StartsWith(x))
                    {
                        return false;
                    }
                }
            }
            return addArticle(href, enteredDate, title, sourceId, st.name, shortContent, st.countryId, attr);
        }

        bool addArticle(string href, DateTime enteredDate, string title, int sourceId, string from, string shortContent, int countryId, string[] attr)
        {
            if (href.Length >= 512)
                return false;
            HtmlAgilityPack.HtmlWeb www = null;
            HtmlAgilityPack.HtmlDocument dcc = null;
            if (attr.Length > 5 && attr[5].StartsWith("#h"))
            {
                www = new HtmlWeb();
                dcc = www.Load(href);
                string[] attr5 = attr[5].Substring(2).Split(new char[] { ',' });
                if (attr5.Length != 2)
                    return false;
                var y = dcc.DocumentNode.SelectSingleNode(attr5[0]);
                if (y == null)
                    return false;
                string commomTitle = "";
                var head = dcc.DocumentNode.SelectSingleNode("//div[@class='pageTitle']");
                if (head != null)
                    commomTitle = WebUtility.HtmlDecode(head.InnerText.Trim().Replace("\n", "").Replace("\r", "").Replace("\t", ""));
                title = WebUtility.HtmlDecode(title).Trim();
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(y.InnerHtml);
                var y1 = doc.DocumentNode.SelectNodes(attr5[1]);
                if (y1 == null)
                    return false;
                foreach (var y2 in y1)
                {
                    if (y2.Attributes == null || y2.Attributes.Count == 0 || y2.Attributes[0].Name != "href")
                        continue;
                    href = y2.Attributes[0].Value;
                    if (enteredDate == DateTime.MinValue)
                    {
                        title = y2.InnerText.Trim();
                        title = WebUtility.HtmlDecode(title);

                        Regex regex = new Regex(@"/20\d\d/\d\d/", RegexOptions.IgnoreCase);
                        MatchCollection foundDate = regex.Matches(href);
                        if (foundDate.Count == 1)
                        {
                            int n = foundDate[0].Index;
                            int year = 0;
                            int m = 0;
                            if (int.TryParse(href.Substring(n + 6, 2), out m) && int.TryParse(href.Substring(n + 1, 4), out year))
                                enteredDate = new DateTime(year, m, 1);
                        }
                        if (enteredDate == DateTime.MinValue)
                        {
                            int n = title.IndexOf("_Q");
                            if (n != -1)
                            {
                                int year = 0;
                                int q = 0;
                                if (!int.TryParse(title.Substring(n + 2, 1), out q) || !int.TryParse(title.Substring(n + 3, 4), out year))
                                {
                                    title = "";
                                    continue;
                                }
                                enteredDate = new DateTime(year, q * 3, 30);
                            }
                            else
                            {
                                title = "";
                                continue;
                            }
                        }
                    }
                    if (enteredDate > DateTime.Now)
                        return false;
                    article obj1 = new article();
                    if (commomTitle != "" && title.IndexOf(' ') == -1)
                        obj1.title = commomTitle + " - " + title;
                    else
                        obj1.title = title;
                    obj1.href = href.Trim();
                    obj1.content = "";
                    obj1.title = obj1.title.Replace("\n", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                    obj1.dt = enteredDate;
                    obj1.sourceId = sourceId;
                    obj1.sourceTypeId = 1;
                    obj1.companyname = from;
                    obj1.content_short = shortContent;
                    if (updateWithReEventParser(obj1, countryId, 0))
                    {
                        if (!string.IsNullOrWhiteSpace(obj1.content_short))
                            articleData.Add(obj1);
                    }
                    enteredDate = DateTime.MinValue;
                }
                return false;
            }
            if (enteredDate == DateTime.MinValue)
            {
                try
                {
                    www = new HtmlWeb();
                    dcc = www.Load(href);
                    if (attr.Length > 6 && !string.IsNullOrWhiteSpace(attr[6]))
                    {
                        string[] attr6 = attr[6].Split(new char[] { ',' });
                        foreach (var dd in attr6)
                        {
                            var ndd = dcc.DocumentNode.SelectSingleNode(dd);
                            if (ndd != null)
                            {
                                enteredDate = extractDateTime1(ndd);
                                if (enteredDate != DateTime.MinValue)
                                    break;
                            }
                        }
                    }
                    if (enteredDate == DateTime.MinValue)
                    {
                        var ndd = dcc.DocumentNode.SelectSingleNode("//div[@class='cmTimeStamp']");
                        if (ndd != null)
                            enteredDate = extractDateTime1(ndd);
                    }
                    if (enteredDate == DateTime.MinValue)
                    {
                        var ndd = dcc.DocumentNode.SelectSingleNode("//div[@class='byline']");
                        if (ndd != null)
                            enteredDate = extractDateTime1(ndd);
                    }
                    if (enteredDate == DateTime.MinValue)
                    {
                        var ndd = dcc.DocumentNode.SelectSingleNode("//time[@class='datestamp']");
                        if (ndd != null)
                            enteredDate = extractDateTime1(ndd);
                    }
                    if (enteredDate == DateTime.MinValue)
                    {
                        var ndd = dcc.DocumentNode.SelectSingleNode("//div[@class='time']");
                        if (ndd != null)
                            enteredDate = extractDateTime1(ndd);
                    }
                    if (enteredDate == DateTime.MinValue)
                    {
                        var ndd = dcc.DocumentNode.SelectSingleNode("//small[@class='date post_date']");
                        if (ndd != null)
                            enteredDate = extractDateTime1(ndd);
                    }
                    //if (enteredDate == DateTime.MinValue)
                    //{
                    //    var ndd = dcc.DocumentNode.SelectSingleNode("//span[@class='article-timestamp article-timestamp-published']");
                    //    if (ndd != null)
                    //        enteredDate = extractDateTime1(ndd);
                    //}
                    //if (enteredDate == DateTime.MinValue)
                    //{
                    //    var ndd = dcc.DocumentNode.SelectSingleNode("//time[@class='entry-date']");
                    //    if (ndd != null)
                    //        enteredDate = extractDateTime1(ndd);
                    //}
                    //if (enteredDate == DateTime.MinValue)
                    //{
                    //    var ndd = dcc.DocumentNode.SelectSingleNode("//p[@class='publishedDate']");
                    //    if (ndd != null)
                    //        enteredDate = extractDateTime1(ndd);
                    //}
                    if (enteredDate == DateTime.MinValue)
                    {
                        enteredDate = extractDateTimeFromUrl(href);
                        if (enteredDate == DateTime.MinValue)
                            return false;
                    }
                    if (enteredDate < start_date)
                        return true;
                }
                catch (Exception ex)
                {
                    reEventsParser.writeToLog("GetNewsFlorida: " + ex.Message);
                    return false;
                }
            }
            if (enteredDate > DateTime.Now)
                return false;
            article obj = new article();
            obj.title = title;
            obj.href = href.Trim();
            obj.content = "";
            if (attr.Length > 5 && !string.IsNullOrWhiteSpace(attr[5]))
            {
                if (dcc == null)
                {
                    www = new HtmlWeb();
                    dcc = www.Load(href);
                }
                if (attr[5].StartsWith("#c"))
                {
                    string str = attr[5].Substring(2);
                    string[] x = str.Split(new char[] { '-' });
                    try
                    {
                        int n1 = dcc.DocumentNode.InnerHtml.IndexOf(x[0]);
                        if (n1 != -1)
                        {
                            int n2 = dcc.DocumentNode.InnerHtml.IndexOf(x[1], n1);
                            if (n2 != -1)
                            {
                                int n11 = n1 + x[0].Length;
                                int n21 = n2 + x[1].Length;
                                obj.content = dcc.DocumentNode.InnerHtml.Substring(n11, n21 - n11 + 1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        reEventsParser.writeToLog("GetNews: " + ex.Message);
                        return false;
                    }
                }
                else
                {
                    string[] attr5 = attr[5].Split(new char[] { ',' });
                    if (attr5.Length == 1)
                    {
                        var y = dcc.DocumentNode.SelectNodes(attr5[0]);
                        if (y == null)
                            return false;
                        obj.content = "";
                        foreach (var y1 in y)
                        {
                            if (!string.IsNullOrWhiteSpace(obj.content) && !obj.content.EndsWith("."))
                                obj.content += ".";
                            HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
                            doc3.LoadHtml(y1.InnerHtml);
                            IEnumerable<HtmlNode> nodes = doc3.DocumentNode.Descendants().Where(n =>
                                n.NodeType == HtmlNodeType.Text &&
                                n.ParentNode.Name != "script" &&
                                n.ParentNode.Name != "style");
                            foreach (HtmlNode node in nodes)
                            {
                                if (!isRelatedStory(node))
                                    obj.content += " " + node.InnerText.Trim();
                            }
                        }
                    }
                    else
                    {
                        var y = dcc.DocumentNode.SelectSingleNode(attr5[0]);
                        if (y == null)
                            return false;
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(y.InnerHtml);
                        var y1 = doc.DocumentNode.SelectNodes(attr5[1]);
                        if (y1 == null)
                            return false;
                        obj.content = "";
                        foreach (var y2 in y1)
                        {
                            if (y2.Attributes != null && y2.Attributes.Count > 0 && y2.Attributes[0].Name == "class" && y2.Attributes[0].Value == "wp-caption-text")
                                continue;
                            if (!string.IsNullOrWhiteSpace(obj.content) && !obj.content.EndsWith("."))
                                obj.content += ".";
                            HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
                            doc3.LoadHtml(y2.InnerHtml);
                            IEnumerable<HtmlNode> nodes = doc3.DocumentNode.Descendants().Where( n => 
                                n.NodeType == HtmlNodeType.Text &&
                                n.ParentNode.Name != "script" &&
                                n.ParentNode.Name != "style");
                            foreach (HtmlNode node in nodes)
                            {
                                if (!isRelatedStory(node))
                                    obj.content += " " + node.InnerText.Trim();
                            }
                        }
                    }
                    obj.content = WebUtility.HtmlDecode(obj.content.Trim());
                    obj.content = obj.content.Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                }
                obj.content = obj.content.Replace("�", " ");
            }
            obj.title = obj.title.Replace("\n", " ").Replace("   ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            obj.dt = enteredDate;
            obj.sourceId = sourceId;
            obj.sourceTypeId = 1;
            obj.companyname = from;
            obj.content_short = shortContent;
            if (updateWithReEventParser(obj, countryId, 0))
            {
                if (!string.IsNullOrWhiteSpace(obj.content_short))
                    articleData.Add(obj);
            }
            return false;
        }

        bool isRelatedStory(HtmlNode node)
        {
            if (node.Name == "div" && node.Attributes != null && node.Attributes.Count == 1 && node.Attributes[0].Name == "class" && node.Attributes[0].Value.Trim() == "related-stories-inline")
            {
                node = null;
                return true;
            }
            if (node.ParentNode != null)
                return isRelatedStory(node.ParentNode);
            return false;
        }

        void extractHrefTitleDate(HtmlAgilityPack.HtmlNode node, ref string hRef, ref string title, ref DateTime date)
        {
            if (node == null)
                return;
            HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
            doc2.LoadHtml(node.InnerHtml);
            var aNodes = doc2.DocumentNode.SelectNodes("//a");
            if (aNodes != null)
            {
                foreach (var aNode in aNodes)
                {
                    if (hRef == "" && aNode.Attributes["href"] != null)
                        hRef = aNode.Attributes["href"].Value;
                    if (title == "" && aNode.Attributes["title"] != null)
                    {
                        title = aNode.Attributes["title"].Value;
                        System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex("<[^>]*>");
                        title = rx.Replace(title, "");
                    }
                    if (title == "")
                        title = aNode.InnerText.Trim();
                }
            }
            if (date == DateTime.MinValue)
            {
                var x = doc2.DocumentNode.SelectNodes("//div[@class='itemMeta']");
                if (x != null)
                {
                    var y = x[0].SelectSingleNode("//span[@class='dateline']");
                    if (y != null)
                    {
                        date = extractDateTime1(y);
                    }
                }
            }
            if (date == DateTime.MinValue)
            {
                var y = doc2.DocumentNode.SelectSingleNode("//time");
                if (y != null)
                    date = extractDateTime1(y);
            }
            if (date == DateTime.MinValue)
            {
                var y = doc2.DocumentNode.SelectSingleNode("//ul[@class='footer']");
                if (y != null)
                    date = extractDateTime1(y);
            }
            if (date == DateTime.MinValue)
            {
                var y = doc2.DocumentNode.SelectSingleNode("//div[@class='trb_outfit_categorySectionHeading_date']");
                if (y != null)
                {
                    date = extractDateTime1(y);
                }
            }
            title = WebUtility.HtmlDecode(title);
        }

        string loadHTML(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            string html = "";
            request.Timeout = 30000;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(),
                             Encoding.UTF8))
                    {
                        html = reader.ReadToEnd();
                        return html;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("exception " + ex.Message);
            }
            return null;
        }

        DateTime extractDateTime1(HtmlAgilityPack.HtmlNode node)
        {
            if (node == null)
                return DateTime.MinValue;
            string s = node.InnerText.Trim();
            int n = s.IndexOf("\n");
            if (n != -1)
            {
                var s1 = s.Substring(0, n);
                var x = extractDateTime1(s1);
                if (x != DateTime.MinValue)
                    return x;
                s = s.Substring(n+1);
            }
            return extractDateTime1(s);
        }

        DateTime extractDateTimeFromUrl(string s)
        {
            DateTime dt = DateTime.MinValue;
            string strRegValue = "/20[0-3][0-9]/";
            Regex regexYear = new Regex(strRegValue, RegexOptions.None);
            MatchCollection fe = regexYear.Matches(s);
            if (fe.Count > 0)
            {
                int p = fe[0].Index+1;
                s = s.Substring(p);
                if (s.Length > 10)
                    s = s.Substring(0, 10);
                dt = dateFromString(s);
            }
            return dt;
        }

        DateTime extractDateTime1(string s)
        {
            DateTime dt = DateTime.MinValue;
            dt = dateFromString(s);
            if (dt != DateTime.MinValue)
                return dt;
            string strRegValue = "20[0-3][0-9]";
            Regex regexYear = new Regex(strRegValue, RegexOptions.None);
            MatchCollection fe = regexYear.Matches(s);
            if (fe.Count > 0)
            {
                int p = fe[0].Index + 4;
                s = s.Substring(0, p);
                if (s.StartsWith("Published on "))
                    s = s.Substring(13);
                dt = dateFromString(s);
            }
            if (dt == DateTime.MinValue)
            {
                string strRegValue1 = "(\\d\\d )?((Jan)|(Feb)|(Mar)|(Apr)|(May)|(Jun)|(Jul)|(Aug)|(Sep)|(Oct)|(Nov)|(Dec))";
                Regex regexYear1 = new Regex(strRegValue1, RegexOptions.None);
                MatchCollection fe1 = regexYear1.Matches(s);
                if (fe1.Count > 0)
                {
                    int p1 = fe1[fe1.Count - 1].Index;
                    s = s.Substring(p1);
                }
                s = s.Replace("Sept.", "Sep.");
                dt = dateFromString(s);
            }
            return dt;
        }

        void runScannerDB()
        {
            string str = Path.GetDirectoryName(Application.ExecutablePath);
            string exe = Path.Combine(str, "ScannerDB.exe");
            using (var proc = Process.Start(
                new ProcessStartInfo
                {
                    FileName = exe,
 //                   Arguments = "/C " + exe,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = str,
                }))
            {
                proc.WaitForExit();
                proc.Close();
            };
        }

        void scrapSpecialFiles(WebSiteInfo st)
        {
            if (!Directory.Exists("C:\\NYT_corpus"))
                return;
            DirectoryInfo yearDI = new DirectoryInfo("C:\\NYT_corpus");
            foreach (var ydi in yearDI.EnumerateDirectories("*"))
            {
                int year = 0;
                if (!int.TryParse(ydi.Name, out year))
                    continue;
                DirectoryInfo mounthDI = new DirectoryInfo(ydi.FullName);
                foreach (var mdi in mounthDI.EnumerateDirectories("*"))
                {
                    int mounth = Convert.ToInt32(mdi.Name);
                    DirectoryInfo dayDI = new DirectoryInfo(mdi.FullName);
                    foreach (var ddi in dayDI.EnumerateDirectories("*"))
                    {
                        int day = Convert.ToInt32(ddi.Name);
                        DateTime dt = new DateTime(year, mounth, day);
                        string[] filesFolder = Directory.GetFiles(ddi.FullName);
                        foreach (var fileName in filesFolder)
                        {
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.DetectEncodingAndLoad(fileName);
                            bool hasRealEstate = false;
                            var nds = doc.DocumentNode.SelectNodes("//classifier[@class='online_producer']");
                            if (nds != null)
                            {
                                foreach (var x in nds)
                                {
                                    if (x.InnerText.IndexOf("Real Estate") != -1)
                                    {
                                        hasRealEstate = true;
                                        break;
                                    }
                                }
                            }
                            if (!hasRealEstate)
                            {
                                nds = doc.DocumentNode.SelectNodes("//meta");
                                if (nds != null)
                                {
                                    foreach (var x in nds)
                                    {
                                        if (x.Attributes != null && x.Attributes.Count >= 2 && x.Attributes[0].Name == "content" && x.Attributes[0].Value == "Real Estate" &&
                                            x.Attributes[1].Name == "name" && x.Attributes[1].Value == "online_sections")
                                        {
                                            hasRealEstate = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!hasRealEstate)
                            {
                                hasRealEstate = false;
                                continue;
                            }
                            string href = "";
                            string source = "";
                            string title = "";
                            string content = "";
                            string shortContent = "";
                            var nd = doc.DocumentNode.SelectSingleNode("//pubdata");
                            if (nd != null && nd.Attributes != null)
                            {
                                foreach (var x in nd.Attributes)
                                {
                                    if (x.Name == "ex-ref")
                                        href = x.Value;
                                    else if (x.Name == "name")
                                        source = x.Value;
                                }
                            }
                            nd = doc.DocumentNode.SelectSingleNode("//hl1");
                            if (nd != null)
                                title = nd.InnerText;
                            nd = doc.DocumentNode.SelectSingleNode("//block[@class='lead_paragraph']");
                            if (nd != null)
                                shortContent = nd.InnerText;
                            else
                            {
                                nd = doc.DocumentNode.SelectSingleNode("//abstract");
                                if (nd != null)
                                    shortContent = nd.InnerText;
                            }
                            nd = doc.DocumentNode.SelectSingleNode("//block[@class='full_text']");
                            if (nd != null)
                                content = nd.InnerText;
                            if (href != "" && title != "")
                            {
                                article obj = new article();
                                obj.title = title;
                                obj.href = href.Trim();
                                obj.content = content.Trim();
                                obj.dt = dt;
                                obj.sourceId = st.id;
                                obj.sourceTypeId = 1;
                                obj.companyname = source;
                                obj.content_short = shortContent.Trim();
                                if (updateWithReEventParser(obj, st.countryId, 0))
                                {
                                    articleData.Add(obj);
                                }
                            }
                            else
                            {
                                href = "";
                            }
                        }
                    }
                }
            }
        }
    }
}
