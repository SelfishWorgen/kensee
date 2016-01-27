using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using utl;

namespace ReEvents
{

    public class ReEventsLocType
    {
        public int locTypeId;
        public string locTypeName;
    }

    public class ReEventsRegionName
    {
        public int regionId;
        public string regionName;
    }

    public class ReEventsCountry
    {
        public int countryId;
        public int regionId;
        public string uniqCountryName;
    }
    
    public class ReEventsCountryName
    {
        public int countryId;
        public string countryName;
    }

    public class ReEventsCity
    {
        public int cityId;
        public int stateId;
        public int countryId;
        public string uniqCityName;
    }

    public class ReEventsStateName
    {
        public int stateId;
        public string stateName;
    }

    public class ReEventsState
    {
        public int stateId;
        public int countryId;
        public string uniqStateName;
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
        public string countryName; 
    }

    public class CityDictionary
    {
        public Dictionary<string, CityDictionary> keys;
        public int cityId;
        public int stateId;
        public int countryId;
        public int regionId;
        public string uniqCityName;
        public string cityName; 
    }

    public class StateDictionary
    {
        public Dictionary<string, StateDictionary> keys;
        public int stateId;
        public int countryId;
        public int regionId;
        public string uniqStateName;
        public string stateName;
    }

    public class ResultLocation
    {
        public int cityId;
        public int stateId;
        public int countryId;
        public int regionId;
        public ContentSentence sent;
        public string keyword;
        public string cityName;
        public string stateName;
        public string countryName;
        public string regionName;
    }

    public class DetectLocations
    {
        CountryDictionary countryDictionary;
        StateDictionary stateDictionary;
        CityDictionary cityDictionary;
        TokenParser tokenParser;
        Dictionary<int, string> uniqCountryNames;
        Dictionary<int, string> uniqStateNames;
        Dictionary<int, string> uniqCityNames;
        Dictionary<int, int> countryRegionIds;  //country, region
        Dictionary<int, string> regionNames;
        List<string> streetSuffixList;

        int regionLocationType;
        int countryLocationType;
        int cityLocationType;
        int stateLocationType;

        public int predefinedCountryId;
        
        // result
        List<int> cityIds;
        List<int> countryIds;
        List<int> regionIds;
        List<int> stateIds;

        string locationKeywords;
        string locations;
        List<ResultLocation> results;

        public DetectLocations()
        {
            countryDictionary = new CountryDictionary { keys = new Dictionary<string, CountryDictionary>(), countryId = 0 };
            stateDictionary = new StateDictionary { keys = new Dictionary<string, StateDictionary>(), countryId = 0, stateId = 0 };
            cityDictionary = new CityDictionary { keys = new Dictionary<string, CityDictionary>(), countryId = 0, cityId = 0, stateId = 0 };
            uniqCountryNames = new Dictionary<int, string>();
            uniqStateNames = new Dictionary<int, string>();
            uniqCityNames = new Dictionary<int, string>();
            regionNames = new Dictionary<int, string>();
            streetSuffixList = new List<string>();

            //result
            cityIds = new List<int>();
            countryIds = new List<int>();
            regionIds = new List<int>();
            stateIds = new List<int>();
            locationKeywords = "";
            locations = "";
            results = new List<ResultLocation>();
            predefinedCountryId = 0;
        }

        public void clear()
        {
            //result
            results = new List<ResultLocation>();
            cityIds = new List<int>();
            stateIds = new List<int>();
            countryIds = new List<int>();
            regionIds = new List<int>();
            locationKeywords = "";
            locations = "";
            predefinedCountryId = 0;
        }

        public void Init(TokenParser tp,
            List<ReEventsCountryName> countryList, List<ReEventsStateName> stateList, List<ReEventsCityName> cityList,
            List<ReEventsCountry> countryRegionList, List<ReEventsState> stateCountryList, List<ReEventsCity> cityCountryList,
            List<ReEventsLocType> locTypeList, List<ReEventsRegionName> regionNameList, string streetSuffixFile)
        {
            tokenParser = tp;

            regionLocationType = -1;
            countryLocationType = -1;
            cityLocationType = -1;
            stateLocationType = -1;
            
            Dictionary<int, int> cityCountryIds = new Dictionary<int, int>(); //city, country
            Dictionary<int, int> cityStateIds = new Dictionary<int, int>(); //city, state
            Dictionary<int, int> stateCountryIds = new Dictionary<int, int>(); //state, country
            countryRegionIds = new Dictionary<int, int>(); //country, region
            foreach (ReEventsRegionName rn in regionNameList)
            {
                regionNames.Add(rn.regionId, rn.regionName);
            }
            foreach (ReEventsCity re in cityCountryList)
            {
                cityCountryIds.Add(re.cityId, re.countryId);
                if (re.stateId > 0)
                    cityStateIds.Add(re.cityId, re.stateId);
                uniqCityNames.Add(re.cityId, re.uniqCityName);
            }
            foreach (ReEventsState rs in stateCountryList)
            {
                stateCountryIds.Add(rs.stateId, rs.countryId);
                uniqStateNames.Add(rs.stateId, rs.uniqStateName);
            }
            foreach (ReEventsCountry r in countryRegionList)
            {
                countryRegionIds.Add(r.countryId, r.regionId);
                uniqCountryNames.Add(r.countryId, r.uniqCountryName);
            }
            foreach (ReEventsCountryName nm in countryList)
            {
                int regionId = countryRegionIds[nm.countryId];
                addCountryToDictionary(nm.countryName, nm.countryId, regionId);
                addCountryToDictionary(nm.countryName.ToUpper(), nm.countryId, regionId);
            }
            foreach (ReEventsStateName sn in stateList)
            {
                if (!stateCountryIds.ContainsKey(sn.stateId)) //state id is missing from main state table, should not be
                    continue;
                int countryId = stateCountryIds[sn.stateId];
                if (!countryRegionIds.ContainsKey(countryId))
                    continue;
                int regionId = countryRegionIds[countryId];
                addStateToDictionary(sn.stateName, sn.stateId, countryId, regionId, uniqStateNames[sn.stateId]);
                addStateToDictionary(sn.stateName.ToUpper(), sn.stateId, countryId, regionId, uniqStateNames[sn.stateId]);
            }

            foreach (ReEventsCityName cn in cityList)
            {
                if (!cityCountryIds.ContainsKey(cn.cityId))   //city id is missing from main city table, should not be
                    continue;
                int countryId = cityCountryIds[cn.cityId];
                if (!countryRegionIds.ContainsKey(countryId)) // country id is missing from main country table, should not be
                    continue;
                int stateId = 0;
                if (cityStateIds.ContainsKey(cn.cityId))
                    stateId = cityStateIds[cn.cityId];
                int regionId = countryRegionIds[countryId];
                addCityToDictionary(cn.cityName, cn.cityId, stateId, countryId, regionId, uniqCityNames[cn.cityId]);
                addCityToDictionary(cn.cityName.ToUpper(), cn.cityId, stateId, countryId, regionId, uniqCityNames[cn.cityId]);
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
                else if (s == "state")
                    stateLocationType = l.locTypeId;
            }
            FileInfo f = new FileInfo(streetSuffixFile);
            if (f.Exists)
            {
                string line;
                StreamReader reader = new StreamReader(streetSuffixFile);
                while ((line = reader.ReadLine()) != null)
                {
                    streetSuffixList.Add(line.ToLower().Trim());
                }
            }
        }

        void addCountryToDictionary(string countryName, int countryId, int regionId)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(countryName);
            string correctCountryName = System.Text.Encoding.UTF8.GetString(tempBytes);
            
            List<Token> tokens = tokenParser.GetTokensForText(correctCountryName);
            int k = tokens.Count;
            CountryDictionary x = countryDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token;
//                string s = tokens[i].token.ToLower();
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
                    x.countryName = correctCountryName;
                    x.regionId = regionId;
                }
            }
        }

        void addCityToDictionary(string cityName, int cityId, int stateId, int countryId, int regionId, string uniqCityName)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(cityName);
            string correctCityName = System.Text.Encoding.UTF8.GetString(tempBytes);

            List<Token> tokens = tokenParser.GetTokensForText(correctCityName);
            int k = tokens.Count;
            CityDictionary x = cityDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token;
//                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new CityDictionary { keys = new Dictionary<string, CityDictionary>(), countryId = 0, cityId = 0, stateId = 0, uniqCityName = ""};
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.countryId = countryId;
                    x.stateId = stateId;
                    x.cityName = correctCityName;
                    x.cityId = cityId;
                    x.regionId = regionId;
                    x.uniqCityName = uniqCityName;
                }
            }
        }

        void addStateToDictionary(string stateName, int stateId, int countryId, int regionId, string uniqStateName)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(stateName);
            string correctStateName = System.Text.Encoding.UTF8.GetString(tempBytes);

            List<Token> tokens = tokenParser.GetTokensForText(correctStateName);
            int k = tokens.Count;
            StateDictionary x = stateDictionary;

            for (int i = 0; i < k; i++)
            {
                string s = tokens[i].token;
//                string s = tokens[i].token.ToLower();
                if (x.keys.ContainsKey(s))
                {
                    x = x.keys[s];
                }
                else
                {
                    var z = new StateDictionary { keys = new Dictionary<string, StateDictionary>(), countryId = 0, stateId = 0, uniqStateName = "" };
                    x.keys.Add(s, z);
                    if (i < k - 1)
                        x = z;
                }
                if (i == k - 1)
                {
                    x.countryId = countryId;
                    x.stateName = correctStateName;
                    x.stateId = stateId;
                    x.regionId = regionId;
                    x.uniqStateName = uniqStateName;
                }
            }
        }

        public void extractLocationsFromTokenList(List<Token> tokens, ContentSentence sentence)
        {
            for (int i = 0; i < tokens.Count; )
            {
                string cn = "";
                int regionId = 0;
                int countryId = 0;
                int stateId = 0;
                string uniqCityName = "";
                string uniqStateName = "";
                if (i > 1)
                {
                    string t = tokens[i - 1].token.ToLower();
                    if (t == "the" || t == "or" || t == "and" || t == "&" || t == "a")
                    {
                        i++;
                        continue;
                    }
                }
                if (String.IsNullOrEmpty(tokens[i].token))
                {
                    i++;
                    continue;
                }
                if (Char.IsLower(tokens[i].token[0]))
                {
                    i++;
                    continue;
                }
                int s = findInCityDictionary(tokens, i, cityDictionary, out cn, out stateId, out countryId, out regionId, out uniqCityName);
                if (s > 0)
                {
                    int cityLenght = tokenParser.GetTokensForText(cn).Count - 1;
                    if (i + cityLenght < tokens.Count && streetSuffixList.Contains(tokens[i + cityLenght].token.ToLower()))
                    {
                        cityLenght += 1;
                    }
                    else
                    {
                        results.Add (new ResultLocation {
                            cityId = s,
                            cityName = uniqCityName,
                            stateId = stateId,
                            stateName = stateId > 0 ? uniqStateNames[stateId] : "", 
                            countryId = countryId,
                            countryName = uniqCountryNames[countryId],
                            regionId = regionId,
                            regionName = regionNames[regionId],
                            keyword = cn,
                            sent = sentence
                        });
                    }
                    i += cityLenght;
                }
                else
                {
                    s = findInStateDictionary(tokens, i, stateDictionary, out cn, out countryId, out regionId, out uniqStateName);
                    if (s > 0)
                    {
                        int stateLenght = tokenParser.GetTokensForText(cn).Count - 1;
                        if (i + stateLenght < tokens.Count && streetSuffixList.Contains(tokens[i + stateLenght].token.ToLower()))
                        {
                            stateLenght += 1;
                        }
                        else
                        {
                            results.Add(new ResultLocation
                            {
                                cityId = -1,
                                cityName = "",
                                stateId = s,
                                stateName = uniqCityName,
                                countryId = countryId,
                                countryName = uniqCountryNames[countryId],
                                regionId = regionId,
                                regionName = regionNames[regionId],
                                keyword = cn,
                                sent = sentence
                            });
                        }
                        i += stateLenght;
                    }
                    else
                    {
                        s = findInCountryDictionary(tokens, i, countryDictionary, sentence, out cn, out regionId);
                        if (s > 0)
                        {
                            int countryLenght = tokenParser.GetTokensForText(cn).Count - 1;
                            if (i + countryLenght < tokens.Count && streetSuffixList.Contains(tokens[i + countryLenght].token.ToLower()))
                            {
                                countryLenght += 1;
                            }
                            else
                            {
                                results.Add(new ResultLocation
                                {
                                    cityId = -1,
                                    cityName = "",
                                    stateId = -1,
                                    stateName = "",
                                    countryId = s,
                                    countryName = uniqCountryNames[s],
                                    regionId = regionId,
                                    regionName = regionNames[regionId],
                                    keyword = cn,
                                    sent = sentence
                                });
                            }
                            i += countryLenght;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
        }

        private int findInCountryDictionary(List<Token> tokens, int index, CountryDictionary dict, ContentSentence sentence, 
            out string countryName, out int regionId)
        {
            countryName = "";
            regionId = 0;
            string str = tokens[index].token;
//            string str = tokens[index].token.ToLower();
            if (tokens[index].tag1 == "E-ORG")
                return 0;
            if (sentence.number > 0) // often in the title Senna detected all words as org - begins with capital 
            {
                if (tokens[index].tag3 == "E-ORG" || tokens[index].tag3 == "I-ORG" || tokens[index].tag3 == "B-ORG")
                    return 0;
                if (tokens[index].tag3 == "B-PER" || tokens[index].tag3 == "E-PER")
                    return 0;
            }
            if (!dict.keys.ContainsKey(str))
                return 0;
            CountryDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                countryName = dict.countryName;
                regionId = dict.regionId;
                return dict.countryId;
            }
            if (index == tokens.Count - 1)
            {
                countryName = y.countryName;
                regionId = y.regionId;
                return y.countryId;
            }
            int s = findInCountryDictionary(tokens, index + 1, y, sentence, out countryName, out regionId); 

            if (s != 0) 
                return s;
            countryName = y.countryName;
            regionId = y.regionId;
            return y.countryId;
        }

        private int findInCityDictionary(List<Token> tokens, int index, CityDictionary dict,
            out string cityName, out int stateId, out int countryId, out int regionId, out string uniqCityNm)
        {
            cityName = "";
            regionId = 0;
            countryId = 0;
            stateId = 0;
            uniqCityNm = "";

            if (tokens[index].tag1 == "E-ORG")
                return 0;
            if (tokens[index].tag3 == "E-ORG" || tokens[index].tag3 == "I-ORG" || tokens[index].tag3 == "B-ORG")
                return 0;
            if (tokens[index].tag3 == "B-PER" || tokens[index].tag3 == "E-PER")
                return 0;
            string str = tokens[index].token;
//            string str = tokens[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            CityDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                stateId = dict.stateId;
                countryId = dict.countryId;
                regionId = dict.regionId;
                uniqCityNm = dict.uniqCityName;
                cityName = dict.cityName;
                return dict.cityId;
            }
            if (index == tokens.Count - 1)
            {
                stateId = y.stateId;
                countryId = y.countryId;
                regionId = y.regionId;
                cityName = y.cityName;
                uniqCityNm = y.uniqCityName;
                return y.cityId;
            }
            int s = findInCityDictionary(tokens, index + 1, y, out cityName, out stateId, out countryId, out regionId, out uniqCityNm);

            if (s != 0) 
                return s;
            cityName = y.cityName;
            stateId = y.stateId;
            countryId = y.countryId;
            regionId = y.regionId;
            uniqCityNm = y.uniqCityName;
            return y.cityId;
        }

        private int findInStateDictionary(List<Token> tokens, int index, StateDictionary dict,
            out string stateName, out int countryId, out int regionId, out string uniqStateNm)
        {
            stateName = "";
            regionId = 0;
            countryId = 0;
            uniqStateNm = "";

            if (tokens[index].tag1 == "E-ORG")
                return 0;
            if (tokens[index].tag3 == "E-ORG" || tokens[index].tag3 == "I-ORG" || tokens[index].tag3 == "B-ORG")
                return 0;
            if (tokens[index].tag3 == "B-PER" || tokens[index].tag3 == "E-PER")
                return 0;
            string str = tokens[index].token;
//            string str = tokens[index].token.ToLower();
            if (!dict.keys.ContainsKey(str))
                return 0;
            StateDictionary y = dict.keys[str];
            if (y == null || y.keys == null || y.keys.Count == 0)
            {
                countryId = dict.countryId;
                regionId = dict.regionId;
                uniqStateNm = dict.uniqStateName;
                stateName = dict.stateName;
                return dict.stateId;
            }
            if (index == tokens.Count - 1)
            {
                countryId = y.countryId;
                regionId = y.regionId;
                stateName = y.stateName;
                uniqStateNm = y.uniqStateName;
                return y.stateId;
            }
            int s = findInStateDictionary(tokens, index + 1, y, out stateName, out countryId, out regionId, out uniqStateNm);

            if (s != 0)
                return s;
            stateName = y.stateName;
            countryId = y.countryId;
            regionId = y.regionId;
            uniqStateNm = y.uniqStateName;
            return y.stateId;
        }

        public void addPredefinedCountry()
        {
            int regionId = countryRegionIds[predefinedCountryId];
            if (!countryIds.Contains(predefinedCountryId))
            {
                addToLocations(uniqCountryNames[predefinedCountryId]);
                addToLocationKeywords(uniqCountryNames[predefinedCountryId] + "(predefined)", 0);
                countryIds.Add(predefinedCountryId);
            }
            if (!regionIds.Contains(regionId))
                regionIds.Add(regionId);
        }

        void addToLocationKeywords(string keyword, int sentenceNumber)
        {
            if (locationKeywords.Length > 0)
                locationKeywords += "|";
            locationKeywords += "{" + sentenceNumber.ToString() + "} " + keyword;
        }

        void addToLocations(string nm)
        {
            if (locations.Length > 0)
                locations += "|";
            locations += nm;
        }

        private void addFromResult(ResultLocation loc)
        {
            if (loc.cityId >= 0 && !cityIds.Contains(loc.cityId))
            {
                addToLocationKeywords(loc.keyword, loc.sent.number);
                addToLocations(loc.cityName);
                cityIds.Add(loc.cityId);
            }
            if (loc.stateId >= 0 && !stateIds.Contains(loc.cityId))
            {
                if (loc.cityId < 0)
                    addToLocationKeywords(loc.keyword, loc.sent.number);
                addToLocations(loc.stateName);
                stateIds.Add(loc.stateId);
            }
            if (!countryIds.Contains(loc.countryId))
            {
                if (loc.cityId < 0 && loc.stateId < 0)
                    addToLocationKeywords(loc.keyword, loc.sent.number);
                countryIds.Add(loc.countryId);
                addToLocations(loc.countryName);
            }
            if (!regionIds.Contains(loc.regionId))
            {
                addToLocations(regionNames[loc.regionId]);
                regionIds.Add(loc.regionId);
            }
            loc.sent.addLocation(loc);
        }


        public bool isCityNamePartOfAnother(string nm)
        {
            foreach (ResultLocation loc in results)
            {
                if (loc.cityId > 0 && loc.cityName.Length > nm.Length && loc.cityName.IndexOf(nm) == 0)
                    return true;
            }
            return false;
        }

        public void getLocations()
        {
            for (int k = 0; k < results.Count; )
            {
                if (results[k].cityId > 0 && isCityNamePartOfAnother(results[k].cityName))
                {
                    results.RemoveAt(k);
                    continue;
                }
                k++;
            }

            int sentenceNumberWithFirstPriority = -1;

            foreach (ResultLocation loc in results)
            {
                if (loc.sent.number == 0 && loc.cityId <= 0)
                {
                    // if title contains location (country) don't search another
                    addFromResult(loc);
                    sentenceNumberWithFirstPriority = 0;
                }

                if (loc.sent.hasArticleDate)
                {
                    // if string as "Washington, May 2, 2015" exists, don't search another
                    if (sentenceNumberWithFirstPriority < 0 || sentenceNumberWithFirstPriority == loc.sent.number)
                    {
                        addFromResult(loc);
                        sentenceNumberWithFirstPriority = loc.sent.number;
                    }
                }
            }
            if (sentenceNumberWithFirstPriority >= 0)
                return;

            // analyze if there are many locations and try to filter out locations in sentences without events
            bool haveLocationsWithEvents = false;
            foreach (ResultLocation loc in results) //here will be check event
            {
                if (loc.sent.eventWasFound)
                    haveLocationsWithEvents = true;
            }
            foreach (ResultLocation loc in results) //here will be check event
            {
                if (!haveLocationsWithEvents || loc.sent.eventWasFound)
                    addFromResult(loc);
            }
            // if we didn't found any location, use predefined, if exist
            if (countryIds.Count == 0 && predefinedCountryId > 0)
                addPredefinedCountry();
        }

        public List<GeoLocation> getLocationIds()
        {
            List<GeoLocation> res = new List<GeoLocation>();
            foreach (int id in countryIds)
            {
                res.Add(new GeoLocation { locationId = id, locationType = countryLocationType });
            }
            foreach (int id in cityIds)
            {
                res.Add(new GeoLocation { locationId = id, locationType = cityLocationType });
            }
            foreach (int id in regionIds)
            {
                res.Add(new GeoLocation { locationId = id, locationType = regionLocationType });
            }
            return res;
        }

        public void writeLocationsToResult(ReEventsParser.WriteLineToResult writeLineToResult)
        {
            writeLineToResult("location keywords", locationKeywords);
            writeLineToResult("locations - db", locations);
        }

        public void addCountry(int countryId)
        {
            if (countryId == 0)
                return;
            countryIds.Add(countryId);
            int regionId = countryRegionIds[countryId];
            if (!countryIds.Contains(countryId))
            {
                addToLocations(uniqCountryNames[countryId]);
                addToLocationKeywords(uniqCountryNames[countryId], 0);
                countryIds.Add(countryId);
            }
            if (!regionIds.Contains(regionId))
                regionIds.Add(regionId);
        }

        public string getCountryName(int id)
        {
            if (uniqCountryNames.ContainsKey(id))
                return uniqCountryNames[id];
            return "";
        }
        public string getCityName(int id)
        {
            if (uniqCityNames.ContainsKey(id))
                return uniqCityNames[id];
            return "";
        }

    }



}
