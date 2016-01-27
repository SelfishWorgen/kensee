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
        public void GetNews_11(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string url = "http://www.citycon.com/content/releases";

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            int pgNum = 20;
            HtmlAgilityPack.HtmlNode lastPgNode = doc.DocumentNode.SelectSingleNode("//li[@class='pager-last last']");
            if (lastPgNode != null && lastPgNode.ChildNodes != null)
            {
                foreach (HtmlAgilityPack.HtmlNode n in lastPgNode.ChildNodes)
                {
                    if (n.Name == "a")
                    {
                        string ss = n.Attributes["href"].Value;
                        string[] pieces = ss.Split(new string[] { "page=" }, StringSplitOptions.None);
                        if (pieces.Length > 1)
                            pgNum = Convert.ToInt32(pieces[1]);
                        break;
                    }
                }
            }

            url = "http://www.citycon.com/content/releases?field_release_type_tid_selective=All&field_original_post_date_value_selective=All&title=&page=";

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = pgNum + 1;
                progressBar1.Value = 0;
            });

            bool stopFlag = false;
            for (int pg = 0; pg <= pgNum; pg++)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = pg + 1;
                    lbl_message2.Text = "Articles of Page " + (pg + 1) + " Fetching...";
                });

                string url_ = url + pg;
                HtmlAgilityPack.HtmlWeb web1 = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc1 = web1.Load(url_);

                HtmlAgilityPack.HtmlNodeCollection divColl = doc1.DocumentNode.SelectNodes("//div[@class='media-archive-item']");
                if (divColl != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode div in divColl)
                    {
                        HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                        doc2.LoadHtml(div.InnerHtml);

                        //
                        string title = "", href = "";
                        HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                        if (hrefNode == null)
                            continue;
                        title = hrefNode.InnerText.Trim();
                        href = "http://www.citycon.com" + hrefNode.Attributes["href"].Value;

                        string content = "";
                        HtmlAgilityPack.HtmlWeb contWeb = new HtmlWeb();
                        HtmlAgilityPack.HtmlDocument contDoc = contWeb.Load(href);
                        HtmlAgilityPack.HtmlNode contNode = contDoc.DocumentNode.SelectSingleNode("//div[@class='field-release-text']");
                        if (contNode != null)
                            content = contNode.InnerHtml.Trim();

                        HtmlAgilityPack.HtmlNode dtNode = contDoc.DocumentNode.SelectSingleNode("//div[@class='field-sub-headline']");
                        DateTime enteredDate = extractDateTime(dtNode);
                        if (enteredDate == DateTime.MinValue)
                        {
                            dtNode = contDoc.DocumentNode.SelectSingleNode("//strong");
                            enteredDate = extractDateTime(dtNode);
                        }
                        if (enteredDate == DateTime.MinValue)
                            continue;

                        if (enteredDate < start_date)
                        {
                            stopFlag = true;
                            break;
                        }
                        if (url_dict.ContainsKey(href))
                            continue;
                        url_dict.Add(href, false);
                        article obj = new article();
                        obj.dt = enteredDate;
                        obj.content = content;
                        obj.href = href;
                        obj.title = title;
                        obj.sourceId = sourceId;
                        obj.sourceTypeId = sourceTypeId;
                        obj.companyname = "Citycon Oyj";

                        if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                            articleData.Add(obj);
                    }
                }

                if (stopFlag == true)
                    break;
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
