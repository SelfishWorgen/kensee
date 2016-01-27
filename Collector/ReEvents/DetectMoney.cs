using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;
using System.Text.RegularExpressions;

namespace ReEvents
{
    public class CurrencyNames
    {
        public Dictionary<string, CurrencyNames> keys;
        public int cnt;
    }


    public class DetectMoney
    {
        List<string> factors = new List<string> { "million", "millions", "billion", "billions", 
            "thousand", "thousands", "hundred", "hundreds", "сrore", "sekm"};

        static string regExprSign = @"us\$|usd|\$|¥|£|€|₪|₽|₴|₩|₹|usd|jpy|gbp|eur|ils|rur|uah|krw|thb|inr|nis|dkk|brl|aud|sek";
        CurrencyNames currencyNames;

        public DetectMoney()
        {
            currencyNames = new CurrencyNames { keys = new Dictionary<string, CurrencyNames>(), cnt = 0 };
            addCurrencyNames("dollar");
            addCurrencyNames("dollars");
            addCurrencyNames("yen");
            addCurrencyNames("yens");
            addCurrencyNames("pound");
            addCurrencyNames("pounds");
            addCurrencyNames("euro");
            addCurrencyNames("new israeli shekel");
            addCurrencyNames("new israeli shekels");
            addCurrencyNames("peso");
            addCurrencyNames("pesos");
            addCurrencyNames("crone");
            addCurrencyNames("crones");
            addCurrencyNames("real");
            addCurrencyNames("reals");
            addCurrencyNames("yuan");
            addCurrencyNames("yuans");
            addCurrencyNames("kroner");
            addCurrencyNames("kroners");
            addCurrencyNames("rouble");
            addCurrencyNames("roubles");
            addCurrencyNames("rand");
            addCurrencyNames("rands");
            addCurrencyNames("won");
            addCurrencyNames("wons");
            addCurrencyNames("krona");
            addCurrencyNames("kronas");
            addCurrencyNames("franc");
            addCurrencyNames("francs");
            addCurrencyNames("rupee");
            addCurrencyNames("rupees");
        }

        public void extractPriceFromTokenList(List<Token> tokens, ContentSentence sentence)
        {
            int start = -1;
            int end = -1;
            Regex regexSign = new Regex(regExprSign, RegexOptions.IgnoreCase);
            MatchCollection foundKeys = regexSign.Matches(sentence.txt);
            if (foundKeys.Count > 0)
            {
                // try to find currency sign and detect num value:
                // the problem is tokens, that ignores symbols as €, £ etc.
                Match f = foundKeys[0]; //temporary only one price
                int signPos = f.Index;
                int signLength = f.Length;
                for (int i = 0; i < tokens.Count; i++)
                {
                    Token t = tokens[i];
                    int ts = t.tokenStart;
                    int te = t.end_offset;
                    //int pte = i == 0 ? sentence.txt.Length : tokens[i - 1].end_offset;
                    if (ts == signPos && te - ts == signLength) // case "€ 54.10", case "54.10 ₽"
                    {
                        if (checkTokenSign(tokens, i + 1, signPos, signLength, out start, out end)) // case "€ 54.10"
                            break;
                        if (checkTokenSign(tokens, i - 1, signPos, signLength, out start, out end)) // case "54.10 ₽"
                            break;
                    }
                    else if ((ts == signPos || ts - 1 == signPos) && te - ts > 1) // case "€54.10"
                    {
                        if (checkTokenSign(tokens, i, signPos, signLength, out start, out end))
                            break;
                    }
                    else if ((te == signPos || te + 1 == signPos) && te - ts > 1) // case "54.10¥", it is unlikely
                    {
                        if (checkTokenSign(tokens, i, signPos, signLength, out start, out end))
                            break;
                    }
                    else if (ts > signPos)
                        break;
                }
            }
            else // sign character was not found
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    Token t = tokens[i];
                    int ts = t.tokenStart;
                    int te = t.end_offset;
                    int cnt = 0;
                    if (checkToken(tokens, i, currencyNames, out cnt))
                    {
                        // usd millions 54 - ?
                        if (isFactor(tokens, i + 1) && isValue(tokens, i + 2))
                        {
                            start = tokens[i].tokenStart;
                            end = tokens[i + 2].end_offset;
                            break;
                        }
                        // million usd 54
                        if (isFactor(tokens, i - 1) && isValue(tokens, i + 2))
                        {
                            start = tokens[i - 1].tokenStart;
                            end = tokens[i + 2].end_offset;
                            break;
                        }
                        // 54 millions dollars
                        if (isFactor(tokens, i - 1) && isValue(tokens, i - 2))
                        {
                            start = tokens[i - 2].tokenStart;
                            end = tokens[i].end_offset;
                            break;
                        }
                        // usd 54 million
                        if (isValue(tokens, i + 1))
                        {
                            start = tokens[i].tokenStart;
                            if (isFactor(tokens, i + 2))
                                end = tokens[i + 2].end_offset;
                            else
                                end = tokens[i + 1].end_offset;
                            break;
                        }
                        if (isValue(tokens, i - 1))
                        {
                            if (isFactor(tokens, i - 2))
                            {
                                end = tokens[i].end_offset;
                                start = tokens[i - 2].tokenStart;
                            }
                            else if (isFactor(tokens, i + 1))
                            {
                                end = tokens[i + 1].end_offset;
                                start = tokens[i - 1].tokenStart;
                            }
                            else
                            {
                                start = tokens[i - 1].tokenStart;
                                end = tokens[i].end_offset;
                            }
                            break;
                        }
                    }
                }
            }
            if (start > -1 && end > -1)
            {
                sentence.priceStr = sentence.txt.Substring(start, end - start);
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
            if (tokens[index].token.Length == 0)
                return false;
            if (Char.IsDigit(tokens[index].token[0]))
                return true;
            return false;
        }

        bool checkTokenSign(List<Token> tokens, int index, int signPos, int signLenght, out int start, out int end)
        {
            start = -1;
            end = -1;
            if (index >= tokens.Count)
                return false;
            if (index < 0)
                return false;
            if (Char.IsDigit(tokens[index].token[0]) ||
                (tokens[index].tokenStart == signPos && tokens[index].end_offset - tokens[index].tokenStart < signLenght 
                    && Char.IsDigit(tokens[index].token[signLenght])))
            {
                // case "€ 54.10, 54.20 USD"
                start = signPos > tokens[index].tokenStart ? tokens[index].tokenStart : signPos;
                end = tokens[index].end_offset;
                if (index < tokens.Count - 1 && factors.Contains(tokens[index + 1].token.ToLower()))
                    end = tokens[index + 1].end_offset;
                return true;
            }
            return false;
        }

        void addCurrencyNames(string str)
        {
            string[] arrs = str.Split(new char[] { ' ' });
            int k = arrs.Length;

            CurrencyNames x = currencyNames;
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
                    var z = new CurrencyNames { keys = new Dictionary<string, CurrencyNames>(), cnt = 0 };
                    x.keys.Add(s, z);
                    x = z;
                }
                if (i == k - 1)
                {
                    x.cnt = k;
                }

            }
        }

        bool checkToken(List<Token> tokens, int index, CurrencyNames x, out int cnt)
        {
            cnt = 0;
            string s = tokens[index].token;
            if (!x.keys.ContainsKey(s))
                return false;
            CurrencyNames y = x.keys[s];
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
