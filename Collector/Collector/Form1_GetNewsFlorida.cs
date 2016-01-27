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
using ExcelReader;
using System.Diagnostics;

namespace Collector
{
    // wget --recursive --level=1 --convert-links --no-directories --no-host-directories -P%2 --accept html,htm %1
    public partial class Form1 : Form
    {
       // List<string> scrapedUrls = null;

       // class SiteInfo
       // {
       //     public SiteInfo(string u, int n, string nm, bool d) { url = u; name = nm; stepN = n; toAddDay = d; }
       //     public string url;
       //     public string name;
       //     public int stepN;
       //     public bool toAddDay;
       // }
       // SiteInfo[] scrappingSites = new SiteInfo[] 
       // {
       //     new SiteInfo("http://realtime.blog.palmbeachpost.com", 21, "PalmBeachPost", true),
       //     new SiteInfo("http://www.floridatrend.com/tagged/49", 25, "Florida Trend", false)
       //};

       // public void GetNews_site(string baseUrl, int stepNumber, string from)
       // {
       //     if (!beforeStep(stepNumber))
       //         return;
       //     string baseFolder = reEventsParser.opt.FilesPath;
       //     string loadFolder = Path.Combine(baseFolder, "downloadedFiles");
       //     string logFile = Path.Combine(loadFolder, "log.txt");
       //     var date = start_date;
       //     int d = (int)((DateTime.Now - date).TotalDays + 1);
       //     this.Invoke((MethodInvoker)delegate
       //     {
       //         progressBar1.Maximum = d;
       //         progressBar1.Value = 0;
       //     });
       //     for (int i = 0; i < d; i++)
       //     {
       //         if (Directory.Exists(loadFolder))
       //             Directory.Delete(loadFolder, true);
       //         System.Threading.Thread.Sleep(1000);
       //         Directory.CreateDirectory(loadFolder);
       //         var url = baseUrl + date.ToString("/yyyy/MM/dd");
       //         string arg = "--recursive --no-parent --no-directories --no-host-directories --level=0 --output-file=\"" + logFile + "\" -P" + loadFolder + "  \"" + url + "\"";
       //         try
       //         {
       //             using (var proc = Process.Start(
       //             new ProcessStartInfo
       //             {
       //                 FileName = "wget.exe",
       //                 Arguments = arg,
       //                 UseShellExecute = false,
       //                 CreateNoWindow = true,
       //                 WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\"
       //             }))
       //             {
       //                 proc.WaitForExit();
       //                 proc.Close();
       //             }
       //         }
       //         catch (Exception ex)
       //         {
       //             string nn = ex.Message;
       //         }
       //         var log = File.ReadAllLines(logFile);
       //         int logLinesCount = log.Length;
       //         int fc = 0;
       //         for (int j = 0; j < logLinesCount; j++)
       //         {
       //             int n = log[j].IndexOf("--  http");
       //             if (n != -1)
       //             {
       //                 string url1 = log[j].Substring(n + 4);
       //                 for (j++; j < logLinesCount; j++)
       //                 {
       //                     if (log[j].IndexOf("--  http") != -1)
       //                     {
       //                         j--;
       //                         break;
       //                     }
       //                     n = log[j].IndexOf(" saved ");
       //                     if (n != -1)
       //                     {
       //                         fc++;
       //                         if (fc <= 2)
       //                             break;
       //                         int m = log[j].IndexOf(" - '");
       //                         if (m != -1)
       //                         {
       //                             string fileName = log[j].Substring(m + 4, n - 2 - m - 3);
       //                             HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
       //                             doc.Load(fileName);
       //                             var list = doc.DocumentNode.SelectNodes("//title");
       //                             string title = "";
       //                             if (list != null)
       //                                 title = list[0].InnerText;
       //                             list = doc.DocumentNode.SelectNodes("//meta");
       //                             if (list == null)
       //                                 continue;
       //                             if (title == "")
       //                             {
       //                                 foreach (var node in list)
       //                                 {
       //                                     string name = node.GetAttributeValue("name", "");
       //                                     if (name == "description")
       //                                     {
       //                                         title = node.GetAttributeValue("content", "");
       //                                         break;
       //                                     }
       //                                 }
       //                             }
       //                             foreach (var node in list)
       //                             {
       //                                 string name = node.GetAttributeValue("name", "");
       //                                 string content = node.GetAttributeValue("content", "");
       //                                 if (name == "keywords")
       //                                 {
       //                                     if (content.IndexOf("real estate") != -1)
       //                                     {
       //                                         HtmlNodeCollection findclasses = doc.DocumentNode.SelectNodes("//div[@class='cm-story-body entry-content ']");
       //                                         if (findclasses == null)
       //                                             findclasses = doc.DocumentNode.SelectNodes("//div[@class='cm-story-body entry-content cm-story-nophoto']");
       //                                         string text = "";
       //                                         if (findclasses != null)
       //                                         {
       //                                             text = findclasses[0].InnerText;
       //                                         }
       //                                         if (text == "")
       //                                             text = File.ReadAllText(fileName);
       //                                         if (title != "")
       //                                         {
       //                                             bool toAdd = true;
       //                                             foreach (var x in articleData)
       //                                             {
       //                                                 if (title == x.title)
       //                                                 {
       //                                                     toAdd = false;
       //                                                     break;
       //                                                 }
       //                                             }
       //                                             if (toAdd)
       //                                             {
       //                                                 article obj = new article();
       //                                                 obj.title = title;
       //                                                 obj.href = url1.Trim();
       //                                                 obj.content = text;
       //                                                 obj.dt = date;
       //                                                 obj.sourceId = stepNumber;
       //                                                 obj.sourceTypeId = 1;
       //                                                 obj.country = "";
       //                                                 obj.companyname = from;
       //                                                 obj.sector = "Real Estate";
       //                                                 if (updateWithReEventParser(obj))
       //                                                     articleData.Add(obj);
       //                                             }
       //                                         }
       //                                     }
       //                                     break;
       //                                 }
       //                             }
       //                         }
       //                         break;
       //                     }
       //                 }
       //             }
       //         }
       //         this.Invoke((MethodInvoker)delegate
       //         {
       //             progressBar1.Value = i;
       //             lbl_message2.Text = "Articles from " + date.ToString("yyyy-MM-dd") + " were fetched.";
       //         });
       //         date = date.AddDays(1);
       //     }
       //     storeMySQL();
       //     afterStep(stepNumber);
       // }

       // public void GetNews_site1(string baseUrl, int stepNumber, string from)
       // {
       //     if (!beforeStep(stepNumber))
       //         return;
       //     string baseFolder = reEventsParser.opt.FilesPath;
       //     string loadFolder = Path.Combine(baseFolder, "downloadedFiles");
       //     string logFile = Path.Combine(loadFolder, "log.txt");
       //     if (Directory.Exists(loadFolder))
       //         Directory.Delete(loadFolder, true);
       //     System.Threading.Thread.Sleep(1000);
       //     Directory.CreateDirectory(loadFolder);
       //     var url = baseUrl;
       //     string arg = "--recursive --no-parent --no-check-certificate --no-directories --no-host-directories --level=0 --output-file=\"" + logFile + "\" -P" + loadFolder + "  \"" + url + "\"";
       //     try
       //     {
       //         using (var proc = Process.Start(
       //         new ProcessStartInfo
       //         {
       //             FileName = "wget.exe",
       //             Arguments = arg,
       //             UseShellExecute = false,
       //             CreateNoWindow = true,
       //             WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\"
       //         }))
       //         {
       //             proc.WaitForExit();
       //             proc.Close();
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         string nn = ex.Message;
       //     }
       //     var log = File.ReadAllLines(logFile);
       //     int logLinesCount = log.Length;
       //     int fc = 0;
       //     this.Invoke((MethodInvoker)delegate
       //     {
       //         progressBar1.Maximum = logLinesCount;
       //         progressBar1.Value = 0;
       //     });
       //     scrapedUrls = new List<string>();
       //     for (int j = 0; j < logLinesCount; j++)
       //     {
       //         this.Invoke((MethodInvoker)delegate
       //         {
       //             progressBar1.Value = j;
       //         });
       //         int n = log[j].IndexOf("--  http");
       //         if (n != -1)
       //         {
       //             string url1 = log[j].Substring(n + 4);
       //             var uri = new Uri(url1);
       //             var baseRef = uri.GetLeftPart(UriPartial.Authority);
       //             for (j++; j < logLinesCount; j++)
       //             {
       //                 if (log[j].IndexOf("--  http") != -1)
       //                 {
       //                     j--;
       //                     break;
       //                 }
       //                 n = log[j].IndexOf(" saved ");
       //                 if (n == -1)
       //                     continue;
       //                 fc++;
       //                 //  if (fc <= 2)
       //                 //      break;
       //                 int m = log[j].IndexOf(" - '");
       //                 if (m == -1)
       //                     continue;
       //                 // file name for saved site was found
       //                 string fileName = log[j].Substring(m + 4, n - 2 - m - 3);
       //                 if (!File.Exists(fileName))
       //                     break;
       //                 string ext = Path.GetExtension(fileName);
       //                 if (ext == ".pdf" || ext == ".css" || ext == ".jpg" || ext == ".jpeg" || ext == ".js" || ext == ".txt" || ext == ".png" || ext == ".git" || ext == ".hfc")
       //                     break;
       //                 HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
       //                 doc.Load(fileName);
       //                 HtmlNodeCollection findclasses; ;
       //                 if ((findclasses = doc.DocumentNode.SelectNodes("//div[@class='cmListItem cmClearfix']")) != null)
       //                 {
       //                     addArticles(findclasses, stepNumber, from, baseRef);
       //                 }
       //                 else if ((findclasses = doc.DocumentNode.SelectNodes("//li[@class='sprite iconListBullet']")) != null)
       //                 {
       //                     addArticles(findclasses, stepNumber, from, baseRef);
       //                 }
       //                 else if ((findclasses = doc.DocumentNode.SelectNodes("//item")) != null)
       //                 {
       //                     addArticles(findclasses, stepNumber, from, baseRef);
       //                 }
       //                 else if ((findclasses = doc.DocumentNode.SelectNodes("//div[@class='blogPostWrapper item']")) != null)
       //                 {
       //                     addArticles(findclasses, stepNumber, from, baseRef);
       //                 }
       //                 else if ((findclasses = doc.DocumentNode.SelectNodes("//div[@class='blurb-list']")) != null)
       //                 {
       //                     addArticles(findclasses, stepNumber, from, baseRef);
       //                 }
       //                 else
       //                 {
       //                     string title = "";
       //                     var node1 = doc.DocumentNode.SelectSingleNode("//title");
       //                     if (node1 != null && (title = node1.InnerText) != "")
       //                     {
       //                         findclasses = doc.DocumentNode.SelectNodes("//meta");
       //                         DateTime enteredDate = DateTime.MinValue;
       //                         if (findclasses != null)
       //                         {
       //                             foreach (var nn in findclasses)
       //                             {
       //                                 if (nn.Attributes.Count > 2 && nn.Attributes[0].Name == "name" && nn.Attributes[0].Value == "date" && nn.Attributes[1].Name == "content")
       //                                 {
       //                                     enteredDate = dateFromString(nn.Attributes[1].Value);
       //                                     break;
       //                                 }
       //                             }
       //                         }
       //                         if (enteredDate == DateTime.MinValue)
       //                         {
       //                             var dt = doc.DocumentNode.SelectSingleNode("//p[@class='timestamp']");
       //                             if (dt != null)
       //                                 enteredDate = extractDateTime1(dt);
       //                         }
       //                         if (enteredDate != DateTime.MinValue && enteredDate >= start_date)
       //                         {
       //                             addArticle(url1, enteredDate, title, stepNumber, from, baseRef, "");
       //                             break;
       //                         }
       //                     }
       //                     if ((findclasses = doc.DocumentNode.SelectNodes("//a")) != null)
       //                     {
       //                         addArticles(findclasses, stepNumber, from, baseRef);
       //                     }

       //                 }
       //                 break;
       //             }
       //         }
       //     }
       //     storeMySQL();
       //     afterStep(stepNumber);
       // }

       // void extractHrefTitleDate1(HtmlAgilityPack.HtmlNode node, out string hRef, out string title, out DateTime date)
       // {
       //     hRef = "";
       //     title = "";
       //     date = DateTime.MinValue;
       //     if (node == null)
       //         return;
       //     if (node.Attributes["href"] != null)
       //     {
       //         hRef = node.Attributes["href"].Value;
       //         if (hRef != "")
       //         {
       //             title = node.InnerText.Trim();
       //             title = WebUtility.HtmlDecode(title);
       //             return;
       //         }
       //     }
       //     HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
       //     doc2.LoadHtml(node.InnerHtml);
       //     var aNodes = doc2.DocumentNode.SelectNodes("//a");
       //     if (aNodes == null)
       //     {
       //         var node1 = doc2.DocumentNode.SelectSingleNode("//title");
       //         if (node1 != null)
       //             title = node1.InnerText;
       //         node1 = doc2.DocumentNode.SelectSingleNode("//link");
       //         if (node1 != null)
       //         {
       //             hRef = node1.InnerText;
       //             if (hRef == "")
       //                 hRef = node1.NextSibling.InnerText;
       //         }
       //         node1 = doc2.DocumentNode.SelectSingleNode("//pubdate");
       //         if (node1 != null)
       //         {
       //             date = extractDateTime1(node1);
       //             if (date == DateTime.MinValue && node1.HasChildNodes)
       //                 date = extractDateTime1(node1.ChildNodes[0]);
       //         }
       //     }
       //     else
       //     {
       //         date = extractDateTime1(node);
       //         foreach (var aNode in aNodes)
       //         {
       //             if (hRef == "" && aNode.Attributes["href"] != null)
       //                 hRef = aNode.Attributes["href"].Value;
       //             if (aNode.Attributes["title"] != null)
       //                 title = aNode.Attributes["title"].Value;
       //             if (title == "")
       //                 title = aNode.InnerText.Trim();
       //         }
       //         if (date == DateTime.MinValue)
       //         {
       //             var d = doc2.DocumentNode.SelectSingleNode("//span[@class='greyText']");
       //             if (d != null)
       //                 date = extractDateTime1(d);
       //         }
       //     }
       //     title = WebUtility.HtmlDecode(title);
       // }

       // void addArticles(HtmlNodeCollection nodes, int stepNumber, string from, string baseRef)
       // {
       //     foreach (HtmlAgilityPack.HtmlNode x in nodes)
       //     {
       //         DateTime enteredDate;
       //         string href;
       //         string title;
       //         extractHrefTitleDate1(x, out href, out title, out enteredDate);
       //         if (href == "" || title == "")
       //         {
       //             title = "";
       //             continue;
       //         }
       //         if (enteredDate != DateTime.MinValue && enteredDate < start_date)
       //             continue;
       //         addArticle(href, enteredDate, title, stepNumber, from, baseRef, "");
       //     }
       // }

       // void addArticle(string href, DateTime enteredDate, string title, int sourceId, string from, string baseRef, string shortContent)
       // {
       //     if (href.StartsWith("/"))
       //         href = baseRef + href;
       //     else if (!href.StartsWith(baseRef))
       //         return;
       //     if (href.Length >= 255)
       //         return;
       //     foreach (var xx in scrapedUrls)
       //     {
       //         if (href.Trim() == xx)
       //             return;
       //     }
       //     scrapedUrls.Add(href.Trim());
       //     if (enteredDate == DateTime.MinValue)
       //     {
       //         try
       //         {
       //             HtmlAgilityPack.HtmlWeb www = new HtmlWeb();
       //             HtmlAgilityPack.HtmlDocument dcc = www.Load(href);
       //             var ndd = dcc.DocumentNode.SelectSingleNode("//div[@class='cmTimeStamp']");
       //             if (ndd != null)
       //                 enteredDate = extractDateTime1(ndd);
       //             if (enteredDate != DateTime.MinValue && enteredDate < start_date)
       //                 return;
       //         }
       //         catch (Exception ex)
       //         {
       //             reEventsParser.writeToLog("GetNewsFlorida: " + ex.Message);
       //             return;
       //         }
       //     } 
       //     if (enteredDate > DateTime.Now)
       //         return;
       //     article obj = new article();
       //     obj.title = title;
       //     obj.href = href.Trim();
       //     obj.content = "";
       //     obj.dt = enteredDate;
       //     obj.sourceId = sourceId;
       //     obj.sourceTypeId = 1;
       //     obj.country = "";
       //     obj.companyname = from;
       //     obj.sector = "Real Estate";
       //     obj.content_short = shortContent;
       //     if (updateWithReEventParser(obj))
       //         articleData.Add(obj);
       // }
    }
}
