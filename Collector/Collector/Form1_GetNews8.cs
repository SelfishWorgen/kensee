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
        public void GetNews_8(int stepNumber)
        {
            int sourceId = 0;
            int countryId = 0;
            int sourceTypeId = getSourceTypeId(stepNumber, out sourceId, out countryId);
            if (sourceId == 0)
                return;
            if (!beforeStep(stepNumber, sourceTypeId, sourceId, 6))
                return;

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Urls of articles in Year 2013 fetching...";
            });
            string html_2013 = "<div class=\"NewsListContainer\"><div class=\"NewsItemRow\"><div class=\"Date\">Dec 17, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-December-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of December, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Nov 19, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Implements-Distribution-Reinvestment-Plan/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Implements Distribution Reinvestment Plan</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Nov 11, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-November-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of November, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Nov 11, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Reports-Solid-Third-Quarter-2013-Results/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Reports Solid Third Quarter 2013 Results</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Oct 22, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Completes-Acquisition-of-Commercial-Portfolio-from-Loblaw-Companies-Limited/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Completes Acquisition of Commercial Portfolio from Loblaw Companies Limited</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Oct 17, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-to-Acquire-Properties-from-Loblaw-Companies-Limited-and-a-Third-Party-Vendor/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust to Acquire Properties from Loblaw Companies Limited and a Third-Party Vendor</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Oct 16, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-October-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of October, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Oct 04, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Schedules-Third-Quarter-2013-Results-Release-and-Conference-Call/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Schedules Third Quarter 2013 Results Release and Conference Call</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Sep 17, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-September-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of September, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Sep 12, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-to-Present-at-CIBC-World-Markets-Annual-Eastern-Institutional-Investor-Conference/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust to Present at CIBC World Markets Annual Eastern Institutional Investor Conference</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Sep 03, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Files-Final-Base-Shelf-Prospectus/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Files Final Base Shelf Prospectus</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Aug 19, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Files-Preliminary-Base-Shelf-Prospectus/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Files Preliminary Base Shelf Prospectus</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Aug 14, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Period-of-July-5-2013-to-August-31-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Period of July 5, 2013 to August 31, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Jul 15, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Announces-the-Exercise-in-Full-of-the-60-Million-Over-Allotment-Option-Associated-with-its-Recently-Completed-Initial-Public-Offering/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Announces the Exercise in Full of the $60 Million Over-Allotment Option Associated with its Recently Completed Initial Public Offering</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Jul 05, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Completes-400-Million-Initial-Public-Offering-and-600-Million-Senior-Unsecured-Debenture-Offering/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Completes $400 Million Initial Public Offering and $600 Million Senior Unsecured Debenture Offering</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Jun 26, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Prices-400-Million-Initial-Public-Offering-and-600-Million-Senior-Unsecured-Debentures-Offering/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Prices <span>$400 Million</span> Initial Public Offering and $600 Million Senior Unsecured Debentures Offering</a></div><div class=\"NewsItemRow\"><div class=\"Date\">May 24, 2013</div><a href=\"/news-and-events/news-releases/press-release-details/2013/Choice-Properties-Real-Estate-Investment-Trust-Files-Preliminary-Prospectuses-for-Initial-Public-Offering-of-Trust-Units-and-Senior-Unsecured-Debentures/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Files Preliminary Prospectuses for Initial Public Offering of Trust Units and Senior Unsecured Debentures</a></div></div>";
            HtmlAgilityPack.HtmlDocument doc_2013 = new HtmlAgilityPack.HtmlDocument();
            doc_2013.LoadHtml(html_2013);

            HtmlAgilityPack.HtmlNodeCollection divs = doc_2013.DocumentNode.SelectNodes("//div[contains(@class,'NewsItemRow')]");
            foreach (HtmlAgilityPack.HtmlNode divNode in divs)
            {
                HtmlAgilityPack.HtmlDocument tt = new HtmlAgilityPack.HtmlDocument();
                tt.LoadHtml(divNode.InnerHtml);

                string dt = tt.DocumentNode.SelectSingleNode("//div[@class='Date']").InnerText;
                DateTime enteredDate = dateFromString(dt);

                HtmlAgilityPack.HtmlNode hrefNode = tt.DocumentNode.SelectSingleNode("//a");
                if (hrefNode == null)
                    continue;
                string href = "http://www.choicereit.ca" + hrefNode.Attributes["href"].Value;
                string title = hrefNode.InnerText.Trim();
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
                obj.companyname = "Choice Properties REIT";
                articleData.Add(obj);
            }

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Urls of articles in Year 2014 fetching...";
            });


            string html_2014 = "<div class=\"NewsListContainer\"><div class=\"NewsItemRow\"><div class=\"Date\">Dec 16, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-December-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of December, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Nov 10, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-November-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of November, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Nov 10, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Reports-Results-for-the-Third-Quarter-Ended-September-30-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Reports Results for the Third Quarter Ended September 30, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Oct 21, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-October-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of October, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Oct 08, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Completes-Acquisition-of-Portfolio-from-Loblaw-Companies-Limited/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Completes Acquisition of Portfolio from Loblaw Companies Limited</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Oct 02, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Schedules-Third-Quarter-2014-Results-Release/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Schedules Third Quarter 2014 Results Release</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Sep 17, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-September-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of September, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Aug 19, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-August-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of August, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Jul 21, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Reports-Results-for-the-Second-Quarter-Ended-June-30-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Reports Results for the Second Quarter Ended June 30, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Jul 11, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-July-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of July, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Jun 20, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Schedules-Second-Quarter-2014-Results-Release/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Schedules Second Quarter 2014 Results Release</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Jun 17, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-June-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of June, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">May 06, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Completes-Acquisition-of-Retail-Portfolio-from-Loblaw-Companies-Limited/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Completes Acquisition of Retail Portfolio from Loblaw Companies Limited</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Apr 28, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Announces-Election-of-Trustees/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Announces Election of Trustees</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Apr 23, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-May-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of May, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Apr 23, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Reports-Results-for-the-First-Quarter-Ended-March-31-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Reports Results for the First Quarter Ended March 31, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Apr 15, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-April-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of April, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Mar 18, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Schedules-First-Quarter-2014-Results-Release-and-Annual-Meeting-of-Unitholders/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Schedules First Quarter 2014 Results Release and Annual Meeting of Unitholders</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Mar 18, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-March-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of March, 2014</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Feb 18, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-February-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of February, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Feb 18, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Reports-Solid-Results-for-the-Fourth-Quarter-Ended-December-31-2013/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Reports Solid Results for the Fourth Quarter Ended December 31, 2013</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Feb 06, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Completes-450-million-Issuance-of-Series-C-and-Series-D-Senior-Unsecured-Debentures/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Completes $450 million Issuance of Series C and Series D Senior Unsecured Debentures</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Feb 03, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-to-Issue-450-million-of-Series-C-and-Series-D-Senior-Unsecured-Debentures/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust to Issue $450 million of Series C and Series D Senior Unsecured Debentures</a></div><div class=\"NewsItemRow NewsItemRowAlt\"><div class=\"Date\">Jan 21, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Declares-Distribution-for-the-Month-of-January-2014/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Declares Distribution for the Month of January, 2014</a></div><div class=\"NewsItemRow\"><div class=\"Date\">Jan 13, 2014</div><a href=\"/news-and-events/news-releases/press-release-details/2014/Choice-Properties-Real-Estate-Investment-Trust-Schedules-Fourth-Quarter-2013-Results-Release-and-Conference-Call/default.aspx\" class=\"newsTitle\">Choice Properties Real Estate Investment Trust Schedules Fourth Quarter 2013 Results Release and Conference Call</a></div></div>";
            HtmlAgilityPack.HtmlDocument doc_2014 = new HtmlAgilityPack.HtmlDocument();
            doc_2014.LoadHtml(html_2014);

            HtmlAgilityPack.HtmlNodeCollection divs1 = doc_2014.DocumentNode.SelectNodes("//div[contains(@class,'NewsItemRow')]");
            foreach (HtmlAgilityPack.HtmlNode divNode in divs1)
            {
                HtmlAgilityPack.HtmlDocument tt = new HtmlAgilityPack.HtmlDocument();
                tt.LoadHtml(divNode.InnerHtml);

                string dt = tt.DocumentNode.SelectSingleNode("//div[@class='Date']").InnerText;
                DateTime enteredDate = dateFromString(dt);
                if (enteredDate == DateTime.MinValue)
                    continue;

                HtmlAgilityPack.HtmlNode hrefNode = tt.DocumentNode.SelectSingleNode("//a");
                if (hrefNode == null)
                    continue;

                string href = "http://www.choicereit.ca" + hrefNode.Attributes["href"].Value;
                string title = hrefNode.InnerText.Trim();
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
                obj.companyname = "Choice Properties REIT";
                articleData.Add(obj);
            }

            this.Invoke((MethodInvoker)delegate
            {
                lbl_message2.Text = "Urls of articles in Year 2015 fetching...";
            });
            string url_ = "https://api.import.io/store/data/4ceb464f-f9ed-4f66-807d-79768d29fee7/_query?_user=0a38efe3-fc06-4ef6-9d4c-52700311cc3b&_apikey=g8x7XNDblxWJXiPFqqHeBhDZOtdtRER%2Fi1exRwgF%2FLRfabARTionRaHRLN%2FkuY%2BU0b79z7eFOJ%2BkGrDOAYXCjQ%3D%3D";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url_);
            req.Method = "POST";
            string Data = "{ \"input\": { \"webpage/url\": \"http://www.choicereit.ca/news-and-events/news-releases/default.aspx\" } }";
            byte[] postBytes = Encoding.UTF8.GetBytes(Data);

            Stream requestStream = req.GetRequestStream();
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream resStream = response.GetResponseStream();

            var sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            var result_json = new JavaScriptSerializer().Deserialize<dynamic>(responseText);

            foreach (var item in result_json["results"])
            {
                string dt = item["date"];
                DateTime enteredDate = dateFromString(dt);
                if (enteredDate == DateTime.MinValue)
                    continue;

                string title = item["title"];
                string link = item["link"];

                if (title != "" && link != "")
                {
                    if (url_dict.ContainsKey(link))
                        continue;
                    url_dict.Add(link, false);
                    article obj = new article();
                    obj.dt = enteredDate;
                    obj.title = title;
                    obj.href = link;
                    obj.content = "";
                    obj.sourceId = sourceId;
                    obj.sourceTypeId = sourceTypeId;
                    obj.companyname = "Choice Properties REIT";
                    articleData.Add(obj);
                }
            }

            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Maximum = articleData.Count;
                progressBar1.Value = 0;
                lbl_message2.Text = "0 of " + articleData.Count + " articles were fetched.";
            });

            for (int i = 0; i < articleData.Count;)
            {
                if (!updateWithReEventParser(articleData[i], countryId, sourceTypeId == 2 ? sourceId : 0))
                {
                    articleData.RemoveAt(i);
                    continue;
                }

                this.Invoke((MethodInvoker)delegate
                {
                    progressBar1.Value = i + 1;
                    lbl_message2.Text = (i + 1) + " of " + articleData.Count + " articles were fetched.";
                });
                i++;
            }

            storeMySQL();
            afterStep(stepNumber);
        }
    }
}
