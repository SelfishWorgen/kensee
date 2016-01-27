using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerDB
{
    public class MarketSentimentCreator
    {
        public MarketSentimentCreator()
        {
            DBConnect db_obj = new DBConnect("kensee");
            db_obj.cleanTable("tbl_market_sentiment");
        }

        public void fillTable(Dictionary<int, InfoForSentimentGraph> newResidentialConstructionDict, Dictionary<int, InfoForSentimentGraph> nationalHomePriceIndex)
        {
            fillTableForCountry(236, newResidentialConstructionDict[236], nationalHomePriceIndex[236]);
        }

        public void fillTableForCountry(int countryId, InfoForSentimentGraph newResidentialConstruction, InfoForSentimentGraph nationalHomePriceIndex)
        {
            DBConnect db_obj = new DBConnect("kensee");
            List<MarketSentimentItem> msl = db_obj.readSentiments(countryId);

            DateTime dtNow = DateTime.Now;
            DateTime dt1 = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
            
            DateTime startDateM3 = dt1.AddMonths(-3);
            startDateM3 = startDateM3.AddDays((8 - (int)startDateM3.DayOfWeek) % 7);

            DateTime startDateM6 = dt1.AddMonths(-6);
            startDateM6 = startDateM6.AddDays((8 - (int)startDateM6.DayOfWeek) % 7);

            DateTime startDateY1 = dt1.AddYears(-1);
            startDateY1 = startDateY1.AddDays((8 - (int)startDateY1.DayOfWeek) % 7);
            
            if (dtNow.Day != 1)
                dt1 = new DateTime(dtNow.Year, dtNow.Month, 1).AddMonths(1);
            DateTime startDateY3 = dt1.AddYears(-3);
            DateTime startDateY12 = dt1.AddYears(-12);
            if (startDateY12.Year <= 2003)
                startDateY12 = new DateTime(2004, 1, 1);

            fillTableForPeriod(db_obj, countryId, msl, startDateM3, true, 3, newResidentialConstruction, nationalHomePriceIndex);
            fillTableForPeriod(db_obj, countryId, msl, startDateM6, true, 6, newResidentialConstruction, nationalHomePriceIndex);
            fillTableForPeriod(db_obj, countryId, msl, startDateY1, true, 12, newResidentialConstruction, nationalHomePriceIndex);
            fillTableForPeriod(db_obj, countryId, msl, startDateY3, false, 36, newResidentialConstruction, nationalHomePriceIndex);
            fillTableForPeriod(db_obj, countryId, msl, startDateY12, false, 144, newResidentialConstruction, nationalHomePriceIndex);
        }

        void fillTableForPeriod(DBConnect db_obj, int countryId, List<MarketSentimentItem> msl, DateTime startDate, bool weeks, int period,
            InfoForSentimentGraph newResidentialConstruction, InfoForSentimentGraph nationalHomePriceIndex)
        {
            List<MarketSentimentItem> mslResult = new List<MarketSentimentItem>();
            DateTime currDate = startDate;
            DateTime prevDate = startDate;
            float sentiment = 0;
            int count = 0;
            DateTime first = DateTime.MinValue;
            bool toAdd = false;
            foreach (var x in msl)
            {
                if (x.date <= startDate)
                    continue;
                while (x.date >= currDate)
                {
                    if (toAdd || prevDate != currDate)
                    {
                        if (!toAdd)
                            first = prevDate;
                        float newResidentialConstructionVl = 0;
                        if (weeks)
                        {
                            DateTime dt = first;
                            if (dt.Year - newResidentialConstruction.firstYear >= 0)
                            {
                                for (int i = 1; i <= 7; i++)
                                {
                                    var a = newResidentialConstruction.values[dt.Year - newResidentialConstruction.firstYear];
                                    float vl = 0;
                                    if (dt.Month - 1 < a.Count)
                                        vl = a[dt.Month - 1];
                                    else
                                        vl = a[a.Count - 1];
                                    int days = DateTime.DaysInMonth(dt.Year, dt.Month);
                                    newResidentialConstructionVl += vl / days;
                                    dt = dt.AddDays(1);
                                }
                            }
                        }
                        else if (first.Year - newResidentialConstruction.firstYear >= 0)
                        {
                            var a = newResidentialConstruction.values[first.Year - newResidentialConstruction.firstYear];
                            if (first.Month - 1 < a.Count)
                                newResidentialConstructionVl = a[first.Month - 1];
                            else
                                newResidentialConstructionVl = a[a.Count - 1];
                        }
                        float nationalHomePriceIndexVl = 0;
                        if (weeks)
                        {
                            DateTime dt = first;
                            if (dt.Year - nationalHomePriceIndex.firstYear >= 0)
                            {
                                for (int i = 1; i <= 7; i++)
                                {
                                    var a = nationalHomePriceIndex.values[dt.Year - nationalHomePriceIndex.firstYear];
                                    float vl = 0;
                                    if (dt.Month - 1 < a.Count)
                                        vl = a[dt.Month - 1];
                                    else
                                        vl = a[a.Count - 1];
                                    int days = DateTime.DaysInMonth(dt.Year, dt.Month);
                                    nationalHomePriceIndexVl += vl / days;
                                    dt = dt.AddDays(1);
                                }
                            }
                        }
                        else if (first.Year - nationalHomePriceIndex.firstYear >= 0)
                        {
                            var a = nationalHomePriceIndex.values[first.Year - nationalHomePriceIndex.firstYear];
                            if (first.Month - 1 < a.Count)
                                nationalHomePriceIndexVl = a[first.Month - 1];
                            else
                                nationalHomePriceIndexVl = a[a.Count - 1];
                        }
                        mslResult.Add(new MarketSentimentItem(first, count == 0 ? 0 : sentiment / count, newResidentialConstructionVl, nationalHomePriceIndexVl));
                        if (newResidentialConstructionVl == 0)
                            newResidentialConstructionVl = 0;
                        toAdd = false;
                        sentiment = 0;
                        count = 0;
                    }
                    prevDate = currDate;
                    if (weeks)
                        currDate = currDate.AddDays(7);
                    else
                        currDate = currDate.AddMonths(1);
                }
                sentiment += x.sentiment_value;
                count++;
                if (!toAdd)
                    first = x.date;
                toAdd = true;
            }
            if (mslResult.Count != 0)
            {
                mslResult[0].sentimemt_index_period_smoothed = mslResult[0].sentiment_value;
                if (mslResult.Count > 1)
                    mslResult[1].sentimemt_index_period_smoothed = (mslResult[1].sentiment_value + mslResult[0].sentiment_value) / 2;
                for (int i = 2; i < mslResult.Count; i++)
                    mslResult[i].sentimemt_index_period_smoothed = (mslResult[i].sentiment_value + mslResult[i - 1].sentiment_value + mslResult[i - 2].sentiment_value) / 3;
                db_obj.storeSentimentsToDB(mslResult, countryId, period);
            }
        }
    }

    public class MarketSentimentItem
    {
        public MarketSentimentItem(DateTime dt, float vl, float x, float x1) { date = dt; sentiment_value = vl; newResidentialConstruction = x; nationalHomePriceIndex = x1; }
        public DateTime date;
        public float sentiment_value;
        public float sentimemt_index_period_smoothed;
        public float newResidentialConstruction;
        public float nationalHomePriceIndex;
    }
}
