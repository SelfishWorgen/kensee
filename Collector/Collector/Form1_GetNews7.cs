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
        public void GetNews_7(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            int pgNum = 1;
            string url1 = "http://www.snl.com/irweblinkx/news.aspx?iid=4143678&start=1&mode=1";
            HtmlAgilityPack.HtmlWeb www = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument dcc = www.Load(url1);
            HtmlAgilityPack.HtmlNode tdNode = dcc.DocumentNode.SelectSingleNode("//td[@class='data defaultbold']");
            if (tdNode != null)
            {
                string td = tdNode.InnerText.Trim();
                string[] pieces = td.Split(new string[] { " of " }, StringSplitOptions.None);
                td = pieces[1];
                pgNum = Convert.ToInt32(td) / 20;
                if (Convert.ToInt32(td) % 20 != 0)
                    pgNum++;
            }

            List<string> hrefs = new List<string>();
            bool stopFlag = false;
            for (int i = 1; i <= pgNum; i++)
            {
                if (stopFlag == true)
                    break;
                string url_ = "http://www.snl.com/irweblinkx/news.aspx?iid=4143678&start=" + (i - 1) * 20 + 1 + "&mode=1";
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url_);

                HtmlAgilityPack.HtmlNode tbNode = doc.DocumentNode.SelectSingleNode("//table[@id='PRTable']");
                if (tbNode == null)
                    continue;
                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(tbNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection trColl = doc1.DocumentNode.SelectNodes("//tr[@class='data']");
                if (trColl == null || trColl.Count == 0)
                    continue;
                this.Invoke((MethodInvoker)delegate
                {
                    lbl_message2.Text = "Urls of articles in Page " + i + " fetching...";
                });

                foreach (HtmlAgilityPack.HtmlNode tr in trColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(tr.InnerHtml);

                    string dt = "";
                    HtmlAgilityPack.HtmlNode tddateNode = doc2.DocumentNode.SelectSingleNode("//td");
                    dt = tddateNode.InnerText;
                    DateTime enteredDate = dateFromString(dt);
                    dt = enteredDate.ToString("yyyy-MM-dd");

                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    string href = "";
                    HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                    if (hrefNode != null)
                    {
                        href = "http://www.snl.com/irweblinkx/" + hrefNode.Attributes["href"].Value;
                        href = href.Replace("&amp;", "&");
                        hrefs.Add(href);
                    }
                }
            }

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "0 of " + hrefs.Count + " articles were fetched.";
                progressBar1.Maximum = hrefs.Count;
                progressBar1.Value = 0;
            });

            for (int i = 0; i < hrefs.Count; i++)
            {
                string href = hrefs[i];

                HtmlAgilityPack.HtmlWeb web1 = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc3 = web1.Load(href);

                string title = "";
                HtmlAgilityPack.HtmlNode titleNode = doc3.DocumentNode.SelectSingleNode("//div[@id='PRHedline']");
                HtmlAgilityPack.HtmlNode dtNode = doc3.DocumentNode.SelectSingleNode("//div[@class='PRDatetime']");
                if (titleNode == null || dtNode == null)
                    continue;
                title = titleNode.InnerText;

                string dt = "";
                dt = dtNode.InnerText;
                dt = dt.Split(new string[] { " - " }, StringSplitOptions.None)[1];
                DateTime enteredDate = dateFromString(dt);
                if (enteredDate == DateTime.MinValue)
                    continue;
                if (url_dict.ContainsKey(href))
                    continue;
                url_dict.Add(href, false);
                article obj = new article();
                obj.sourceId = sourceId;
                obj.sourceTypeId = sourceTypeId;
                obj.content = "";
                obj.dt = enteredDate;
                obj.href = href;
                obj.title = title;
                obj.companyname = "Crombie REIT";

                if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                    articleData.Add(obj);

                this.Invoke((MethodInvoker)delegate
                {
                    lbl_message2.Text = (i + 1) + " of " + hrefs.Count + " articles were fetched.";
                    progressBar1.Maximum = hrefs.Count;
                    progressBar1.Value = i + 1;
                });
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }

}