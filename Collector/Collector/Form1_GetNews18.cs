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

namespace Collector
{
    public partial class Form1 : Form
    {
        public void GetNews_18(int stepNumber)
        {
            if (!beforeStep(stepNumber, 2, 0, 6))
                return;
            reEventsParser.writeToLog("Scrapping from Bloomberg");
            int sourceTypeId = 2;
            List<companyTicker> bloombergTickers = getCompanyTickers("bloomberg");

            string companyName = "";
            string ticker = "";
            string country = "";
            string sector = "N/A";
            int countryId = 0;
            int sourceId = 0;
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = bloombergTickers.Count;
                progressBar1.Value = 0;
                lbl_message2.Text = "0 of " + bloombergTickers.Count.ToString() + " sites were fetched.";
            });
           
            
            for (int i = 0; i < bloombergTickers.Count; i++)
            {
                ticker = bloombergTickers[i].ticker;
                country = bloombergTickers[i].country;
                sector = bloombergTickers[i].industry;
                companyName = bloombergTickers[i].companyName;
                countryId = bloombergTickers[i].countryId;
                sourceId = bloombergTickers[i].id;


                if (ticker == "")
                    continue;
                string url = "http://www.bloomberg.com/quote/" + ticker;


                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);


                HtmlAgilityPack.HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes("//article");
                if (articleNodes == null)
                {
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                    continue;
                }

                int iii = 0;
                foreach (HtmlAgilityPack.HtmlNode articleNode in articleNodes)
                {
                    if (articleNode.InnerHtml == null)
                        continue;
                    HtmlAgilityPack.HtmlDocument docArticle = new HtmlAgilityPack.HtmlDocument();
                    docArticle.LoadHtml(articleNode.InnerHtml);
                    if (docArticle.DocumentNode.ChildNodes == null)
                        continue;
                    HtmlAgilityPack.HtmlNode refNode = docArticle.DocumentNode.SelectSingleNode("//a");
                    if (refNode == null)
                        continue;
                    HtmlAgilityPack.HtmlNode timeNode = articleNode.SelectSingleNode("//time");
                    if (timeNode == null || refNode.Attributes["href"] == null)
                        continue;
                    string href = refNode.Attributes["href"].Value;
                    if (string.IsNullOrEmpty(href))
                        continue;
                    string title = refNode.InnerText;
                    if (string.IsNullOrEmpty(title))
                        continue;
                    string timeText = timeNode.InnerText;
                    if (string.IsNullOrEmpty(timeText))
                        continue;
                    DateTime dt;
                    if (!DateTime.TryParse(timeText.Trim(), out dt))
                        continue;
                    if (dt < start_date)
                        continue;

                    Uri uriResult;
                    bool result = Uri.TryCreate(href, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                    if (!result)
                        continue;
                    string html = loadHTML(href);
                    if (html == null)
                        break;
                    doc.LoadHtml(html);
                    var y = doc.DocumentNode.SelectSingleNode("//div[@class='article-body__content']");
                    article obj = new article();
                    obj.content = "";
                    if (y == null)
                        y = doc.DocumentNode.SelectSingleNode("//div[@class='article_body']");
                    if (y != null)
                    {
                        foreach (var x in y.ChildNodes)
                            obj.content += x.InnerText;
                        obj.content = obj.content.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                        obj.content = WebUtility.HtmlDecode(obj.content);
                    }
                    var ya = doc.DocumentNode.SelectNodes("//meta");
                    if (ya != null)
                    {
                        foreach (var x in ya)
                        {
                            if (x.Attributes != null && x.Attributes.Count == 2 && x.Attributes[0].Name == "content" && x.Attributes[1].Name == "name" && x.Attributes[1].Value == "description")
                            {
                                obj.content_short = x.Attributes[0].Value;
                                obj.content_short = obj.content_short.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                                obj.content_short = WebUtility.HtmlDecode(obj.content_short);
                                break;
                            }
                        }
                    }
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    obj.dt = dt;
                    obj.href = href;
                    obj.title = title;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = companyName;

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                    {
                        iii++;
                        articleData.Add(obj);
                    }
                }
                if (iii > 0)
                    reEventsParser.writeToLog(iii.ToString() + " articles found for " + companyName + ", url " + url);
                else
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = i;
                    lbl_message2.Text = i.ToString() + " sites  of " + bloombergTickers.Count.ToString() + " were fetched. (" + articleData.Count.ToString() + " articles)";
                });

            }
            storeMySQL();
            afterStep(stepNumber);
        }
    }
}