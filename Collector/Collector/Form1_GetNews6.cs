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
        public void GetNews_6(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            for (int year = 2015; year >= 2005; year--)
            {
                string url = "http://investor.riocan.com/English/investor-relations/press-releases/" + year + "/";

                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);

                HtmlAgilityPack.HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='ModuleContainerInner']");
                if (divNode == null)
                    continue;

                HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
                doc1.LoadHtml(divNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection divColl = doc.DocumentNode.SelectNodes("//div[contains(@class,'ModuleItemRow ModuleItem')]");
                if (divColl == null)
                    continue;
                this.Invoke((MethodInvoker)delegate
                {
                    lbl_message2.Text = "0 of " + divColl.Count + " articles in Year " + year + " were fetched.";
                    progressBar1.Maximum = divColl.Count;
                    progressBar1.Value = 0;
                });

                int i = 0;
                bool stopFlag = false;
                foreach (HtmlAgilityPack.HtmlNode div in divColl)
                {
                    i++;
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(div.InnerHtml);

                    HtmlAgilityPack.HtmlNode dtNode = doc2.DocumentNode.SelectSingleNode("//span[@class='ModuleDate']");
                    DateTime enteredDate = extractDateTime(dtNode);
                    if (enteredDate == DateTime.MinValue)
                        continue;
                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }
                    string href = "";
                    string title = "";
                    extractHrefAndTitle(div, out href, out title);
                    if (href == "" || title == "")
                        continue;
                    href = "http://investor.riocan.com" + href;
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    article obj = new article();
                    obj.content = "";
                    obj.dt = enteredDate;
                    obj.href = href;
                    obj.title = title;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Riocan REIT";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);

                    this.Invoke((MethodInvoker)delegate
                    {
                        lbl_message2.Text = i + " of " + divColl.Count + " articles in Year " + year + " were fetched.";
                        progressBar1.Value = i;
                    });
                }

                if (stopFlag == true)
                    break;

            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
