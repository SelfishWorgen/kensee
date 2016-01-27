using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerDB
{
    public class ComparisionDataCreator
    {
        public ComparisionDataCreator()
        {
            DBConnect db_obj = new DBConnect("kensee");
            db_obj.cleanTable("tbl_comparision_data");
        }

        public void fillTable()
        {
           fillTableForCountry(236);
        }

        public void fillTableForCountry(int countryId)
        {
            DBConnect db_obj = new DBConnect("kensee");
            Dictionary<string, List<ComparisionDataItem>> cmp = db_obj.readArticlesForProperty(countryId);
            foreach (var z in cmp)
            {
                string propertyName = z.Key;
                var list = z.Value;
                list.Sort(delegate(ComparisionDataItem x, ComparisionDataItem y)
                {
                    return x.date.CompareTo(y.date);
                });
                DateTime startDate = list[0].date;
                startDate = startDate.AddDays(7 - (int)startDate.DayOfWeek);
                startDate = startDate.AddHours(-startDate.Hour);
                startDate = startDate.AddMinutes(-startDate.Minute);
                startDate = startDate.AddSeconds(-startDate.Second);
                startDate = startDate.AddMilliseconds(-startDate.Millisecond);
                List<ComparisionDataItem> cmpWeeks = new List<ComparisionDataItem>();
                DateTime first = DateTime.MinValue;
                bool toAdd = false;
                int buy_sell_count = 0;
                int construction_count = 0;
                int rent_count = 0;
                foreach (var x in list)
                {
                    if (x.date >= startDate)
                    {
                        if (toAdd)
                            cmpWeeks.Add(new ComparisionDataItem { date = first, buy_sell_count = buy_sell_count, construction_count = construction_count, rent_count = rent_count });
                        toAdd = false;
                        buy_sell_count = 0;
                        construction_count = 0;
                        rent_count = 0;
                        while (x.date >= startDate)
                            startDate = startDate.AddDays(7);
                    }
                    construction_count += x.construction_count;
                    rent_count += x.rent_count;
                    buy_sell_count += x.buy_sell_count;
                    if (!toAdd)
                        first = x.date;
                    toAdd = true;
                }
                db_obj.storeComparisionDataToDB(cmpWeeks, countryId, propertyName);
            }
        }
    }

    public class ComparisionDataItem
    {
        public DateTime date;
        public int buy_sell_count;
        public int construction_count;
        public int rent_count;
    }
}
