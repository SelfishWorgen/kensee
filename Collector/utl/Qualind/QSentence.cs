using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class QSentence
    {
        public double score_sent;
        public double field;
        public string descr;
        public List<string> foundKeys;
        public Sentence sentence;
        public int qsentenceN;
        public bool addInTextWindow;

        public QSentence(Sentence snt, int qsntN)
        {
            qsentenceN = qsntN;
            sentence = snt;
            field = 0;
            score_sent = 0;
            descr = "No";
        }
    }
}
