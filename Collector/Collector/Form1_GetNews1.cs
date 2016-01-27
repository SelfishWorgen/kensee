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
        public void GetNews_1(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 12))
                return;
            
            int pgNum = 0;
            string url_ = "http://reitnews.commercialcanada.ca/?paged=2";
            HtmlAgilityPack.HtmlWeb www = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument dcc = www.Load(url_);
            string title1 = dcc.DocumentNode.SelectSingleNode("//title").InnerText;
            string[] pieces = title1.Split(new string[] { "Page 2 of " }, StringSplitOptions.None);
            title1 = pieces[1];
            pieces = title1.Split(new string[] { " " }, StringSplitOptions.None);
            title1 = pieces[0];
            pgNum = Convert.ToInt32(title1);

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = pgNum;
                progressBar1.Value = 0;
            });

            bool stopFlag = false;

            for (int i = 1; i <= pgNum; i++)
            {
                if (stopFlag)
                    break;
                string url = "http://reitnews.commercialcanada.ca/?paged=" + i;

                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = null;
                try
                {
                    doc = web.Load(url);
                }
                catch (Exception ex)
                {
                    continue;
                }
                HtmlAgilityPack.HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article");
                if (articles == null)
                    continue;
                foreach (HtmlAgilityPack.HtmlNode article in articles)
                {
                    string title = "";
                    string href = "";
                    DateTime enteredDate = DateTime.MinValue;

                    HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                    doc1.LoadHtml(article.InnerHtml);

                    HtmlAgilityPack.HtmlNode p = doc1.DocumentNode.SelectSingleNode("//time[@class='entry-date']");
                    enteredDate = extractDateTime(p);

                    if (enteredDate == DateTime.MinValue)
                        continue;

                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    HtmlAgilityPack.HtmlNode titleNode = doc1.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");
                    extractHrefAndTitle(titleNode, out href, out title);

                    if (href == "" || title == "")
                        continue;
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    article obj = new article();
                    obj.title = title;
                    obj.href = href.Trim();
                    obj.dt = enteredDate;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "REIT Canada";
                    obj.content = "";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);
                }

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = i;
                    lbl_message2.Text = "Articles of Page " + i.ToString() + " were fetched.";
                });
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
