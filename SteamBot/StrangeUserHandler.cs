using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class StrangeUserHandler : StrangeBankUserHandler
    {
        public StrangeUserHandler(Bot bot, SteamID sid) : base(bot, sid) 
        { 
            Pricelist.LoadBlacklist();
        }

        public override Price OtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            Price value = Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value <= Pricelist.Scrap * 3)
                return value - Pricelist.Scrap * 1;
            if (value <= Pricelist.Scrap * 6)
                return value - Pricelist.Scrap * 2;
            if (value <= Pricelist.Refined * 2)
                return value - Pricelist.Scrap * 3;
            if (value <= Pricelist.Refined * 2.33)
                return Pricelist.Refined * 1.66;
            if (value <= Pricelist.Refined * 3.66)
                return value - Pricelist.Scrap * 6;
            if (value <= Pricelist.Refined * 4)
                return Pricelist.Refined * 3;
            if (value <= Pricelist.Refined * 5.33)
                return value - Pricelist.Refined * 1;
            if (value <= Pricelist.Refined * 5.66)
                return Pricelist.Refined * 4.33;
            if (value <= Pricelist.Refined * 6.33)
                return value - Pricelist.Refined * 1.33;
            else
                return Pricelist.Refined * 5;
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
            if (getNumItems(inventoryItem.Defindex, inventoryItem.Quality) >= 3)
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
            if (inventoryItem.IsNotCraftable)
            {
                reason = "Item is not craftable.";
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
            else
            {
                reason = "";
                return true;
            }
        }
    }
}
