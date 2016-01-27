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
        public void GetNews_10(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string[] urls = { "http://www.swiss-prime-site.ch/en/service/media", "http://www.swiss-prime-site.ch/en/service/media/archiv" };

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });

            foreach (string url in urls)
            {
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);

                HtmlAgilityPack.HtmlNode tbodyNode = doc.DocumentNode.SelectSingleNode("//tbody");
                if (tbodyNode == null)
                    continue;

                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(tbodyNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection trColl = doc2.DocumentNode.SelectNodes("//tr");
                if (trColl == null)
                    continue;

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = trColl.Count;
                    progressBar1.Value = 0;
                    lbl_message2.Text = "0 Articles were fetched...";
                });

                for (int i = 0; i < trColl.Count; i++)
                {
                    HtmlAgilityPack.HtmlDocument doc3 = new HtmlAgilityPack.HtmlDocument();
                    doc3.LoadHtml(trColl[i].InnerHtml);

                    HtmlAgilityPack.HtmlNodeCollection tdColl = doc3.DocumentNode.SelectNodes("//td");
                    if (tdColl == null)
                        continue;

                    string dt = tdColl[0].InnerText.Replace("&nbsp;", " ");
                    string title = tdColl[1].InnerText;

                    HtmlAgilityPack.HtmlDocument doc_ = new HtmlAgilityPack.HtmlDocument();
                    doc_.LoadHtml(tdColl[2].InnerHtml);

                    HtmlAgilityPack.HtmlNode linkNode = doc_.DocumentNode.SelectSingleNode("//a");
                    if (linkNode == null)
                        continue;

                    string href = "http://www.swiss-prime-site.ch" + linkNode.Attributes["href"].Value;

                    DateTime enteredDate = dateFromString(dt);

                    if (enteredDate < start_date)
                        break;
                    if (url_dict.ContainsKey(href))
                        continue;
                    url_dict.Add(href, false);
                    article obj = new article();
                    obj.dt = enteredDate;
                    obj.href = href;
                    obj.title = title;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Swiss Prime Site";
                    obj.content = "";

                    if (updateWithReEventParser(obj, countryId, sourceTypeId == 2 ? sourceId : 0))
                        articleData.Add(obj);

                    this.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = articleData.Count;
                        lbl_message2.Text = articleData.Count.ToString() + " Articles were fetched...";
                    });
                }
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
