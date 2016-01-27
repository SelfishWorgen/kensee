using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Net;
using utl;

namespace ReEvents
{
    public partial class ReEventsParser
    {

        private void getContentSentencies(string url)
        {
            url = url.Trim();
            string ext = Path.GetExtension(url).ToLower();
            try
            {
                if (ext == ".pdf")
                {
                    nBoilerPipeUsed = false;
                    downLoadPdf(url);
                }
                else
                {
                    nBoilerPipeUsed = true;
                    getTextByUrl(url);
                }
            }
            catch (Exception ex)
            {
                sentences.Clear();
                if (ex is IOException)
                    throw;
            }
        }

        private void downLoadPdf(string url)
        {
            int n = 0;
            try
            {
                WebClient webClient = new WebClient();
                byte[] downloadedBytes = webClient.DownloadData(url);
                n = 0;
                while (downloadedBytes.Length == 0)
                {
                    if (++n > 60)
                        break;
                    System.Threading.Thread.Sleep(2000);
                    downloadedBytes = webClient.DownloadData(url);
                }
                if (n > 60)
                {
                    //"Cannot download file"
                    n++;
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(downloadedBytes, 0, 20);
                Stream file1 = File.Open(downLoadFileName, FileMode.OpenOrCreate);
                file1.Write(downloadedBytes, 0, downloadedBytes.Length);
                file1.Close();
            }
            catch (Exception ex)
            {
                n++;
                //"Exception while download"
                return;
            }
            HtmlParser.currentFileName = downLoadFileName;
            HtmlParser.currentPageNumber = 0;
            var htmlParser = new HtmlParser(parseRows, null, false);
            htmlParser.parsePdf();
        }


        public void parseRows(List<ParsingRow> rows, Utils.PerformProgress performProgress)
        {
            if (rows.Count == 0)
                return;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                row.notEmptyCell = -2;
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    if (!string.IsNullOrWhiteSpace(row.Cells[j].text))
                    {
                        if (row.notEmptyCell >= 0)
                        {
                            row.notEmptyCell = -1;
                            break;
                        }
                        row.notEmptyCell = j;
                    }
                }
                if (row.notEmptyCell == -2)
                    row.notEmptyCell = 0;
            }

            int firstTableN = 0;
            int lastTableN = -1;
            while (true) // to look all tables
            {
                int lastTextN = lastTableN + 1;
                for (; lastTextN < rows.Count; lastTextN++)
                {
                    if (rows[lastTextN].Cells.Count > 1)
                    {
                        lastTextN--;
                        break;
                    }
                }
                if (lastTextN == rows.Count)
                    lastTextN = rows.Count - 1;
                if (lastTableN + 1 <= lastTextN) // parse text
                {
                    int i = firstTableN;
                    while (true)
                    {
                        int lastN = i + 1000;
                        if (lastN > lastTextN)
                            lastN = lastTextN;
                        if (performProgress != null)
                            performProgress((i * 100) / rows.Count, "", 0);
                        parseText(rows, i, lastN);
                        if (lastN == lastTextN)
                            break;
                        i = lastN + 1;
                    }
                }
                firstTableN = lastTextN + 1;
                if (firstTableN >= rows.Count)
                    break;
                lastTableN = firstTableN + 1;
                int cellCount = 0;
                while (true)
                {
                    for (; lastTableN < rows.Count && rows[lastTableN].Cells.Count > 1; lastTableN++)
                    {
                        if (rows[lastTableN].Cells.Count > cellCount)
                            cellCount = rows[lastTableN].Cells.Count;
                    }
                    if (lastTableN + 1 < rows.Count && rows[lastTableN + 1].Cells.Count > 1)
                    {
                        lastTableN += 1;
                        continue;
                    }
                    if (lastTableN + 2 < rows.Count && rows[lastTableN + 2].Cells.Count > 1)
                    {
                        lastTableN += 2;
                        continue;
                    }
                    break;
                }
                if (lastTableN >= rows.Count)
                    lastTableN = rows.Count - 1;
                else if (rows[lastTableN].Cells.Count <= 1)
                    lastTableN--;
                if (performProgress != null)
                    performProgress((firstTableN * 100) / rows.Count, "", 0);
                parseTable(rows, firstTableN, lastTableN);
                firstTableN = lastTableN + 1;
                if (firstTableN >= rows.Count)
                    break;
            }
        }

        void parseTable(List<ParsingRow> rows, int firstN, int lastN)
        {
            //TODO later
        }

        void parseText(List<ParsingRow> rows, int firstN, int lastN)
        {
            TextContainer txtCont = new TextContainer(rows, firstN, lastN);
            txtCont.extractText();
            extractSentences(txtCont, false);
        }

        void extractSentences(TextContainer txtCont, bool fromTable)
        {
            for (; !txtCont.Parsed; txtCont.shift())
            {
                var s1 = txtCont.CurrentStr.Trim();
                if (string.IsNullOrWhiteSpace(s1))
                    continue;
                if (s1.IndexOf(' ') == -1)
                    continue;
                string positions = "";
                ParsingCellTextFragment tf = txtCont.textFragment(0, ref positions);
                Sentence.SentenceContext sentenceContext = Sentence.SentenceContext.SC_text;
                if (fromTable)
                    sentenceContext = tf.tableHeader ? Sentence.SentenceContext.SC_tableHeader : Sentence.SentenceContext.SC_cell;
                else if (tf.title) // tf.header || (tf.fontSize >= ParsingCellTextFragment.fontSizeMin * 2 && ParsingCellTextFragment.fontSizeMin > 0))
                    sentenceContext = Sentence.SentenceContext.SC_title;
                Sentence snt = new Sentence(s1, 0, sentenceContext, tf.fontSize, positions, 0);
                sentences.Add(snt);
            }
        }

        private void getTextByUrl(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url.Trim());
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                HtmlConverter.ConvertToTextAndTitle(responseFromServer, 1, out stringContent, out stringTitle);
                //stringContent = HtmlConverter.ConvertToText(responseFromServer, 0);
                stringContent = WebUtility.HtmlDecode(stringContent);
                stringContent = stringContent.Replace("\n", " ");
                sentenceStrings.InsertRange(0, stringContent.Split(new char[] { '.', '?' }));
                if (!string.IsNullOrEmpty(stringTitle))
                {
                    stringTitle = WebUtility.HtmlDecode(stringTitle);
                    sentenceStrings.Insert(0, stringTitle);
                }
            }
            catch
            {
                nBoilerPipeUsed = false;
                sentenceStrings.Clear();
            }
            if (!nBoilerPipeUsed)
            {
                try
                {
                    var htmlReader = new HtmlReader();
                    var rows = htmlReader.readUrl(url);
                    parseRows(rows, null);
                }
                catch
                {
                    sentences.Clear();
                }
            }
        }
    }


}

