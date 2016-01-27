using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class ParsingCellTextFragment
    {
        static public int fontSizeSum;
        static public int fontSizeMin;
        static public int fontSizeCount;

        public int filePosition; // line number for Excel
        public int cellNumber;
        public string text;
        public string fontStyle;
        public int fontSize;
        public uint color;
        public bool bold;
        public bool header;
        public bool tableHeader;
        public bool title; 

        public ParsingCellTextFragment(string txt, int fp, int fs = 0, string f_style = null, bool bld = false)
        {
            text = txt;
            fontStyle = f_style;
            filePosition = fp;
            if (fs == 0)
                fs = 4; 
            fontSize = fs;
            bold = bld;
            if (fs != 0)
            {
                fontSizeSum += fs * txt.Length;
                fontSizeCount += txt.Length;
                if (fontSizeMin > fs)
                    fontSizeMin = fs;
            }
        }
    }
}
