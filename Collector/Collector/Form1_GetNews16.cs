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
        public void GetNews_16(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string url = "http://www.hufvudstaden.se/en/Media/Pressreleases/";

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            int curYear = DateTime.Now.Year;
            bool stopFlag = false;
            for (int i = curYear; i >= 2005; i--)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = curYear - 2005 + 1;
                    progressBar1.Value = curYear - i + 1;
                    lbl_message2.Text = "Articles of Year " + i + " fetching...";
                });

                HtmlAgilityPack.HtmlNode yearDivNode = doc.DocumentNode.SelectSingleNode("//div[@id='year-" + i + "']");
                if (yearDivNode == null)
                    continue;

                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(yearDivNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection divColl = doc1.DocumentNode.SelectNodes("//div[@class='pressrelease']");
                if (divColl == null)
                    continue;
                foreach (HtmlAgilityPack.HtmlNode div in divColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(div.InnerHtml);

                    string dt = "";
                    HtmlAgilityPack.HtmlNode dtNode = doc2.DocumentNode.SelectSingleNode("//p[@class='pressrelease-date']");
                    HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                    if (dtNode == null || hrefNode == null)
                        continue;
                    dt = dtNode.InnerText.Trim();
                    string[] pieces = dt.Split(new string[] { " " }, StringSplitOptions.None);

                    dt = pieces[0] + " " + pieces[1] + ", " + i;

                    DateTime enteredDate = dateFromString(dt);

                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    string title = hrefNode.InnerText.Trim();
                    string href = "http://www.hufvudstaden.se" + hrefNode.Attributes["href"].Value;
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
                    obj.companyname = "Hufvudstaden";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);
                }

                if (stopFlag == true)
                    break;
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
