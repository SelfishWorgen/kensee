using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerDB
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DBConnect db_obj = new DBConnect("kensee");
                List<WebSiteInfo> webSiteInfo = db_obj.readNewsWebsite();
                db_obj = null;

                Dictionary<int, InfoForSentimentGraph> newResidentialConstructionDict = new Dictionary<int, InfoForSentimentGraph>();
                Dictionary<int, InfoForSentimentGraph> nationalHomePriceIndex = new Dictionary<int, InfoForSentimentGraph>();

                MacroCreator mc = new MacroCreator();
                mc.fillTable(newResidentialConstructionDict, nationalHomePriceIndex);

                MarketSentimentCreator msc = new MarketSentimentCreator();
                msc.fillTable(webSiteInfo, newResidentialConstructionDict, nationalHomePriceIndex);

                HighlightsCreator hlc = new HighlightsCreator();
                hlc.fillTable();

                ComparisionDataCreator cc = new ComparisionDataCreator();
                cc.fillTable();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
