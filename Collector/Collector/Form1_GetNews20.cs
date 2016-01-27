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
        public void GetNews_20(int stepNumber)
        {
            if (!beforeStep(stepNumber, 2, 0, 6))
                return;
            reEventsParser.writeToLog("Scrapping from Yahoo");
            List<companyTicker> yahooTickers = getCompanyTickers("yahoo");
            string companyName = "";
            string ticker = "";
            string country = "";
            string sector = "N/A";
            int countryId = 0;
            int sourceId = 0;
            int sourceTypeId = 2;
            for (int i = 0; i < yahooTickers.Count; i++)
            {
                companyName = yahooTickers[i].companyName;
                ticker = yahooTickers[i].ticker;
                country = yahooTickers[i].country;
                sector = yahooTickers[i].industry;
                countryId = yahooTickers[i].countryId;
                sourceId = yahooTickers[i].id;

                if (ticker == "")
                    continue;
                string url = "http://finance.yahoo.com/q/h?s=" + ticker + "+Headlines";

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = 0;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "Connecting...";
                });

                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);

                HtmlAgilityPack.HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='mod yfi_quote_headline withsky']");
                if (divNode == null)
                {
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                    continue;
                }
                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(divNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection h3Coll = doc1.DocumentNode.SelectNodes("//h3");
                if (h3Coll == null)
                {
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                    continue;
                }
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = h3Coll.Count;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "0 of " + h3Coll.Count + " articles were fetched.";
                });

                int ii = 0;
                int iii = 0;
                foreach (HtmlAgilityPack.HtmlNode h3 in h3Coll)
                {
                    string dt = h3.InnerText.Trim();
                    ii++;

                    if (dt == "")
                        continue;
                    DateTime enteredDate = dateFromString(dt);

                    if (enteredDate < start_date)
                        break;

                    HtmlAgilityPack.HtmlNode ulNode = h3.NextSibling;
                    if (ulNode == null || ulNode.ChildNodes.Count == 0)
                        continue;

                    HtmlAgilityPack.HtmlNode hrefNode = null;
                    foreach (HtmlAgilityPack.HtmlNode h in ulNode.ChildNodes)
                    {
                        if (h.Name != "li")
                            continue;
                        if (h.ChildNodes.Count == 0)
                            continue;
                        hrefNode = h.SelectSingleNode("//a");
                        if (hrefNode == null)
                            continue;
                        string href = hrefNode.Attributes["href"].Value;
                        string title = hrefNode.InnerText;
                        if (url_dict.ContainsKey(href))
                            continue;
                        url_dict.Add(href, false);
                        article obj = new article();
                        obj.dt = enteredDate;
                        obj.content = "";
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

                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = ii;
                            lbl_message2.Text = iii.ToString() + " Articles were fetched...";
                        });
                    }
                }
                if (iii > 0)
                    reEventsParser.writeToLog(iii.ToString() + " articles found for " + companyName + ", url " + url);
                else
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
