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
        public void GetNews_15(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            int curYear = DateTime.Now.Year;
            bool stopFlag = false;
            List<string> dtList = new List<string>();
            List<string> titleList = new List<string>();
            List<string> hrefList = new List<string>();
            for (int i = curYear; i >= 1999; i--)
            {
                dtList.Clear();
                titleList.Clear();
                hrefList.Clear();

                string url = "http://www.sonaesierra.com/en-GB/pressroom/news/" + i + "/default.aspx";

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Maximum = curYear - 1999 + 1;
                    progressBar1.Value = curYear - i + 1;
                    lbl_message2.Text = "Articles of Year " + i + " fetching...";
                });

                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(url);

                HtmlAgilityPack.HtmlNodeCollection dtColl = doc.DocumentNode.SelectNodes("//td[@class='news_date']");
                HtmlAgilityPack.HtmlNodeCollection titleColl = doc.DocumentNode.SelectNodes("//a[@class='links_PR_News']");
                if (dtColl == null || titleColl == null)
                    continue;
                foreach (HtmlAgilityPack.HtmlNode dt in dtColl)
                    dtList.Add(dt.InnerText.Trim());
                foreach (HtmlAgilityPack.HtmlNode title in titleColl)
                {
                    hrefList.Add("http://www.sonaesierra.com" + title.Attributes["href"].Value);
                    titleList.Add(title.InnerText.Trim());
                }

                for (int k = 0; k < dtColl.Count; k++)
                {
                    string dt = dtList[k];
                    string title_txt = titleList[k];
                    string href_txt = hrefList[k];

                    string[] pieces = dt.Split(new string[] { "-" }, StringSplitOptions.None);
                    dt = pieces[2] + "-" + pieces[1] + "-" + pieces[0];

                    DateTime enteredDate;
                    if (!DateTime.TryParseExact(dt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out enteredDate))
                        continue;

                    if (enteredDate < start_date)
                    {
                        stopFlag = true;
                        break;
                    }
                    if (url_dict.ContainsKey(href_txt))
                        continue;
                    url_dict.Add(href_txt, false);
                    article obj = new article();
                    obj.dt = enteredDate;
                    obj.content = "";
                    obj.href = href_txt;
                    obj.title = title_txt;
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Sonae Sierra Brasil";
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
