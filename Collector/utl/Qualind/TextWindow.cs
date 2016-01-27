using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class TextWindow
    {
        public List<QSentence> qsentencies;
        public bool bold;
        public string textLines;
        public string positions;
        public bool deleted;

        public TextWindow(QSentence snt)
        {
            qsentencies = new List<QSentence>();
            qsentencies.Add(snt);
            bold = snt.sentence.context == Sentence.SentenceContext.SC_title;
        }

        public void addSentence(QSentence snt)
        {
            qsentencies.Add(snt);
            bold |= snt.sentence.context == Sentence.SentenceContext.SC_title;
        }

        public double getSemScore()
        {
            double semScore = 0;
            foreach (var s in qsentencies)
                semScore += s.field;
            semScore = semScore / qsentencies.Count;
            if (semScore > 0.95)
                semScore = 0.95;
            return semScore;
        }

        public string getTextLines()
        {
            if (textLines != null)
                return textLines;
            return qsentencies[0].sentence.firstLine.ToString("00000") + "-" + qsentencies[qsentencies.Count - 1].sentence.lastLine.ToString("00000");
        }

        public string getFileName()
        {
            return qsentencies[0].sentence.fileName;
        }

        public int getPageNumber()
        {
            return qsentencies[0].sentence.page;
        }

        public string getFoundKeys()
        {
            Dictionary<string, int> keyDic = new Dictionary<string, int>();
            string str = "\"";
            foreach (var s in qsentencies)
            {
                foreach (var k in s.foundKeys)
                {
                    if (keyDic.ContainsKey(k))
                        continue;
                    keyDic.Add(k, 0);
                    if (str != "\"")
                        str += ", ";
                    str += k;
                }
            }
            str += "\"";
            return str;
        }

        public string getText()
        {
            string str = "";
            foreach (var s in qsentencies)
                str += s.sentence.text + " ";
            return str;
        }

        public void mark()
        {
            for (int i = 1; i < qsentencies.Count - 1; i++)
                qsentencies[i].descr = "Inside";
            qsentencies[qsentencies.Count - 1].descr = "End";
            qsentencies[0].descr = "Start";
        }

        public string getPositions()
        {
            if (positions != null)
                return positions;
            List<KeyValuePair<int, int>> posList = new List<KeyValuePair<int, int>>();
            foreach (var p in qsentencies)
            {
                if (p.sentence.positions != null && p.sentence.fileName == qsentencies[0].sentence.fileName && p.sentence.page == qsentencies[0].sentence.page)
                {
                    var a = p.sentence.positions.Split(new char[] { ',' });
                    foreach (var b in a)
                    {
                        var c = b.Split(new char[] { ' ' });
                        if (c.Length == 2)
                            posList.Add(new KeyValuePair<int, int>(Convert.ToInt32(c[0]), Convert.ToInt32(c[1])));
                    }
                }
            }
            posList.Sort(delegate(KeyValuePair<int, int> x1, KeyValuePair<int, int> y1)
            {
                return x1.Key.CompareTo(y1.Key);
            });
            string pos = "";
            foreach (var z in posList)
            {
                Utils.addPosition(ref pos, z.Key, z.Value);
            }
            return pos;
        }
    }
}
