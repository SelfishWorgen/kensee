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
        public void GetNews_4(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string url = "http://www.firstcapitalrealty.ca/Pressroom.aspx";

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = null;
            try
            {
                doc = web.Load(url);
            }
            catch (Exception ex)
            {
                return;
            }
            HtmlAgilityPack.HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//div[@class='body']");
            if (bodyNode == null)
            {
                afterStep(stepNumber);
                return;
            }
            HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
            doc1.LoadHtml(bodyNode.InnerHtml);

            HtmlAgilityPack.HtmlNodeCollection itemNodeColl = doc1.DocumentNode.SelectNodes("//p");
            if (itemNodeColl == null)
            {
                afterStep(stepNumber);
                return;
            }
            foreach (HtmlAgilityPack.HtmlNode itemNode in itemNodeColl)
            {
                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(itemNode.InnerHtml);

                HtmlAgilityPack.HtmlNode strongNode = doc2.DocumentNode.SelectSingleNode("//strong");
                DateTime enteredDate = extractDateTime(strongNode);
                if (enteredDate == DateTime.MinValue)
                    continue;
                if (enteredDate < start_date)
                    break;

                string title = "";
                string href = "";
                extractHrefAndTitle(itemNode, out href, out title);
                if (title == "" || href == "")
                    continue;

                href = "http://www.firstcapitalrealty.ca/" + href;
                string html = loadHTML(href);
                if (html == null)
                    continue;
                HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
                doc3.LoadHtml(html);

                HtmlAgilityPack.HtmlNode embedNode = doc3.DocumentNode.SelectSingleNode("//embed");
                if (embedNode == null || embedNode.Attributes.Count == 0)
                    continue;
                href = embedNode.Attributes["src"].Value;
                if (href == null)
                    continue;
                href = "http://www.firstcapitalrealty.ca/" + href;
                int k = href.IndexOf(".pdf");
                if (k > 0)
                    href = href.Substring(0, k + 4);
                if (url_dict.ContainsKey(href))
                    continue;
                url_dict.Add(href, false);
                article obj = new article();
                obj.dt = enteredDate;
                obj.title = title;
                obj.href = href;
                obj.content = "";
                obj.sourceId = sourceId;
                obj.sourceTypeId = sourceTypeId;
                obj.companyname = "First Capital Realty Inc";

                if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                    articleData.Add(obj);
            }
            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = articleData.Count + " articles were fetched.";
                progressBar1.Maximum = articleData.Count;
                progressBar1.Value = articleData.Count;
            });

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
