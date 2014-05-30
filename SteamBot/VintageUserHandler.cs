using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class VintageUserHandler : StrangeBankUserHandler
    {
        public VintageUserHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override Price OtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            Price value = Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value <= Pricelist.Scrap * 3)
                return value - Pricelist.Scrap * 1;
            if (value <= Pricelist.Scrap * 6)
                return value - Pricelist.Scrap * 2;
            if (value <= Pricelist.Refined * 1.33)
                return value - Pricelist.Scrap * 3;
            if (value <= Pricelist.Refined * 1.66)
                return Pricelist.Refined * 1;
            if (value <=  Pricelist.Refined * 3)
                return value - Pricelist.Scrap * 0.66;
            if (value <=  Pricelist.Refined * 3.33)
                return  Pricelist.Refined * 2.33;
            if (value <=  Pricelist.Refined * 5)
                return value -  Pricelist.Refined * 1;
            else
                return  Pricelist.Refined * 4;
        }

        public override Price MyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
        }

        public override bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            int count = getNumItems(inventoryItem.Defindex, inventoryItem.Quality);

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
