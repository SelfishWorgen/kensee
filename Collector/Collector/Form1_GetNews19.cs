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
        public void GetNews_19(int stepNumber)
        {
            if (!beforeStep(stepNumber, 2, 0, 6))
                return;
            reEventsParser.writeToLog("Scrapping from Reuters");
            int sourceTypeId = 2;
            List<companyTicker> reutersTickers = getCompanyTickers("reuters");
            string companyName = "";
            string ticker = "";
            string country = "";
            string sector = "N/A";
            int countryId = 0;
            int sourceId = 0;
            for (int i = 0; i < reutersTickers.Count; i++)
            {
                companyName = reutersTickers[i].companyName;
                ticker = reutersTickers[i].ticker;
                country = reutersTickers[i].country;
                sector = reutersTickers[i].industry;
                countryId = reutersTickers[i].countryId;
                sourceId = reutersTickers[i].id;

                if (ticker == "")
                    continue;
                string url = "http://www.reuters.com/finance/stocks/companyNews?symbol=" + ticker;

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = 0;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "Connecting...";
                });

                HtmlAgilityPack.HtmlWeb wb1 = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = wb1.Load(url);

                HtmlAgilityPack.HtmlNode divPanel = doc.DocumentNode.SelectSingleNode("//div[@class='column2 gridPanel grid4']");
                if (divPanel == null)
                {
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                    continue;
                }
                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(divPanel.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection moduleColl = doc1.DocumentNode.SelectNodes("//div[@class='module']");
                if (moduleColl == null)
                {
                    reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                    continue;
                }
                int ii = 0;
                int iii = 0;
                foreach (HtmlAgilityPack.HtmlNode module in moduleColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(module.InnerHtml);

                    string header = "";
                    HtmlAgilityPack.HtmlNode headerNode = doc2.DocumentNode.SelectSingleNode("//div[@class='moduleHeader']");
                    if (headerNode != null)
                        header = headerNode.InnerText.Trim();

                    if (header == "Press Releases")
                    {
                        HtmlAgilityPack.HtmlNodeCollection liColl = doc2.DocumentNode.SelectNodes("//li");
                        if (liColl == null)
                        {
                            reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                            break;
                        }
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Maximum = liColl.Count;
                            progressBar1.Value = 0;
                            lbl_message2.Text = "0 of " + liColl.Count + " articles were fetched.";
                        });

                        foreach (HtmlAgilityPack.HtmlNode li in liColl)
                        {
                            HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
                            doc3.LoadHtml(li.InnerHtml);

                            HtmlAgilityPack.HtmlNode hrefNode = doc3.DocumentNode.SelectSingleNode("//a");
                            if (hrefNode == null)
                                continue;

                            string title = hrefNode.InnerText.Trim();
                            string href1 = hrefNode.Attributes["href"].Value;
                            string href = "http://www.reuters.com" + hrefNode.Attributes["href"].Value;
                            string[] pieces = href1.Split(new string[] { "/" }, StringSplitOptions.None);
                            string dt = pieces[2] + "-" + pieces[3] + "-" + pieces[4];

                            if (dt == "")
                                continue;
                            DateTime enteredDate = dateFromString(dt);
                            if (enteredDate < start_date)
                                break;
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
                                progressBar1.Value = ii + 1;
                                lbl_message2.Text = articleData.Count + " of " + liColl.Count + " Articles were fetched...";
                            });

                            ii++;
                        }
                        if (iii > 0)
                            reEventsParser.writeToLog(iii.ToString() + " articles found for " + companyName + ", url " + url);
                        else
                            reEventsParser.writeToLog("No one articles found for " + companyName + ", url " + url);
                        break;
                    }
                }
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
