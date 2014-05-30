using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    /*
    class ToolUserHandler : StrangeBankUserHandler
    {
        private HashSet<int> Tools = new HashSet<int>() { 
            5020, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 
            5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 
            5042, 5044, 5046, 5050, 5051, 5052, 5053, 5054, 
            5055, 5056, 5060, 5065, 5070, 5071, 0725, 0758
        };

        public ToolUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {
            Pricelist.LoadBlacklist();
        }

        public override int GetMyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));
        }

        public override int GetOtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            int value = ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value < ToScrap(new Pricelist.Price(.33, "metal")))
                return value - 1;
            if (value < ToScrap(new Pricelist.Price(.66, "metal")))
                return value - 2;
            if (value <= ToScrap(new Pricelist.Price(2, "metal")))
                return value - 3;
            if (value <= ToScrap(new Pricelist.Price(2.33, "metal")))
                return ToScrap(new Pricelist.Price(1.66, "metal"));
            if (value <= ToScrap(new Pricelist.Price(3.33, "metal")))
                return value - 6;
            if (value <= ToScrap(new Pricelist.Price(3.66, "metal")))
                return ToScrap(new Pricelist.Price(2.66, "metal"));
            if (value <= ToScrap(new Pricelist.Price(5, "metal")))
                return value - 9;
            if (value <= ToScrap(new Pricelist.Price(5.66, "metal")))
                return ToScrap(new Pricelist.Price(4, "metal"));
            if (value <= ToScrap(new Pricelist.Price(6.66, "metal")))
                return value - ToScrap(new Pricelist.Price(1.66, "metal"));
            return ToScrap(new Pricelist.Price(5, "metal"));
        }

        public bool IsTool(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            if (Tools.Contains(inventoryItem.Defindex))
                return true;

            if (schemaItem.Name.ToLower().Contains("strange part"))
                return true;

            return false;
        }

        public override bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (!IsTool(inventoryItem, schemaItem))
            {
                reason = "Item is not a tool.";
                return false;
            }
            reason = null;
            return true;
        }

        public override bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            reason = "Item is not a tool.";
            return IsTool(inventoryItem, schemaItem);
        }
    }
     * */
}
