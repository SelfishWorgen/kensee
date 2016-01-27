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
        public void GetNews_17(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string url = "http://investors.platzer.se/en/pressreleases";

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);
            HtmlAgilityPack.HtmlNodeCollection divColl = doc.DocumentNode.SelectNodes("//div[@class='views-row-spacer']");
            if (divColl == null)
            {
                afterStep(stepNumber);
                return;
            }

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = divColl.Count;
                progressBar1.Value = 0;
                lbl_message2.Text = "0 of " + divColl.Count + " Articles were fetched...";
            });

            foreach (HtmlAgilityPack.HtmlNode div in divColl)
            {
                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(div.InnerHtml);

                HtmlAgilityPack.HtmlNode dtNode = doc1.DocumentNode.SelectSingleNode("//span[@class='date-display-single']");
                HtmlAgilityPack.HtmlNode hrefNode = doc1.DocumentNode.SelectSingleNode("//a");
                if (dtNode == null || hrefNode == null)
                    continue;
                string dt = dtNode.InnerText;
                string[] pieces = dt.Split(new string[] { " " }, StringSplitOptions.None);
                dt = pieces[0];
                DateTime enteredDate = dateFromString(dt);
                if (enteredDate < start_date)
                    break;

                string title = hrefNode.InnerText.Trim();
                string href = "http://investors.platzer.se" + hrefNode.Attributes["href"].Value;
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
                obj.companyname = "Platzer Fastigheter Holding";

                if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                    articleData.Add(obj);
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = articleData.Count;
                    lbl_message2.Text = articleData.Count + " of " + divColl.Count + " Articles were fetched...";
                });
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
