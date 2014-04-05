using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SteamBot
{
    public static class Pricelist
    {
        private static Dictionary<String, Object> prices = null;
        public static HashSet<int> blacklist = null;

        public static void LoadBlacklist()
        {
            if (blacklist == null)
            {
                blacklist = new HashSet<int>();
                StreamReader reader = new StreamReader("blacklist.txt");
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
                string data = new WebClient().DownloadString("http://backpack.tf/api/IGetPrices/v3/?format=vdf&key=51a1653bba25360638000001");
                File.WriteAllText("pricelist.vdf", data);
                prices = loadPricelist(data);
            }
            catch (WebException)
            {
                if (prices == null)
                {
                    StreamReader reader = new StreamReader("pricelist.vdf");
                    string data = reader.ReadToEnd();
                    reader.Close();
                    prices = loadPricelist(data);
                }
            }
        }

        private static Dictionary<string, Object> loadPricelist(string pricelist)
        {
            string[] file = pricelist.Split(new char[] { '\n' });
            int i = 0;
            return readThroughBraces(file, ref i);
        }

        private static Object[] readKey(string[] file, ref int i)
        {
            string key = file[i].Trim();
            Object[] keyObject = new Object[2];
            string nextLine = null;
            if (i != file.Length - 1)
            {
                nextLine = file[i + 1].Trim();
            }
            if (nextLine != null && nextLine.Equals("{"))
            {
                i += 2;
                keyObject[1] = readThroughBraces(file, ref i);
                keyObject[0] = key.Replace("\"", "");
            }

            else
            {
                int start1 = key.IndexOf("\"", 0);
                int end1 = key.IndexOf("\"", start1 + 1);
                int start2 = key.IndexOf("\"", end1 + 1);
                int end2 = key.IndexOf("\"", start2 + 1);

                keyObject[0] = key.Substring(start1 + 1, end1 - start1 - 1);
                keyObject[1] = key.Substring(start2 + 1, end2 - start2 - 1);
            }
            return keyObject;
        }

        private static Dictionary<string, Object> readThroughBraces(string[] file, ref int i)
        {
            Dictionary<string, Object> subdict = new Dictionary<string, object>();
            for (; (i < file.Length && !file[i].Trim().Equals("}")); i++)
            {
                if (file[i].Trim().Equals(""))
                {
                    continue;
                }
                Object[] keyInfo = readKey(file, ref i);
                subdict.Add((string)keyInfo[0], keyInfo[1]);
            }
            return subdict;
        }

        public static string getCurrency(int defindex, string quality)
        {
            return (string)traverse("Response", "prices", defindex + "", quality, "0", "currency");
        }

        public static Price getHighPrice(int defindex, string quality)
        {
            double value = Convert.ToDouble((string)traverse("Response", "prices", defindex + "", quality + "", "0", "value_high"));
            string currency = getCurrency(defindex, quality);
            return new Price(value, currency);
        }

        public static Price getLowPrice(int defindex, string quality)
        {
            double value = Convert.ToDouble((string)traverse("Response", "prices", defindex + "", quality, "0", "value" ));
            string currency = getCurrency(defindex, quality);
            return new Price(value, currency);
        }

        private static Object traverse(Dictionary<string, object> currentLevel, string[] keys, int index)
        {
            if (currentLevel.ContainsKey(keys[index]))
            {
                Object value = currentLevel[keys[index]];

                if (index >= keys.Length - 1)
                {
                    return value;
                }

                else
                {
                    currentLevel = (Dictionary<string, object>)value;
                    return traverse(currentLevel, keys, index + 1);
                }
            }
            else
            {
                if (keys[index].Equals("value_high") && currentLevel.ContainsKey("value"))
                {
                    return currentLevel["value"];
                }
                throw new Exception(String.Format("Could not find the key {0} in {1}", keys[index], keys.ToString()));
            }
        }

        public static Object traverse(params string[] keys)
        {
            return traverse(prices, keys, 0);
        }

        public struct Price
        {
            public Price(double value, string currency)
            {
                Value = value;
                Currency = currency;
            }

            public double Value;
            public string Currency;
        }
    }
}
