using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{

    public class AreaMeasureUnits
    {
        public Dictionary<string, AreaMeasureUnits> keys;
        public int cnt;
    }

    public class DetectArea
    {
        AreaMeasureUnits areaMeasureUnits;
        List<string> factors = new List<string> { "million", "millions", "thousand", "thousands", "hundred", "hundreds"};
        
        public DetectArea()
        {
            areaMeasureUnits = new AreaMeasureUnits { keys = new Dictionary<string, AreaMeasureUnits>(), cnt = 0 };
            addAreaMeasureUnit("sq.ft.");
            addAreaMeasureUnit("sq. ft.");
            addAreaMeasureUnit("square foot");
            addAreaMeasureUnit("square feet");
            addAreaMeasureUnit("acres");
            addAreaMeasureUnit("square meters");
        }

        void addAreaMeasureUnit(string str)
        {
            string[] arrs = str.Split(new char[] { ' ' });
            int k = arrs.Length;
            AreaMeasureUnits x = areaMeasureUnits;
            for (int i = 0; i < k; i++)
            {
                string s1 = arrs[i];
                string s = s1.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new AreaMeasureUnits { keys = new Dictionary<string, AreaMeasureUnits>(), cnt = 0 };
                    x.keys.Add(s, z);
                    x = z;
                }
                if (i == k - 1)
                {
                    x.cnt = k;
                }
            }
        }

        public void extractAreaFromTokenList(List<Token> tokens, ContentSentence sentence)
        {
            int start = -1;
            int end = -1;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];
                int ts = t.tokenStart;
                int te = t.end_offset;
                int cnt = 0;

                if (checkToken(tokens, i, areaMeasureUnits, out cnt))
                {
                    if (isFactor(tokens, i - 1) && isValue(tokens, i - 2))
                    {
                        start = tokens[i - 2].tokenStart;
                        end = tokens[i + cnt - 1].end_offset;
                        break;
                    }
                    if (isValue(tokens, i - 1))
                    {
                        start = tokens[i - 1].tokenStart;
                        end = tokens[i + cnt - 1].end_offset;
                        break;
                    }
                }
            }
            if (start > -1 && end > -1)
            {
                sentence.areaStr = sentence.txt.Substring(start, end - start);
            }
        }

        bool isFactor(List<Token> tokens, int index)
        {
            if (index >= tokens.Count)
                return false;
            if (index < 0)
                return false;
            if (factors.Contains(tokens[index].token))
                return true;
            return false;
        }

        bool isValue(List<Token> tokens, int index)
        {
            if (index >= tokens.Count)
                return false;
            if (index < 0)
                return false;
            if (Char.IsDigit(tokens[index].token[0]))
                return true;
            return false;
        }

        bool checkToken(List<Token> tokens, int index, AreaMeasureUnits x, out int cnt)
        {
            cnt = 0;
            string s = tokens[index].token;
            if (!x.keys.ContainsKey(s))
                return false;
            AreaMeasureUnits y = x.keys[s];
            if (y == null || y.keys == null)
            {
                cnt = x.cnt;
                return true;
            }
            if (y.keys.Count == 0)
            {
                cnt = y.cnt;
                return true;
            }
            if (index == tokens.Count - 1)
            {
                if (y.cnt > 0)
                {
                    cnt = y.cnt;
                    return true;
                }
                return false;
            }
            if (checkToken(tokens, index + 1, y, out cnt))
                return true;
            if (y.cnt > 0)
            {
                cnt = y.cnt;
                return true;
            }
            return false;
        }
    }
}
