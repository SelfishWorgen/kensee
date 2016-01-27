using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LanguageIdentification;

namespace Collector
{
    public partial class Form1 : Form
    {
        LanguageIdentifier li = null;

        void initLangIdentifier()
        {
            li = LanguageIdentifier.New(Path.Combine(reEventsParser.opt.FilesPath, "26langprofiles-twit-char-3-3-tlc-all.bin.gz"), "Likely", -1);
        }

        bool isSiteEnglish(article at)
        {
            var lang = li.Identify(at.content);
            if (lang != "en")
                return false;
            return true;
        }

        bool isTextEnglish(string txt)
        {
            var lang = li.Identify(txt);
            if (lang != "en")
                return false;
            return true;
        }
    }
}
