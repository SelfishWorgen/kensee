using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{
    public class ReEventRule : IComparable 
    {
        public string name;
        public bool haveNegative;
        public List<ReEventRuleKey> ruleKeys;
        public List<ReEventRuleKey> ruleKeysNeg;
        public bool inWholeText;
        public bool inWholeTextNeg;
        public int counter;
        public int counterNeg;
        public int priority;
        public int codeEvent;
        public List<int> sentenceNumbers;
        public string allRuleText;
        public bool hasCompany;

        public ReEventRule()
        {
            haveNegative = false;
            ruleKeys = new List<ReEventRuleKey>();
            ruleKeysNeg = new List<ReEventRuleKey>();
            allRuleText = "";
            counter = 0;
            counterNeg = 0;
            codeEvent = 0;
            sentenceNumbers = new List<int>();
            hasCompany = false;
        }

        public void clear()
        {
            foreach (ReEventRuleKey k in ruleKeys)
            {
                k.clear();
            }
            foreach (ReEventRuleKey k in ruleKeysNeg)
            {
                k.clear();
            }
            allRuleText = "";
            counter = 0;
            counterNeg = 0;
            sentenceNumbers = new List<int>();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            ReEventRule r = obj as ReEventRule;
            if (counter == r.counter)
            {
                return priority < r.priority ? 1 : -1;
            }

            return counter > r.counter ? 1 : -1;
        }

        public void countAllRuleKeys(List<Token> tokens, List<Token> stemmedTokens, ContentSentence sentence)
        {
            if (!inWholeText)
            {
                bool allKeysFound = true;
                foreach (ReEventRuleKey ruleKey in ruleKeys)
                {
                    if (!ruleKey.foundRuleKey(tokens, stemmedTokens, sentence))
                    {
                        allKeysFound = false;
                        //break;
                        // I commented this "break" because we need to have in the result log all entities of rulekey
                        // If we will have mode without writing to log, uncomment.
                    }
                }
                if (allKeysFound)
                {
                    if (sentence.number == 0 || sentence.firstSensible)
                        counter += 9;
                    counter++;
                    sentence.eventWasFound = true;
                    allRuleText += "{" + sentence.number.ToString() + "}";
                    //addToSnippet();
                    sentenceNumbers.Add(sentence.number);
                    //new staff
                    sentence.eventIds.Add(codeEvent);
                    sentence.eventNames.Add(name);
                }
                else
                {
                    //deleteSnippet();
                }
                if (ruleKeysNeg.Count == 0)
                    return;
                allKeysFound = true;
                foreach (ReEventRuleKey ruleKey in ruleKeysNeg)
                {
                    if (!ruleKey.foundRuleKey(tokens, stemmedTokens, sentence))
                    {
                        allKeysFound = false;
                        //break;
                    }
                }
                if (allKeysFound)
                {
                    allRuleText += "{Neg:" + sentence.number.ToString() + "}";
                    counterNeg++;
                }
            }
            else // in whole text, financial update
            {
                foreach (ReEventRuleKey ruleKey in ruleKeys)
                {
                    if (ruleKey.foundRuleKey(tokens, stemmedTokens, sentence))
                    {
                        if (sentence.number == 0 || sentence.firstSensible)
                            counter += 9;
                        allRuleText += "{" + sentence.number.ToString() + "}";
                        counter++;
                        if (sentence.listElementsCount > 0 && !inWholeText)
                            counter += sentence.listElementsCount;
                        //addToSnippet();
                        if (!ruleKey.isCurrency)
                        {
                            sentence.eventWasFound = true;
                            sentenceNumbers.Add(sentence.number);
                        }
                    }
                }
            }
        }

        public void fixCounters()
        {
            if (haveNegative)
            {
                counter -= counterNeg;
            }
            if (inWholeText)
            {
                foreach (ReEventRuleKey r in ruleKeys)
                {
                    if (!r.ruleKeyFound)
                    {
                        counter = 0;
                        break;
                    }
                }
                if (counter > 0)
                {
                    int c = counter / ruleKeys.Count;
                    if (c * ruleKeys.Count < counter)
                        counter = c + 1;
                    else
                        counter = c;
                }
            }
        }

    }
}