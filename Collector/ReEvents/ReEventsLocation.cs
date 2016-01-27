using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using utl;

namespace ReEvents
{

    public class ReEventsLocType
    {
        public int locTypeId;
        public string locTypeName;
    }

    public class ReEventsCountry
    {
        public int countryId;
        public int regionId;
        // only for debugging - to show in debug app country name
        public string countryName;
    }
    
    public class ReEventsCountryName
    {
        public int countryId;
        public string countryName;
    }

    public class ReEventsCity
    {
        public int cityId;
        public int countryId;
    }

    public class ReEventsCityName
    {
        public int cityId;
        public string cityName;
    }

    public class CountryDictionary
    {
        public Dictionary<string, CountryDictionary> keys;
        public int countryId;
        public int regionId;
        //for debug
        public string countryName; 
    }

    public class CityDictionary
    {
        public Dictionary<string, CityDictionary> keys;
        public int cityId;
        public int countryId;
        public int regionId;
        //for debug
        public string cityName; 
    }

    public class ReEventsLocation
    {
        CountryDictionary countryDictionary;
        CityDictionary cityDictionary;
        TokenParser tokenParser;

        int regionLocationType;
        int countryLocationType;
        int cityLocationType;

        // result
        List<int> cityIds;
        List<int> countryIds;
        List<int> regionIds;

        // for debug 
        List<string> cityNames;  
        List<string> countryNames;

        public ReEventsLocation(TokenParser tp)
        {
            countryDictionary = new CountryDictionary { keys = new Dictionary<string, CountryDictionary>(), countryId = 0 };
            cityDictionary = new CityDictionary { keys = new Dictionary<string, CityDictionary>(),  countryId = 0, cityId = 0};
            tokenParser = tp;

            //result
            cityIds = new List<int>();
            countryIds = new List<int>();
            regionIds = new List<int>();

            //debug
            cityNames = new List<string>();
            countryNames = new List<string>();
        }

        public void clear()
        {
            //result
            cityIds.Clear();
            countryIds.Clear();
            regionIds.Clear();

            //debug
            countryNames.Clear();
            cityNames.Clear();
        }

        public void Init(List<ReEventsCountryName> countryList, List<ReEventsCityName> cityList,
            List<ReEventsCountry> countryRegionList, List<ReEventsCity> cityCountryList,
            List<ReEventsLocType> locTypeList)
        {
            Dictionary<int, int> cIds = new Dictionary<int, int>();
            Dictionary<int, int> cnIds = new Dictionary<int, int>();
            foreach (ReEventsCity re in cityCountryList)
            {
                cIds.Add(re.cityId, re.countryId);
            }
            foreach (ReEventsCountry r in countryRegionList)
            {
                cnIds.Add(r.countryId, r.regionId);
            }
            foreach (ReEventsCountryName nm in countryList)
            {
                int regionId = cnIds[nm.countryId];
                addCountryToDictionary(nm.countryName, nm.countryId, regionId);
            }
            Dictionary<int, string> cntr = new Dictionary<int, string>();
            foreach (ReEventsCityName cn in cityList)
            {
                int countryId = cIds[cn.cityId];
                int regionId = cnIds[countryId];
                addCityToDictionary(cn.cityName, cn.cityId, countryId, regionId);
            }
            foreach (ReEventsLocType l in locTypeList)
            {
                string s = l.locTypeName.ToLower();
                if (s == "region")
                    regionLocationType = l.locTypeId;
                else if (s == "country")
                    countryLocationType = l.locTypeId;
                else if (s == "city")
                    cityLocationType = l.locTypeId;
            }
        }

        void addCountryToDictionary(string countryName, int countryId, int regionId)
        {
            List<Token> tokens = tokenParser.GetTokensForText(countryName);
            int k = tokens.Count;
            CountryDictionary x = countryDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new CountryDictionary { keys = new Dictionary<string, CountryDictionary>(), countryId = 0, countryName = "", };
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.countryId = countryId;
                    x.countryName = countryName;
                    x.regionId = regionId;
                }
            }
        }

        void addCityToDictionary(string cityName, int cityId, int countryId, int regionId)
        {
            List<Token> tokens = tokenParser.GetTokensForText(cityName);
            int k = tokens.Count;
            CityDictionary x = cityDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new CityDictionary { keys = new Dictionary<string, CityDictionary>(), countryId = 0, cityName = "" , cityId = 0,};
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.countryId = countryId;
                    x.cityName = cityName;
                    x.cityId = cityId;
                    x.regionId = regionId;                   
                }
            }
        }

        private void extractCountryFromTokenList(List<Token> tokens, string snt)
        {
            for (int i = 0; i < tokens.Count; )
            {
                string str = tokens[i].token.ToLower();
                string cn = "";
                int regionId = 0;
                int countryId = 0;
                int s = findInCountryDictionary(tokens, i, countryDictionary, out cn, out regionId);
                if (s > 0) 
                {
                    if (!countryIds.Contains(s))
                        countryIds.Add(s);
                    if (!regionIds.Contains(regionId))
                        regionIds.Add(regionId);
                    countryNames.Add(cn);
                    i += tokenParser.GetTokensForText(cn).Count;
                }
                else
                {
                    s = findInCityDictionary(tokens, i, cityDictionary, out cn, out countryId, out regionId);
                    if (s > 0) 
                    {
                        if (!cityIds.Contains(s))
                            cityIds.Add(s);
                        if (!countryIds.Contains(countryId))
                            countryIds.Add(countryId);
                        if (!regionIds.Contains(regionId))
                            regionIds.Add(regionId);
                        cityNames.Add(cn);
                        i += tokenParser.GetTokensForText(cn).Count;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        private int findInCountryDictionary(List<Token> tokens, int index, CountryDictionary dict, 
            out string countryName, out int regionId)
        {
            countryName = "";
            regionId = 0;
            string str = tokens[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            CountryDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                countryName = dict.countryName;
                regionId = y.regionId;
                return dict.countryId;
            }
            if (index == tokens.Count - 1)
            {
                countryName = y.countryName;
                regionId = y.regionId;
                return y.countryId;
            }
            int s = findInCountryDictionary(tokens, index + 1, y, out countryName, out regionId); 

            if (s != 0) 
                return s;
            countryName = y.countryName;
            regionId = y.regionId;
            return y.countryId;
        }

        private int findInCityDictionary(List<Token> tokens, int index, CityDictionary dict,
            out string cityName, out int countryId, out int regionId)
        {
            cityName = "";
            regionId = 0;
            countryId = 0;
            string str = tokens[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            CityDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                cityName = dict.cityName;
                countryId = y.countryId;
                regionId = y.regionId;
                return dict.cityId;
            }
            if (index == tokens.Count - 1)
            {
                cityName = y.cityName;
                countryId = y.countryId;
                regionId = y.regionId;
                return y.cityId;
            }
            int s = findInCityDictionary(tokens, index + 1, y, out cityName, out countryId, out regionId);

            if (s != 0) 
                return s;
            cityName = y.cityName;
            countryId = y.countryId;
            regionId = y.regionId;
            return y.cityId;
        }
    }

}
