using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamBot
{
    public struct Price
    {
        private int scrap;

        public Price(int scrap)
        {
            this.scrap = scrap;
        }

        public static Price Zero
        {
            get
            {
                return new Price(0);
            }
        }

        public int Scrap
        {
            get
            {
                return scrap;
            }
        }

        public double Reclaimed
        {
            get
            {
                return this / Pricelist.Reclaimed;
            }
        }

        public double Refined
        {
            get
            {
                return this / Pricelist.Refined;
            }
        }

        public double Keys
        {
            get
            {
                return this / Pricelist.Key;
            }
        }

        public double Earbuds
        {
            get
            {
                return this / Pricelist.Earbuds;
            }
        }

        public static Price operator +(Price p1, Price p2)
        {
            return new Price(p1.Scrap + p2.Scrap);
        }

        public static Price operator -(Price p1, Price p2)
        {
            return new Price(p1.Scrap - p2.Scrap);
        }

        public static Price operator *(double d, Price p)
        {
            return new Price((int)(p.Scrap * d));
        }

        public static Price operator *(Price p, double d)
        {
            return d * p;
        }

        public static double operator /(Price p, Price c)
        {
            return (float)p.scrap / (float)c.scrap;
        }

        public static bool operator ==(Price p1, Price p2)
        {
            return p1.scrap == p2.scrap;
        }

        public static bool operator !=(Price p1, Price p2)
        {
            return !(p1 == p2);
        }

        public static bool operator >(Price p1, Price p2)
        {
            return p1.scrap > p2.scrap;
        }

        public static bool operator <(Price p1, Price p2)
        {
            return p1.scrap < p2.scrap;
        }

        public static bool operator >=(Price p1, Price p2)
        {
            return p1.scrap >= p2.scrap;
        }

        public static bool operator <=(Price p1, Price p2)
        {
            return p1.scrap <= p2.scrap;
        }

        public override string ToString()
        {
            string currency;
            string value;
            if (this < Pricelist.Refined)
            {
                value = scrap.ToString();
                currency = "Scrap";
            }
            else if (this < Pricelist.Key)
            {
                value = ((scrap * 100 / Pricelist.Refined.Scrap) / 100f).ToString();
                currency = "Refined";
            }
            else if (this < Pricelist.Earbuds)
            {
                value = ((scrap * 100 / Pricelist.Key.Scrap) / 100f).ToString();
                currency = "Keys";
            }
            else
            {
                value = ((scrap * 100 / Pricelist.Earbuds.Scrap) / 100f).ToString();
                currency = "Earbuds";
            }
            return String.Format("{0} {1}", value, currency);
        }
    }
}
