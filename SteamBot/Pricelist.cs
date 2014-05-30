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

        // Stock weapons are keyed by their non-upgradeable index in the pricelist, so we need to convert the upgradeable defindex to the Normal quality one.
        private static Dictionary<int, int> old = new Dictionary<int, int> 
        { 
            { 205, 18 }, // Rocket Launcher
            { 199, 10 }, // Shotgun (All Four Classes)
            { 196, 6 },  // Shovel
        
            { 208, 21 }, // Flamethrower
            { 192, 2 },  // Fire Axe

            { 200, 13 }, // Scattergun
            { 209, 23 }, // Pistol (Engie and Scout)
            { 190, 0 },  // Bat

            { 206, 19 }, // Grenade Launcher
            { 207, 20 }, // Stickybomb Launcher
            { 191, 1 },  // Bottle

            { 202, 15 }, // Minigun
            { 195, 5 },  // Fists

            { 197, 7 },  // Wrench
            { 737, 25 }, // Construction PDA

            { 211, 29 }, // Medigun
            { 204, 17 }, // Syringe Gun
            { 198, 8 },  // Bonesaw

            { 201, 14 }, // Sniper Rifle
            { 203, 16 }, // SMG
            { 193, 3 },  // Kukri

            
            { 210, 24 }, // Revolver
            { 212, 30 }, // Invis Watch
            { 194, 4 },  // Knife
        };

        public static HashSet<int> blacklist = null;

        private static string pricelistFile = "pricelist.json";
        private static string blacklistFile = "blacklist.txt";
        private static string apiURL = "http://backpack.tf/api/IGetPrices/v3/?format=json&key={0}";
        private static string apiKey = "51a1653bba25360638000001";

        public static Price Get(int defindex, string quality, bool high)
        {
            float value;
            string currency;

            GetRaw(defindex, quality, high, out value, out currency);
            return new Price(ToScrap(value, currency));
        }

        public static bool HasPrice(int defindex, string quality)
        {
            if (old.ContainsKey(defindex))
            {
                defindex = old[defindex];
            }
            JToken location = null;
            try
            {
                location = pricelist.SelectToken(defindex.ToString()).SelectToken(quality).SelectToken("0").SelectToken("current");
            }
            catch (NullReferenceException) { }
            if (location == null)
            {
                return false;
            }
            string currency = (string)location["currency"];
            if (currency == "metal" || currency == "keys" || currency == "earbuds" || currency == "usd")
            {
                return true;
            }
            return false;
        }

        private static void GetRaw(int defindex, string quality, bool high, out float value, out string currency)
        {
            if (old.ContainsKey(defindex))
            {
                defindex = old[defindex];
            }
            string key = high ? "value_high" : "value";
            JToken location = null;
            try
            {
                location = pricelist.SelectToken(defindex.ToString()).SelectToken(quality).SelectToken("0").SelectToken("current");
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
            currency = (string)location["currency"];
        }

        private static int ToScrap(float value, string currency)
        {
            if (currency == "metal")
            {
                return (int)Math.Round(value * 9);
            }
            else if (currency == "keys")
            {
                return (int)Math.Round(value * Key.Scrap);
            }
            else if (currency == "earbuds")
            {
                return (int)Math.Round(value * Earbuds.Scrap);
            }
            else if (currency == "usd")
            {
                return (int)Math.Round(value * USD.Scrap);
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
                float value;
                string currency;

                GetRaw(5021, "6", true, out value, out currency);
                return new Price(ToScrap(value, currency));
            }
        }

        public static Price Earbuds
        {
            get
            {
                float value;
                string currency;

                GetRaw(143, "6", true, out value, out currency);

                return new Price(ToScrap(value, currency));
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

        public static Price USD
        {
            get
            {
                float value;
                string currency;

                GetRaw(5002, "6", true, out value, out currency);

                return new Price((int)((1 / value) * 9));
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
                JObject temp = JObject.Parse(raw);
                pricelist = temp["response"]["prices"];
                File.WriteAllText(pricelistFile, raw);
            }
            catch (WebException) { }

            if (pricelist == null)
            {
                StreamReader reader = new StreamReader(pricelistFile);
                string raw = reader.ReadToEnd();
                reader.Close();
                pricelist = JObject.Parse(raw)["response"]["prices"];
                
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
