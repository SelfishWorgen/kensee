using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace utl
{
    public class TokenParser
    {
        public delegate void CallBackSrv([MarshalAs(UnmanagedType.LPStr)] string token, int start_offset, int end_offset,
            [MarshalAs(UnmanagedType.LPStr)] string tag1,
            [MarshalAs(UnmanagedType.LPStr)] string tag2,
            [MarshalAs(UnmanagedType.LPStr)] string tag3);

        [DllImport("Parser.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ParseText")]
        private static extern void ParseText(char[] text, char[] data, CallBackSrv callBackFun);
        [DllImport("Parser.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "InitializeParser")]
        private static extern void InitializeParser(char[] data);
        [DllImport("Parser.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "FinalyzeParser")]
        private static extern void FinalyzeParser();

        CallBackSrv callBackFun = null;
        List<Token> tokenList;
        string dataPath;

        public TokenParser(string path)
        {
            dataPath = path + "\\parser\\";
            callBackFun = callBack;
            InitializeParser((dataPath + "\0").ToCharArray());
        }

        public List<Token> GetTokensForText(string text)
        {
            try
            {
                tokenList = new List<Token>();
                ParseText((text + "\0").ToCharArray(), (dataPath + "\0").ToCharArray(), callBackFun);
                return tokenList;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public void callBack(string token, int tokenStart, int end_offset, string tag1, string tag2, string tag3)
        {
            try
            {
                tokenList.Add(new Token(token, tag1, tag2, tag3, tokenStart, end_offset));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }

    public class Token
    {
        public string token;
        public string tag1;
        public string tag2;
        public string tag3;
        public int tokenStart;
        public int end_offset;

        public Token(string tk, string tg1, string tg2, string tg3, int s, int e)
        {
            token = tk;
            tag1 = tg1;
            tag2 = tg2;
            tag3 = tg3;
            tokenStart = s;
            end_offset = e;
        }
    }
}
