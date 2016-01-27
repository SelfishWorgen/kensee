using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Data;
using System.Net;
using System.Globalization;
using System.IO;

namespace utl
{
    public class TraverseState
    {
        public HtmlNode node;
        public int fontSize;
        public uint color;
        public string fontStyle;
        public bool bold;
        public int childId;
    }

    public class HtmlReader
    {
        List<ParsingRow> rows;
        ParsingRow currentRow = null;
        ParsingCell currentCell = null;
        public bool absoluteAllCells;

        public HtmlReader()
        {
            absoluteAllCells = true;
            rows = new List<ParsingRow>();
        }

        public List<ParsingRow> read(string fileName)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.DetectEncodingAndLoad(fileName);
            ParsingCellTextFragment.fontSizeCount = 0;
            ParsingCellTextFragment.fontSizeSum = 0;
            ParsingCellTextFragment.fontSizeMin = 0;
            traverse(htmlDocument.DocumentNode);
            checkCurrentCell();
            return rows;
        }

        public List<ParsingRow> readHtml(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.DetectEncodingHtml(html);
            ParsingCellTextFragment.fontSizeCount = 0;
            ParsingCellTextFragment.fontSizeSum = 0;
            ParsingCellTextFragment.fontSizeMin = 0;
            traverse(htmlDocument.DocumentNode);
            checkCurrentCell();
            return rows;
        }

        public List<ParsingRow> readUrl(string url)
        {
            try
            {
                ParsingCellTextFragment.fontSizeCount = 0;
                ParsingCellTextFragment.fontSizeSum = 0;
                ParsingCellTextFragment.fontSizeMin = 0;
                url = url.Trim();
                var web = new HtmlWeb();
                if (url.Length > 5 && url.Substring(0, 4).CompareTo("http") != 0)
                    url = "http://" + url;
                HtmlDocument htmlDocument = web.Load(url);
                traverse(htmlDocument.DocumentNode);
                checkCurrentCell();
                return rows;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
                try
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    string html = loadHTML(url);
                    doc.LoadHtml(html);
                    traverse(doc.DocumentNode);
                    checkCurrentCell();
                    return rows;
                }
                catch (Exception ex1)
                {
                    return null;
                }
            }
        }

        string loadHTML(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            string html = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(),
                         Encoding.ASCII))
                {
                    html = reader.ReadToEnd();
                }
            }
            return html;
        }

         private void traverse(HtmlNode node)
        {
            int fontSize = 8;
            string fontStyle = "";
            bool bold = false;
            uint color = 0;
            int childId = 0;
            List<TraverseState> traverseState = new List<TraverseState>();
            while (true)
            {
                while (true)
                {
                    if (childId != 0)
                    {
                        if (childId < node.ChildNodes.Count)
                        {
                            TraverseState st = new TraverseState();
                            st.node = node;
                            st.fontSize = fontSize;
                            st.color = color;
                            st.fontStyle = fontStyle;
                            st.bold = bold;
                            st.childId = childId + 1;
                            traverseState.Insert(0, st);
                            node = node.ChildNodes[childId];
                            childId = 0;
                            continue;
                        }
                        if (node.Name == "ul")  //list
                        {
                            if (currentCell != null)
                                currentCell.addText(". ");
                        }
                        else if (node.Name == "ll") //list element
                        {
                            if (currentCell != null)
                                currentCell.addText(", ");
                        }
                    }
                    else
                    {
                        tryToReadAttributes(node, ref fontSize, ref fontStyle, ref bold, ref color);
                        if (node.Name == "script")
                            break;
                        if (node.Name == "select")
                            break;
                        if (node.Name == "style")
                            break;
                        if (node.Name == "a")
                            break;
                        if (node.Name == "comment")
                            break;
                        if (node.Name == "meta")
                            break;
                        if (node.Name == "br")
                        {
                            if (currentCell != null)
                                currentCell.addText(" ");
                            break;
                        }
                        if (node.Name == "div")
                        {
                            if (currentCell != null && !string.IsNullOrWhiteSpace(currentCell.text) && !currentCell.text.EndsWith("."))
                                currentCell.addText(";");
                            checkCurrentCell();
                            if (parseDivNode(node))
                                break;
                        }
                        if (node.Name == "tr")
                        {
                            checkCurrentCell();
                            currentRow = new ParsingRow();
                        }
                        else if (node.Name == "td" || node.Name == "th")
                        {
                            checkCurrentCell();
                            currentCell = null;
                        }
                        if (node.Attributes.Count != 0)
                        {
                            for (int i = 0; i < node.Attributes.Count; i++)
                            {
                                if (node.Attributes[i].Name != "content")
                                    continue;

                                var attr = node.Attributes[i];
                                currentCell.addText(attr.Value + " ");
                            }
                        }
                        if (node.HasChildNodes)
                        {
                            if (node.ChildNodes.Count > 0)
                            {
                                TraverseState st = new TraverseState();
                                st.node = node;
                                st.fontSize = fontSize;
                                st.color = color;
                                st.fontStyle = fontStyle;
                                st.bold = bold;
                                st.childId = 1;
                                traverseState.Insert(0, st);
                                node = node.ChildNodes[0];
                                continue;
                            }
                        }
                        else if (node.NodeType == HtmlNodeType.Text)
                        {
                            if (currentCell == null)
                                currentCell = new ParsingCell();
                            var tf = new ParsingCellTextFragment(replaceChars(node.InnerText), node.StreamPosition, fontSize, fontStyle);
                            tf.color = color;
                            if (traverseState[0].node.Name.ToLower() == "title")
                                tf.title = true;
                            currentCell.addText(tf);
                        }
                    }
                    if (currentRow != null && (node.Name == "td" || node.Name == "th"))
                    {
                        if (currentCell == null)
                            currentCell = new ParsingCell();
                        int callspan = getIntAttribute(node, "colspan");
                        if (callspan > 1)
                        {
                            if (callspan == 3)
                            {
                                currentRow.addEmptyCell();
                                currentRow.AddCell(currentCell);
                                currentRow.addEmptyCell();
                            }
                            else
                            {
                                for (int i = 1; i < callspan; i++)
                                    currentRow.addEmptyCell();
                                currentRow.AddCell(currentCell);
                            }
                        }
                        else
                        {
                            currentRow.AddCell(currentCell);
                        }
                        currentCell = null;
                    }
                    else if (node.Name == "tr")
                    {
                        if (currentRow != null && currentRow.Count != 0)
                        {
                            absoluteAllCells = false;
                            rows.Add(currentRow);
                        }
                        currentRow = null;
                    }
                    break;
                }
                if (traverseState.Count == 0)
                    return;
                node = traverseState[0].node;
                fontSize = traverseState[0].fontSize;
                fontStyle = traverseState[0].fontStyle;
                bold = traverseState[0].bold;
                color = traverseState[0].color;
                childId = traverseState[0].childId;
                traverseState.RemoveAt(0);
            }
        }

        bool parseDivNode(HtmlNode node)
        {
            int left = 0;
            int top = 0;
            int fontSize = 0;
            uint color = 0;
            string fontStyle = "";
            bool bold = false;
            bool absolutePosWas = false;
            if (!tryToReadAttributes(node, ref left, ref top, ref fontSize, ref absolutePosWas, ref fontStyle, ref bold, ref color))
            {
                absolutePosWas = false;
                return false;
            }
            if (!absolutePosWas)
            {
                absolutePosWas = false;
                return false;
            }
            if (left == 0 || top == 0)
                absoluteAllCells = false;
            ParsingRow row = new ParsingRow();
            ParsingCell cell = new ParsingCell(left, top);
            row.AddCell(cell);
            rows.Add(row);

            foreach (HtmlNode childNode in node.ChildNodes)
            {
                addTextFragment(cell, childNode, left, top, fontSize, fontStyle, bold, color);
            }
            currentCell = null;
            return true;
        }

        void addTextFragment(ParsingCell cell, HtmlNode node, int left, int top, int fontSize, string fontStyle, bool bold, uint color)
        {
            bool absolutePosWas = false;
            if (node.NodeType == HtmlNodeType.Text)
            {
                string cleanInnerText = replaceChars(node.InnerHtml);
                //if (cleanInnerText.Length == 0)         // exclude such text nodes as "\t"
                //    return;
                tryToReadAttributes(node, ref left, ref top, ref fontSize, ref absolutePosWas, ref fontStyle, ref bold, ref color);
                ParsingCellTextFragment fr = new ParsingCellTextFragment(cleanInnerText, node.StreamPosition, fontSize, "", bold);
                fr.color = color;
                cell.textFragments.Add(fr); 
                return;
            }
            tryToReadAttributes(node, ref left, ref top, ref fontSize, ref absolutePosWas, ref fontStyle, ref bold, ref color);
            foreach (HtmlNode childNode in node.ChildNodes)
            {
                addTextFragment(cell, childNode, left, top, fontSize, fontStyle, bold, color);
            }
        }

        bool tryToReadAttributes(HtmlNode node, ref int left, ref int top, ref int fontSize, ref bool absolutePosWas, ref string fontStyle, ref bool bold, ref uint fontColor)
        {
            if (node.Attributes.Count == 0)
                return false;
            string str = "";
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == "style")
                {
                    str = node.Attributes[i].Value;
                    break;
                }
            }
            if (str.Length == 0)
                return false;
            int n1, n2;
            absolutePosWas = false;
            if (str.IndexOf("position:absolute") != -1)
            {
                n1 = str.IndexOf("left:");
                if (n1 != -1)
                {
                    n2 = str.IndexOf("px;", n1);
                    if (n2 != -1)
                    {
                        absolutePosWas = Int32.TryParse(str.Substring(n1 + 5, n2 - n1 - 5), out left);
                    }
                }
                n1 = str.IndexOf("top:");
                if (n1 != -1)
                {
                    n2 = str.IndexOf("px;", n1);
                    if (n2 != -1)
                    {
                        absolutePosWas |= Int32.TryParse(str.Substring(n1 + 4, n2 - n1 - 4), out top);
                    }
                }
            }
            n1 = str.IndexOf("font-size:");
            if (n1 != -1)
            {
                n2 = str.IndexOf("px;", n1);
                if (n2 != -1)
                {
                    Int32.TryParse(str.Substring(n1 + 10, n2 - n1 - 10), out fontSize);
                }
            }
            n1 = str.IndexOf("color:#");
            if (n1 != -1)
            {
                n2 = str.IndexOf(";", n1);
                if (n2 != -1)
                    uint.TryParse(str.Substring(n1 + 7, n2 - n1 - 7), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out fontColor);
            }
            n1 = str.IndexOf("font-style:");
            if (n1 != -1)
            {
                n2 = str.IndexOf(";", n1);
                if (n2 != -1)
                {
                   fontStyle = str.Substring(n1 + 11, n2 - n1 - 11).Trim();
                }
            }
            n1 = str.IndexOf("ffont-weight:bold;");
            if (n1 != -1)
            {
                bold = true; ;
            }
            return true;
        }

        void tryToReadAttributes(HtmlNode node, ref int fontSize, ref string fontStyle, ref bool bold, ref uint fontColor)
        {
            if (node.Attributes.Count == 0)
                return;
            string str = "";
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == "style")
                {
                    str = node.Attributes[i].Value;
                    break;
                }
            }
            if (str.Length == 0)
                return;
            int n1, n2;
            n1 = str.IndexOf("font-size:");
            if (n1 != -1)
            {
                n2 = str.IndexOf("px;", n1);
                if (n2 != -1)
                    Int32.TryParse(str.Substring(n1 + 10, n2 - n1 - 10), out fontSize);
            }
            n1 = str.IndexOf("color:#");
            if (n1 != -1)
            {
                n2 = str.IndexOf(";", n1);
                if (n2 != -1)
                    uint.TryParse(str.Substring(n1 + 7, n2 - n1 - 7), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out fontColor);
            }
            n1 = str.IndexOf("ffont-weight:bold;");
            if (n1 != -1)
            {
                bold = true; ;
            }
            n1 = str.IndexOf("font-style:");
            if (n1 != -1)
            {
                n2 = str.IndexOf(";", n1);
                if (n2 != -1)
                    fontStyle = str.Substring(n1 + 11, n2 - n1 - 11).Trim();
            }
        }

        int getPosition(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
                return node.StreamPosition;
            foreach (var x in node.ChildNodes)
            {
                int p = getPosition(x);
                if (p != 1)
                    return p;
            }
            return 1;
        }

        int getIntAttribute(HtmlNode node, string name)
        {
            int value = 0;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == name)
                {
                    Int32.TryParse(node.Attributes[i].Value, out value);
                    break;
                }
            }
            return value;
        }

        void checkCurrentCell()
        {
            if (currentCell != null && !string.IsNullOrWhiteSpace(currentCell.text))
            {
                absoluteAllCells = false;
                ParsingRow row = new ParsingRow();
                row.AddCell(currentCell);
                rows.Add(row);
            }
            currentCell = null;
        }

        string replaceChars(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return " ";
            str = WebUtility.HtmlDecode(str);
            str = str.Replace("ﬁ", "fi").Replace("¬", "").Replace("\x07", "").Replace("\x08", "").Replace("\x04", "").Replace("\x05", "").Replace("\x15", "").Replace("\x1D", "").Replace("\x1E", "").Replace("\x1e", "").Replace("\x1b", "").Replace("\x03", "").Replace("\t", " ").Replace("&pound;", "£").Replace("&yen", "¥").Replace("\r", "").Replace("\xb3", "3").Replace("•", " ");
            str = str.Replace("�", " "); 
            if (string.IsNullOrWhiteSpace(str))
                return " ";
            return str;
        }
    }
}
