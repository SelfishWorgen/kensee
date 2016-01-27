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
        public void GetNews_5(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string start_dt = start_date.ToString("yyyy-MM-dd");
            string cur_dt = DateTime.Now.ToString("yyyy-MM-dd");
            string url = "http://ctreit.mediaroom.com/index.php?searchform=1&start=" + start_dt + "&end=" + cur_dt + "&s=2429";

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Urls fetching...";
            });

            for (int pg = 1; pg < 100; pg++)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lbl_message2.Text = "Urls of Page " + pg + " fetching...";
                });
                string url_ = url + "&o=" + (pg - 1) * 25;
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlNodeCollection itemColl = null;
                try
                {
                    HtmlAgilityPack.HtmlDocument doc = web.Load(url_);
                    itemColl = doc.DocumentNode.SelectNodes("//div[@class='item']");
                }
                catch (Exception ex)
                {
                    reEventsParser.writeToLog("Step 5: " + ex.Message);
                }

                if (itemColl == null || itemColl.Count == 0)
                    break;
                foreach (HtmlAgilityPack.HtmlNode item in itemColl)
                {
                    HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                    doc1.LoadHtml(item.InnerHtml);

                    HtmlAgilityPack.HtmlNode dtNode = doc1.DocumentNode.SelectSingleNode("//div[@class='item_date']");
                    DateTime enteredDate = extractDateTime(dtNode);
                    if (enteredDate == DateTime.MinValue)
                        continue;

                    string title = "";
                    string href = "";
                    HtmlAgilityPack.HtmlNode titleNode = doc1.DocumentNode.SelectSingleNode("//div[@class='story_title']");
                    extractHrefAndTitle(titleNode, out href, out title);
                    if (href == "" || title == "")
                        continue;
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    article obj = new article();
                    obj.dt = enteredDate;
                    obj.href = href;
                    obj.title = title;
                    obj.content = "";
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "CT REIT";
                    articleData.Add(obj);
                }
            }

            if (articleData.Count > 0)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = articleData.Count;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "0 of " + articleData.Count + " articles were fetched.";
                });

                for (int k = 0; k < articleData.Count; )
                {
                    if (!updateWithReEventParser(articleData[k], countryId, sourceTypeId == 2 ? sourceId : 0))
                    {
                        articleData.RemoveAt(k);
                        continue;
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = k + 1;
                        lbl_message2.Text = (k + 1) + " of " + articleData.Count + " articles were fetched.";
                    });
                    k++;
                }
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
