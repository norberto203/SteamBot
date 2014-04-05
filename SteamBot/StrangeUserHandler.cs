using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class StrangeUserHandler : StrangeBankV2UserHandler
    {
        public StrangeUserHandler(Bot bot, SteamID sid) : base(bot, sid) 
        { 
            Pricelist.LoadBlacklist();
        }

        public override int GetOtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            int value = ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value <= 3)
                return value - 1;
            if (value <= 6)
                return value - 2;
            if (value <= ToScrap(new Pricelist.Price(2, "metal")))
                return value - 3;
            if (value <= ToScrap(new Pricelist.Price(2.33, "metal")))
                return ToScrap(new Pricelist.Price(1.66, "metal"));
            if (value <= ToScrap(new Pricelist.Price(3.66, "metal")))
                return value - 6;
            if (value <= ToScrap(new Pricelist.Price(4, "metal")))
                return ToScrap(new Pricelist.Price(3, "metal"));
            if (value <= ToScrap(new Pricelist.Price(5.33, "metal")))
                return value - 9;
            if (value <= ToScrap(new Pricelist.Price(5.66, "metal")))
                return ToScrap(new Pricelist.Price(4.33, "metal"));
            if (value <= ToScrap(new Pricelist.Price(6.33, "metal")))
                return value - ToScrap(new Pricelist.Price(1.33, "metal"));
            return ToScrap(new Pricelist.Price(5, "metal"));
        }

        public override bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (Pricelist.blacklist.Contains(inventoryItem.Defindex))
            {
                reason = "Item is blacklisted.";
                return false;
            }
            if (schemaItem.Name.ToLower().Contains("botkiller"))
            {
                reason = "Botkillers are not accepted.";
                return false;
            }
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
            if (schemaItem.ItemSlot == "head" || schemaItem.ItemSlot == "misc")
            {
                reason = "For the time being, strange hats are not accepted.";
                return false;
            }
            if (inventoryItem.Quality != "11")
            {
                reason = "Item is not strange.";
                return false;
            }
            reason = null;
            return true;
        }

        public override bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (inventoryItem.Quality != "11")
            {
                reason = "Item is not strange.";
                return false;
            }
            else if (inventoryItem.IsNotTradeable)
            {
                reason = "Item is not tradeable.";
                return false;
            }
            else if (GetMyValue(inventoryItem, schemaItem) == 0)
            {
                reason = "I could not find the price of that item";
                return false;
            }
            else
            {
                reason = "";
                return true;
            }
        }
    }
}
