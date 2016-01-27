using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReEvents
{
    public class HierarchicalEvent
    {
        public int sentenceNumber;
        public int eventId;
        //public List<int> countryIds;
//        public List<int> companyIds;
        public ResultLocation location;
        public List<string> companyNames;
        public string companyNamesStr;
//        public List<int> propertyIds;
        public int propertyId;
        public string areaStr;
        public string priceStr;
        public string dateStr;

        public HierarchicalEvent(int sn, int eid, ResultLocation loc, List<string> cmnms, int prid, string ars, string prs, string dts)
        {
            sentenceNumber = sn;
            eventId = eid;
            //countryIds = new List<int>();
            //countryIds.AddRange(cids);
            location = loc;
            //companyIds = new List<int>();
            //companyIds.AddRange(cmids);
            companyNames = new List<string>();
            companyNames.AddRange(cmnms);
            companyNamesStr = "";
            bool first = true;
            foreach (string companyNm in cmnms)
            {
                if (!first)
                    companyNamesStr += "; ";
                first = false;
                companyNamesStr += companyNm;
            }
            //propertyIds = new List<int>();
            //propertyIds.AddRange(prids);
            propertyId = prid;
            areaStr = ars;
            priceStr = prs;
            dateStr = dts;
        }


        public bool isEqual(HierarchicalEvent he)
        {
            if (eventId != he.eventId)
                return false;
            //if (countryIds.Count != he.countryIds.Count)
            //    return false;
            //foreach (int id in countryIds)
            //{
            //    if (!he.countryIds.Contains(id))
            //        return false;
            //}
            if (location.cityId != he.location.cityId || location.countryId != he.location.countryId)
                return false;
            //if (companyIds.Count != he.companyIds.Count)
            //    return false;
            //foreach (int id in companyIds)
            //{
            //    if (!he.companyIds.Contains(id))
            //        return false;
            //}
            if (companyNames.Count != he.companyNames.Count)
                return false;
            foreach (string nm in companyNames)
            {
                if (!he.companyNames.Contains(nm))
                    return false;
            }
            //if (propertyIds.Count != he.propertyIds.Count)
            //    return false;
            //foreach (int id in propertyIds)
            //{
            //    if (!he.propertyIds.Contains(id))
            //        return false;
            //}
            if (propertyId != he.propertyId)
                return false;
            if (areaStr != he.areaStr)
                return false;
            if (priceStr != he.priceStr)
                return false;
            if (dateStr != he.dateStr)
                return false;

            // I doubt about below code. But if we have to equal event, and one of them doesn't contain property,
            // it's better to leave event with property.
            if (he.propertyId == 0 && propertyId != 0)
                he.propertyId = propertyId;

            if (propertyId != he.propertyId)
                return false;

            return true;
        }
    }
}
