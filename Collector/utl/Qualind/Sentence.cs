using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class Sentence
    {
        public enum SentenceContext
        {
            SC_title,
            SC_text,
            SC_tableHeader,
            SC_cell,
        };

        public int sentenceN;
        public string text;
        public string fileName;
        public int page;
        public int tableNumber;
        public string positions;
        public int firstLine;
        public int lastLine;
        public int cellN;              // #cen in row
        public int fontSize;
        public SentenceContext context;
        public int year;

        // temporary, during building
        public double tmp_score_sent;
        public List<string> tmp_foundKeys;

        public Sentence(int n, string txt, int fl, int ll)
        {
            sentenceN = n;
            text = txt;
            firstLine = fl;
            lastLine = ll;
        }

        public Sentence(string txt, int clN, SentenceContext cntx, int fs, string poss, int yr)
        {
            year = yr;
            positions = poss;
            fileName = HtmlParser.currentFileName;
            page = HtmlParser.currentPageNumber;
            tableNumber = HtmlParser.currentTableNumber;
            sentenceN = 0;
            text = txt;
            firstLine = 0;
            lastLine = 0;
            cellN = clN;
            context = cntx;
            fontSize = fs;
        }

        public string type()
        {
            switch (context)
            {
                case SentenceContext.SC_title:
                    return "Header";
                case SentenceContext.SC_text:
                    return "Text";
                case SentenceContext.SC_tableHeader:
                    return "Table Header";
                case SentenceContext.SC_cell:
                    return "Cell " + cellN.ToString();
                default:
                    return "";
            }
        }
   }
}
