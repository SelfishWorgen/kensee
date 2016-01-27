using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace utl
{
    public class TextContainer
    {
        public delegate bool RemoveSentence(string str);

        List<ParsingRow> rows;
        int startLine;
        int lastLine;
        int currPos;
        int currStrN;
        List<string> strArr;
        string txt;
        string prevFontStyle;
        int prevFontSize;
        bool cellsOrder;
        bool rowsOrder;
        int maxCellRowCount;
        int currentRowN;

        public bool Parsed { get { return currStrN >= strArr.Count; } }
        public string CurrentStr { get { return strArr[currStrN]; } }
        public int SentencesCount { get { return strArr.Count; } }

        public TextContainer(List<ParsingRow> rs, int sl, int ll)
        {
            rows = rs;
            startLine = sl;
            lastLine = ll;
            currStrN = 0;
            strArr = new List<string>();
            txt = "";
            prevFontStyle = "";
            prevFontSize = 0;
        }

        public void extractText()
        {
            ParsingCell prevCell = null;

            for (int i = startLine; i <= lastLine; i++)
            {
                var row = rows[i];
                if (row.Cells.Count == 0)
                {
                    if (txt != "")
                    {
                        convertTextToSentences(txt);
                        txt = "";
                    }
                    continue;
                }
                var cell = row.Cells[row.notEmptyCell];
                if (cell.textFragments.Count == 0)
                {
                    convertTextToSentences(txt);
                    txt = "";
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(txt) && prevCell != null && prevCell.top < cell.top - 20)
                {
                    convertTextToSentences(txt);
                    txt = "";
                }
                extractCell(cell);
                prevCell = cell;
            }
            convertTextToSentences(txt);
        }

        public int extractRows(RemoveSentence removeSentence)
        {
            rowsOrder = true;
            int cellCount = 0;
            for (int i = startLine; i <= lastLine; i++)
            {
                var row = rows[i];
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    cellCount++;
                    var cell = row.Cells[j];
                    if (cell.textFragments.Count == 0)
                        continue;
                    if (string.IsNullOrWhiteSpace(cell.text))
                    {
                        cell.clear();
                        continue;
                    }
                    if (removeSentence(cell.text))
                    {
                        cell.clear();
                        continue;
                    }
                    cell.textFragments[0].tableHeader = i == startLine;
                    extractCell(cell);
                    if (!string.IsNullOrWhiteSpace(txt))
                        convertTextToSentences(txt);
                    txt = "";
                }
            }
            return cellCount;
        }

        public int extractCells(RemoveSentence removeSentence)
        {
            maxCellRowCount = 0;
            cellsOrder = true;
            int cellCount = 0;
            var cell = rows[0].Cells[0];
            for (int j = 0; cell != null; j++)
            {
                cell = null;
                for (int i = startLine; i <= lastLine; i++)
                {
                    var row = rows[i];
                    if (maxCellRowCount < row.Cells.Count)
                        maxCellRowCount = row.Cells.Count;
                    if (j < row.Cells.Count)
                    {
                        cellCount++;
                        cell = row.Cells[j];
                        if (cell.textFragments.Count == 0)
                            continue;
                        if (string.IsNullOrWhiteSpace(cell.text))
                        {
                            cell.clear();
                            continue;
                        }
                        if (removeSentence(cell.text))
                        {
                            cell.clear();
                            continue;
                        }
                        string str = cell.text.Trim();
                        if (!string.IsNullOrWhiteSpace(txt) && Char.IsUpper(str[0]) || (str[0] == '(' && Char.IsUpper(str[1])))
                        {
                            convertTextToSentences(txt);
                            txt = "";
                        }
                        cell.textFragments[0].tableHeader = i == startLine;
                        extractCell(cell);
                    }
                }
                if (!string.IsNullOrWhiteSpace(txt))
                    convertTextToSentences(txt);
                txt = "";
            }
            return cellCount;
        }


        void extractCell(ParsingCell cell)
        {
            foreach (var tf in cell.textFragments)
            {
                if (tf.fontStyle == "italic" && prevFontStyle != "italic" ||
                    (prevFontSize != 0 && tf.fontSize != 0 && (tf.fontSize > prevFontSize || prevFontSize - tf.fontSize > 2) && tf.text.Length > 3))
                {
                    convertTextToSentences(txt);
                    txt = "";
                }
                if (Utils.allAreUpper(tf.text) && tf.fontSize > prevFontSize + 1)
                {
                    if (txt != "")
                    {
                        convertTextToSentences(txt);
                        txt = "";
                    }
                    convertTextToSentences(tf.text);
                    txt = "";
                    continue;
                }
                txt += tf.text;
                if (tf.text.Length > 3)
                {
                    prevFontStyle = tf.fontStyle;
                    prevFontSize = tf.fontSize;
                }
            }
            txt += " ";
        }

        ParsingCell getNextNotEmptyCell(ref int iCell, ref ParsingRow row)
        {
            ParsingCell cell;
            while (true)
            {
                if (rowsOrder)
                {
                    if (++iCell >= row.Cells.Count)
                    {
                        if (++currentRowN > lastLine)
                            return null;
                        row = rows[currentRowN];
                        iCell = 0;
                    }
                    cell = row.Cells[iCell];
                }
                else if (cellsOrder)
                {
                    if (++currentRowN > lastLine)
                    {
                        if (++iCell >= maxCellRowCount)
                            return null;
                        currentRowN = startLine;
                    }
                    row = rows[currentRowN];
                    if (iCell >= row.Cells.Count)
                        continue;
                    cell = row.Cells[iCell];
                }
                else
                {
                    if (++currentRowN > lastLine)
                        return null;
                    row = rows[currentRowN];
                    if (row.Cells.Count == 0)
                        continue;
                    cell = row.Cells[row.notEmptyCell];
                }
                if (cell.textFragments.Count != 0)
                    return cell;
            }
        }

        public ParsingCellTextFragment textFragment(int posInSentence, ref string positions, int length = 0)
        {
            if (length == 0)
                length = strArr[currStrN].Length;
            int currentPos = posInSentence + currPos;
            int iCell = 0;
            ParsingCell cell;
            ParsingRow row = null;
            if (rowsOrder)
            {
                currentRowN = startLine;
                row = rows[currentRowN];
                iCell = -1;
            }
            else if (cellsOrder)
            {
                iCell = 0;
                currentRowN = startLine - 1;
            }
            else
            {
                currentRowN = startLine - 1;
            }
            while (true)
            {
                cell = getNextNotEmptyCell(ref iCell, ref row);
                if (cell == null)
                {
                    return null;
                }
                for (int j = 0; j < cell.textFragments.Count; j++)
                {
                    var tf = cell.textFragments[j];
                    string str = tf.text;
                    if (str == null)
                        continue;
                    if (currentPos >= str.Length)
                    {
                        currentPos -= str.Length;
                        continue;
                    }
                    int restLen = length - (str.Length - currentPos);
                    int len1 = str.Length - currentPos;
                    if (restLen < 0)
                        len1 += restLen;
                    Utils.addPosition(ref positions, tf.filePosition + currentPos, len1);
         //           if (posInSentence == 0)
                    {
                        while (restLen > 0)
                        {
                            j++;
                            if (j >= cell.textFragments.Count)
                            {
                                j = 0;
                                cell = getNextNotEmptyCell(ref iCell, ref row);
                                restLen--;
                                if (restLen <= 0)
                                    break;
                                if (cell == null)
                                {
                                    cell = null;
                                    return tf;
                                }
                            }
                            var tf1 = cell.textFragments[j];
                            str = tf1.text;
                            int len = str.Length;
                            if (tf1.filePosition != 0)
                                Utils.addPosition(ref positions, tf1.filePosition, restLen > len ? len : restLen);
                            restLen -= len;
                        }
                    }
                    return tf;
                }
                currentPos--;
            }
        }

        public bool checkContext(string keysTextContext, string keysDocContext)
        {
            if (!string.IsNullOrWhiteSpace(keysTextContext) || !string.IsNullOrWhiteSpace(keysDocContext))
            {
                int fs = rows[currentRowN].Cells[rows[currentRowN].notEmptyCell].fontSize;
                for (int i = currentRowN - 1; i > 0; i--)
                {
                    if (rows[i].notEmptyCell == -1)
                        return false;
                    int fs1 = rows[i].Cells[rows[i].notEmptyCell].fontSize;
                    if (fs1 > fs + 1)
                    {
                        for (int j = i; j > 0 && rows[j].notEmptyCell != -1 && rows[j].Cells[rows[j].notEmptyCell].fontSize == fs1; j--)
                        {
                            if (!string.IsNullOrWhiteSpace(keysTextContext))
                            {
                                Regex regex = new Regex(keysTextContext, RegexOptions.IgnoreCase);
                                MatchCollection foundKeys = regex.Matches(rows[j].Cells[rows[j].notEmptyCell].text.ToLower());
                                if (foundKeys.Count != 0)
                                    return true;
                            }
                            if (!string.IsNullOrWhiteSpace(keysDocContext))
                            {
                                Regex regex = new Regex(keysDocContext, RegexOptions.IgnoreCase);
                                MatchCollection foundKeys = regex.Matches(rows[j].Cells[rows[j].notEmptyCell].text.ToLower());
                                if (foundKeys.Count != 0)
                                    return true;
                            }
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        //public PageSourcePosition sourcePosition(int currentPos)
        //{
        //    List<HtmlPosLen> positions = new List<HtmlPosLen>();
        //    var tf = textFragment(currentPos, positions);
        //    if (tf != null)
        //    {
        //        PageSourcePosition newSourcePosition = new PageSourcePosition(tf.textPosition);
        //        newSourcePosition.HtmlStreamPosition = positions[0].streamPosition;
        //        return newSourcePosition;
        //    }
        //    return new PageSourcePosition(0);
        //}

        public string position(int currentPos, string str)
        {
            string positions = "";
            var tf = textFragment(currentPos, ref positions, str.Length);
            if (tf != null)
            {
                return positions;
            }
            return "";
        }

        public void shift()
        {
            currPos += strArr[currStrN].Length;
            currStrN++;
        }

        void convertTextToSentences(string txt)
        {
            int first = 0;
            int len = txt.Length;
            for (int i = 0; i < len; i++)
            {
                if (txt[i] == '\n')
                {
                    //if (i > 0 && txt[i - 1] == '.')
                    //{
                    //    addSentence(txt, ref first, i);
                    //    continue;
                    //}
                    //if (i > 1 && txt[i - 2] == '.' && txt[i - 2] == ' ')
                    //{
                    //    addSentence(txt, ref first, i);
                    //    continue;
                    //}
                    addSentence(txt, ref first, i);
                    continue;
                }
                else if (txt[i] == ';' && i + 2 < len && Char.IsWhiteSpace(txt[i + 1]) && Char.IsWhiteSpace(txt[i + 2]))
                {
                    addSentence(txt, ref first, i);
                    continue;
                }
                else if (txt[i] == '|')
                {
                    addSentence(txt, ref first, i);
                    continue;
                }
                else if (txt[i] != '.')
                    continue;
                else if (i >= 2 && txt[i - 1] == 'r' && txt[i - 2] == 'M')
                    continue;
                else if (i >= 1 && i < len - 1 && Char.IsUpper(txt[i - 1]) && Char.IsUpper(txt[i + 1]))  //TSX: CHP.UN
                    continue;

                if (i < len - 1 && txt[i + 1] == '/')
                {
                    addSentence(txt, ref first, i + 1);
                    continue;
                }

                if (i + 1 < len && txt[i + 1] == ',')
                    continue;
                if (i + 2 < len && txt[i + 1] == ' ' && Char.IsLower(txt[i + 2]))
                    continue;

                int j = i - 1;
                for (; j >= 0; j--)
                {
                    if (!Char.IsWhiteSpace(txt[j]) && txt[j] != '\"' && txt[j] != '”' && txt[j] != '“')
                        break;
                }

                if (j >= 0 && (Char.IsDigit(txt[j]) || Char.IsLower(txt[j]) || txt[j] == ')') ||
                    (j >= 2 && Char.IsUpper(txt[j]) && Char.IsUpper(txt[j - 1]) && Char.IsUpper(txt[j - 2])))
                {
                    j = i + 1;
                    for (; j < len; j++)
                    {
                        if (!Char.IsWhiteSpace(txt[j]) && txt[j] != '\"' && txt[j] != '”' && txt[j] != '“')
                            break;
                    }
                    if (j < len && Char.IsUpper(txt[j]))
                    {
                        addSentence(txt, ref first, i);
                        continue;
                    }
                }
            }
            if (first < len)
            {
                addSentence(txt, ref first, len-1);
            }
        }

        void addSentence(string txt, ref int first, int i)
        {
            string s = txt.Substring(first, i - first + 1).Replace("\n", " ").Trim();
            if (s.Length <= 2)
            {
                first = i + 1;
                return;
            }
            while (true) // remove ";" at the beginning and at the end AND exclude ". .. ... jjj"
            {
                if (s[0] != '.' && s[0] != ';' && s[s.Length - 1] != ';')
                    break;
                if (s[0] == '.' || s[0] == ';')
                    s = s.Substring(1);
                if (s.Length <= 2)
                    break;
                if (s[s.Length - 1] == ';')
                    s = s.Substring(0, s.Length - 2);
                s = s.Trim();
                if (s.Length <= 2)
                    break;
            }
            if (s.Length > 2)
            {
                strArr.Add(s);
            }
            first = i + 1;
        }
    }
}
