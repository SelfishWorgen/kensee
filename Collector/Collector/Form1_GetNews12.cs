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
        public void GetNews_12(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            string url = "http://www.echo.com.pl/en/press-room/press-releases/";

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                lbl_message2.Text = "Connecting...";
            });

            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);

            int pgNum = 20;
            HtmlAgilityPack.HtmlNode lastPgNode = doc.DocumentNode.SelectSingleNode("//ul[@class='pager']");
            if (lastPgNode != null)
            {
                HtmlAgilityPack.HtmlDocument dd = new HtmlAgilityPack.HtmlDocument();
                dd.LoadHtml(lastPgNode.InnerHtml);

                HtmlAgilityPack.HtmlNodeCollection lis = dd.DocumentNode.SelectNodes("//li");
                if (lis != null)
                {
                    HtmlAgilityPack.HtmlNode lastLi = lis[lis.Count - 1];
                    if (lastLi.ChildNodes["a"] != null)
                    {
                        string ss = lastLi.ChildNodes["a"].Attributes["href"].Value;
                        string[] pieces = ss.Split(new string[] { "press-releases/" }, StringSplitOptions.None);
                        if (pieces.Length > 1)
                        {
                            pieces = pieces[1].Split(new string[] { "/" }, StringSplitOptions.None);
                            pgNum = Convert.ToInt32(pieces[0]);
                        }
                    }
                }
            }

            url = "http://www.echo.com.pl/en/press-room/press-releases/{0}/?nbsp;=&tx_ttnews%5Bdate_to%5D=23-04-2015";

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

                string url_ = string.Format(url, pg);
                if (pg == 0)
                    url_ = url_.Replace("press-releases/0/", "press-releases/");
                HtmlAgilityPack.HtmlWeb web1 = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc1 = web1.Load(url_);

                HtmlAgilityPack.HtmlNodeCollection divColl = doc1.DocumentNode.SelectNodes("//div[@class='news-list-item']");
                if (divColl == null)
                    continue;
                foreach (HtmlAgilityPack.HtmlNode div in divColl)
                {
                    HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                    doc2.LoadHtml(div.InnerHtml);

                    string dt = "";
                    HtmlAgilityPack.HtmlNode dtNode = doc2.DocumentNode.SelectSingleNode("//span[@class='news-list-date']");
                    if (dtNode == null)
                        continue;
                    dt = dtNode.InnerText.Trim();
                    string[] pieces = dt.Split(new string[] { "-" }, StringSplitOptions.None);
                    dt = pieces[2] + "-" + pieces[1] + "-" + pieces[0];
                    DateTime enteredDate = dateFromString(dt);

                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }

                    string title = "", href = "";
                    HtmlAgilityPack.HtmlNode hrefNode = doc2.DocumentNode.SelectSingleNode("//a");
                    if (hrefNode == null)
                        continue;
                    title = hrefNode.InnerText.Trim();
                    href = "http://www.echo.com.pl" + hrefNode.Attributes["href"].Value;
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
                    obj.companyname = "news";

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
