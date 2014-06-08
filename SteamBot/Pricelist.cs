using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SteamBot
{
    public static class Pricelist
    {
        private static JToken pricelist;

        public static HashSet<int> blacklist = null;

        private static string pricelistFile = "pricelist.json";
        private static string blacklistFile = "blacklist.txt";
        private static string apiURL = "http://www.trade.tf/api/spreadsheet.json?key={0}";
        private static string apiKey = "528f474a9cbc39f0f5d4c60ad2c4262e";

        public static Price Get(int defindex, string quality, bool high)
        {
            float value;
            string currency;

            GetRaw(defindex, quality, high, out value, out currency);
            return new Price(ToScrap(value, currency));
        }

        public static bool HasPrice(int defindex, string quality)
        {
            JToken location = null;
            try
            {
                location = pricelist.SelectToken("items").SelectToken(defindex.ToString()).SelectToken(quality).SelectToken("regular");
            }
            catch (NullReferenceException) { }

            return location != null;
        }

        private static void GetRaw(int defindex, string quality, bool high, out float value, out string currency)
        {
            string key = high ? "hi" : "low";
            JToken location = null;
            try
            {
                location = pricelist.SelectToken("items").SelectToken(defindex.ToString()).SelectToken(quality).SelectToken("regular");
            }
            catch (NullReferenceException) { }
            if (location == null)
            {
                throw new PriceNotFoundException(defindex, quality);
            }

            JToken item = location[key];
            if (item == null)
            {
                item = location["value"];
            }

            value = (float)item;
            currency = (string)location["unit"];
        }

        private static int ToScrap(float value, string currency)
        {
            if (currency == "r")
            {
                return (int)Math.Round(value * 9);
            }
            else if (currency == "k")
            {
                return (int)Math.Round(value * Key.Scrap);
            }
            else if (currency == "b")
            {
                return (int)Math.Round(value * Earbuds.Scrap);
            }
            else
            {
                throw new CurrencyNotFoundException(currency);
            }
        }

        public static Price Key
        {
            get
            {
                float value = (float)pricelist.SelectToken("units").SelectToken("k");

                return new Price(ToScrap(value, "r"));
            }
        }

        public static Price Earbuds
        {
            get
            {
                float value = (float)pricelist.SelectToken("units").SelectToken("b");

                return new Price(ToScrap(value, "k"));
            }
        }

        public static Price Refined
        {
            get
            {
                return new Price(9);
            }
        }

        public static Price Reclaimed
        {
            get
            {
                return new Price(3);
            }
        }

        public static Price Scrap
        {
            get
            {
                return new Price(1);
            }
        }

        public static void LoadBlacklist()
        {
            if (blacklist == null)
            {
                blacklist = new HashSet<int>();
                StreamReader reader = new StreamReader(blacklistFile);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    blacklist.Add(Int32.Parse(line.Trim()));
                }
            }
        }

        public static void RefreshData()
        {
            try
            {
                string raw = new WebClient().DownloadString(String.Format(apiURL, apiKey));
                pricelist = JObject.Parse(raw);
                File.WriteAllText(pricelistFile, raw);
            }
            catch (WebException) { }

            if (pricelist == null)
            {
                StreamReader reader = new StreamReader(pricelistFile);
                string raw = reader.ReadToEnd();
                reader.Close();
                pricelist = JObject.Parse(raw);
                
            }
        }

        

        public class PriceNotFoundException : Exception 
        {
            public PriceNotFoundException(int defindex, string quality) : base(String.Format("Price for defindex {0} and quality {1} could not be found.", defindex, quality)) { }
        }

        public class CurrencyNotFoundException : Exception
        {
            public CurrencyNotFoundException(string currency) : base(String.Format("Invalid currency: {0}", currency)) { }
        }
    }
}
