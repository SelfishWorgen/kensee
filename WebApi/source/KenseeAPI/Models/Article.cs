using System;
using System.Collections.Generic;

namespace KenseeAPI.Models
{
    public class Article
    {
        public int article_id { get; set; }
        public string url { get; set; }
        public string date { get; set; }
        public string source { get; set; }
        public string country { get; set; }
        public string topic { get; set; }
        public string sentiment { get; set; }
        public string company { get; set; }
        public string property { get; set; }
        public string city { get; set; }
        public string title { get; set; }
        public string snippet { get; set; }
    }

    public class Article1
    {
        public string url { get; set; }
        public string date { get; set; }
        public string content { get; set; }
        public string title { get; set; }
    }

    public class Highlights
    {
        public string topic { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string url { get; set; }
    }
    
    public class Market_sentiment
    {
        public string date;
        public float sentiment;
        public float newResidentialConstruction;
        public float nationalHomePriceIndex;
    }
    
    public class Comparison_data
    {
        public string property;
        public string date;
        public int buy_sell;
        public int construction;
        public int rent;
    }
    
    public class Macro
    {
        public string date;
        public float UnemploymentRate;
        public float Inflation;
        public float HousePriceIndexChange;
        public float RateForLodging;
        public float RateForOffice;
        public float RateForCommercial;
        public float RateForHealthcare;
        public float RateForLeasure;
        public float RateForNonResidential;
        public float RateForResidential;
    }

    public class GeneralInformation
    {
        public string Name;
        public string Value;
    }

    public class DashboardData
    {
        public List<GeneralInformation> generalInformation { get; set; }
        public List<Highlights> highlights { get; set; }
        public List<Market_sentiment> market_sentiment { get; set; }
        public List<Comparison_data> comparison_data { get; set; }
        public List<Macro> macro { get; set; }
        public DashboardData() { generalInformation = new List<GeneralInformation>();  highlights = new List<Highlights>(); market_sentiment = new List<Market_sentiment>(); comparison_data = new List<Comparison_data>(); macro = new List<Macro>(); }
    }
}
