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
        public void GetNews_2(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId,12))
                return;

            string url = "http://www.prnewswire.com/news-releases/financial-services-latest-news/real-estate-list/";

            bool stopFlag = false;
            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Urls fetching...";
            });

            for (int pg = 1; pg < 30; pg++)
            {
                if (stopFlag == true)
                    break;

                string url_ = url + "?c=n&pagesize=200&page=" + pg;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_);
                request.Headers.Add("Accept-Language", "de-DE");
                request.Method = "GET";
                request.Accept = "text/html";
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                string html = "";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(),
                             Encoding.UTF8))
                    {
                        html = reader.ReadToEnd();
                    }
                }

                doc.LoadHtml(html);

                HtmlAgilityPack.HtmlNodeCollection dtColl = doc.DocumentNode.SelectNodes("//div[@class='col-sm-3 col-md-2']");
                if (dtColl == null || dtColl.Count == 0)
                    break;
                HtmlAgilityPack.HtmlNodeCollection titleColl = doc.DocumentNode.SelectNodes("//div[@class='col-sm-9 col-md-10']");
                for (int k = 0; k < dtColl.Count; k++)
                {
                    HtmlAgilityPack.HtmlNode dtNode = dtColl[k];
                    DateTime enteredDate = extractDateTime(dtNode);
                    if (enteredDate == DateTime.MinValue)
                        enteredDate = DateTime.Now;
                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    HtmlAgilityPack.HtmlNode titleNode = titleColl[k];
                    string title, href;
                    extractHrefAndTitle(titleNode, out href, out title);

                    if (title == "" || href == "")
                        continue;
                    if (!isTextEnglish(title))
                    {
                        reEventsParser.writeToLog("Article title was identifies as non-english " + href);
                        continue;
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
                    obj.companyname = "PR newswire";
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

                for (int k = 0; k < articleData.Count;)
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
