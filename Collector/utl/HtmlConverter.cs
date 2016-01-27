using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBoilerpipe.Extractors;

namespace utl
{
    public class HtmlConverter
    {
        public static string ConvertToText(string txt, int extractorId)
        {
            string titleText = ExtractName(txt, "<title>", "</title>");
            String text = "";
            switch (extractorId)
            {
                default:
                case 0:
                    text = ArticleExtractor.INSTANCE.GetText(txt);
                    break;
                case 1:
                    text = ArticleSentencesExtractor.INSTANCE.GetText(txt);
                    break;
                case 2:
                    text = CanolaExtractor.INSTANCE.GetText(txt);
                    break;
                case 3:
                    text = DefaultExtractor.INSTANCE.GetText(txt);
                    break;
                case 4:
                    text = LargestContentExtractor.INSTANCE.GetText(txt);
                    break;
                case 5:
                    text = KeepEverythingExtractor.INSTANCE.GetText(txt);
                    break;
                case 6:
                    text = NumWordsRulesExtractor.INSTANCE.GetText(txt);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(titleText))
                text = titleText + "\n" + text;
            return text;
        }

        public static void ConvertToTextAndTitle(string txt, int extractorId, out string content, out string title)
        {
            title = ExtractName(txt, "<title>", "</title>");
            content = "";
            switch (extractorId)
            {
                default:
                case 0:
                    content = ArticleExtractor.INSTANCE.GetText(txt);
                    break;
                case 1:
                    content = ArticleSentencesExtractor.INSTANCE.GetText(txt);
                    break;
                case 2:
                    content = CanolaExtractor.INSTANCE.GetText(txt);
                    break;
                case 3:
                    content = DefaultExtractor.INSTANCE.GetText(txt);
                    break;
                case 4:
                    content = LargestContentExtractor.INSTANCE.GetText(txt);
                    break;
                case 5:
                    content = KeepEverythingExtractor.INSTANCE.GetText(txt);
                    break;
                case 6:
                    content = NumWordsRulesExtractor.INSTANCE.GetText(txt);
                    break;
            }
            if (content.StartsWith("<iframe "))
            {
                int n = content.LastIndexOf("</iframe>");
                if (n != -1)
                    content = content.Substring(n + 9);
            }
        }

        
        public static string ExtractName(string text, string start, string end)
        {
            int n1 = text.IndexOf(start, 0);
            if (n1 == -1)
                return null;
            n1 += start.Length;
            int n2 = text.IndexOf(end, n1);
            if (n2 == -1)
                return null; 
            return text.Substring(n1, n2 - n1);
        }

        public static string ExtractNameWithRemoving(ref string text, string start, string end)
        {
            int n1 = text.IndexOf(start, 0);
            if (n1 == -1)
                return null;
            n1 += start.Length;
            int n2 = text.IndexOf(end, n1);
            if (n2 == -1)
                return null;
            string result = text.Substring(n1, n2 - n1);
            text = text.Substring(n2 + end.Length);
            return result;
        }
    }
}
