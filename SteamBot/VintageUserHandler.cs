using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class VintageUserHandler : StrangeBankV2UserHandler
    {
        public VintageUserHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override int GetOtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            int value = ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value <= 3)
                return value - 1;
            if (value <= 6)
                return value - 2;
            if (value <= 12)
                return value - 3;
            if (value <= 15)
                return 9;
            if (value <= ToScrap(new Pricelist.Price(3, "metal")))
                return value - 6;
            if (value <= ToScrap(new Pricelist.Price(3.33, "metal")))
                return ToScrap(new Pricelist.Price(2.33, "metal"));
            if (value <= ToScrap(new Pricelist.Price(5, "metal")))
                return value - 9;
            else
                return ToScrap(new Pricelist.Price(4, "metal"));
        }

        public override int GetMyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
        }

        public override bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            int count = Trade.MyInventory.GetItemsByDefindex(inventoryItem.Defindex).Count;

            foreach (ulong id in Trade.OtherOfferedItems)
            {
                Inventory.Item otherItem = Trade.OtherInventory.GetItem(id);
                if (otherItem.Defindex == inventoryItem.Defindex)
                {
                    count++;
                }
            }
            if (count >= 4)
            {
                reason = "I have too many of that item.";
                return false;
            }
            if (IsGifted(inventoryItem))
            {
                reason = "Item is gifted.";
                return false;
            }
            if (inventoryItem.Quality != "3")
            {
                reason = "Item is not vintage.";
                return false;
            }
            reason = null;
            return true;
        }

        public override bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (inventoryItem.Quality == "3")
            {
                reason = "";
                return true;
            }
            else
            {
                reason = "Item is not vintage.";
                return false;
            }
        }
    }
}
