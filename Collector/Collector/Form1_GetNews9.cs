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
        public void GetNews_9(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            List<string> hrefs = new List<string>();
            int year = DateTime.Now.Year;
            int year1 = year;
            bool stopFlag = false;
            for (; year >= 2006; year--)
            {
                if (stopFlag == true)
                    break;
                string url_ = "http://www.callowayreit.com/News/default.aspx?arch=&year=" + year;
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url_);

                HtmlAgilityPack.HtmlNode tbNode = doc.DocumentNode.SelectSingleNode("//div[@class='emptyTable']");
                if (tbNode == null)
                    continue;
                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(tbNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection trColl = doc1.DocumentNode.SelectNodes("//tr");
                if (trColl == null || trColl.Count == 0)
                    continue;
                foreach (HtmlAgilityPack.HtmlNode tr in trColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(tr.InnerHtml);

                    string href = "";
                    string title = "";
                    HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                    if (hrefNode == null)
                        continue;
                    href = "http://www.callowayreit.com" + hrefNode.Attributes["href"].Value;
                    href = WebUtility.HtmlDecode(href);
                    title = hrefNode.InnerText;

                    string dt = "";
                    dt = tr.InnerText.Replace(title, "");
                    DateTime enteredDate = dateFromString(dt);
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
                    obj.content = "";
                    obj.href = href;
                    obj.title = title;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Calloway REIT";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);
                }

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = year1 - 2006 + 1;
                    progressBar1.Value = year1 - year + 1;
                    lbl_message2.Text = "Articles in Year " + year + " fetching...";
                });
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
