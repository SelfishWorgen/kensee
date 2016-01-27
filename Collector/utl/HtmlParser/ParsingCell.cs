using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class ParsingCell
    {
        public List<ParsingCellTextFragment> textFragments;
        public int left;
        public int top;
        public string txt;
        public bool changedCell;

        public ParsingCell(int lft = 0, int _top = 0)
        {
            txt = null;
            left = lft;
            top = _top;
            textFragments = new List<ParsingCellTextFragment>();
        }

        public int getTextLength()
        {
            int len = 0;
            foreach (var x in textFragments)
                len += x.text.Length * x.fontSize * 2 / 5;
            return len;
        }

        public void clear()
        {
            txt = "";
            textFragments.Clear();
        }

        public ParsingCell copy()
        {
            var c = new ParsingCell(left, top);
            foreach (var x in textFragments)
                c.textFragments.Add(x);
            return c;
        }

        public void addText(ParsingCellTextFragment tf)
        {
            txt = null;
            textFragments.Add(tf);
        }

        public void addTexts(List<ParsingCellTextFragment> tfs)
        {
            txt = null;
            textFragments.AddRange(tfs);
        }

        public int fontSize
        {
            get
            {
                if (textFragments.Count == 0)
                    return 8;
                int fs = 0;
                for (int i = 0; i < textFragments.Count; i++)
                {
                    if (textFragments[i].fontSize > fs)
                        fs = textFragments[i].fontSize;
                }
                return fs;
            }
        }

        public string text
        {
            get
            {
                if (textFragments.Count == 0)
                    return "";
                if (txt != null)
                    return txt;
                txt = "";
                for (int i = 0; i < textFragments.Count; i++)
                {
                    if (i != 0 && textFragments[i].fontSize == 2 && !textFragments[i-1].text.EndsWith(" "))
                        txt += " ";
                    txt += textFragments[i].text;
                }
                txt = txt.Trim();
                return txt;
            }
        }

        public bool bold
        {
            get
            {
                if (textFragments.Count == 0)
                    return false;
                for (int i = 0; i < textFragments.Count; i++)
                {
                    if (!string.IsNullOrEmpty(textFragments[i].text) && !textFragments[i].bold)
                        return false;
                }
                return true;
            }
        }

        public void insertTextBefore(string t)
        {
            txt = null;
            textFragments.Insert(0, new ParsingCellTextFragment(t, 0));
        }

        public void addText(string t, int fp = 0)
        {
            txt = null;
            textFragments.Add(new ParsingCellTextFragment(t, fp));
        }

        public int getFilePosition(int shiftInTxt)
        {
            if (textFragments.Count == 0)
                return 0;
            int curLen = 0;
            int prevLen = 0;
            foreach (var fr in textFragments)
            {
                curLen += fr.text.Length;
                if (shiftInTxt >= prevLen && shiftInTxt < curLen && fr != null)
                {
                    return fr.filePosition + shiftInTxt - prevLen;
                }
                prevLen = curLen;
            }
            return textFragments[0].filePosition;
        }

        public string position(string str)
        {
            str = str.ToLower();
            string pos = "";
            foreach (var fr in textFragments)
            {
                if (pos != "")
                    pos += ","; 
                string str1 = fr.text.ToLower();
                if (str1.StartsWith(str))
                {
                    pos += fr.filePosition.ToString() + " " + str.Length.ToString();
                    break;
                }
                if (str.StartsWith(str1))
                {
                    pos += fr.filePosition.ToString() + " " + str1.Length.ToString();
                    str = str.Substring(str1.Length);
                }
                else
                {
                    pos += fr.filePosition.ToString() + " 1";
                    break;
                }
            }
            return pos;
        }
    }
}
