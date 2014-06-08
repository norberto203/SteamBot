using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class HatUserHandler : StrangeBankUserHandler
    {
        private HashSet<int> BannedHats = new HashSet<int>() { 125, 279, 584, 189, 240, 268, 269, 270, 272, 272, 273, 274, 275, 276, 277, 292, 299 };

        public HatUserHandler(Bot bot, SteamID sid) : base(bot, sid) 
        { 
            Pricelist.LoadBlacklist();
        }

        public override Price MyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
        }

        public override Price OtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            Price value = Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value < Pricelist.Refined * 1.33)
                return Price.Zero;
            if (value <= Pricelist.Refined * 2)
                return value - Pricelist.Scrap * 3;
            if (value <= Pricelist.Refined * 2.33)
                return Pricelist.Refined * 1.66;
            if (value <= Pricelist.Refined * 3.33)
                return value - Pricelist.Scrap * 6;
            if (value <= Pricelist.Refined * 3.66)
                return Pricelist.Refined * 2.66;
            if (value <= Pricelist.Refined * 5)
                return value - Pricelist.Refined * 1;
            if (value <= Pricelist.Refined * 5.66)
                return Pricelist.Refined * 4;
            if (value <= Pricelist.Refined * 6.66)
                return value - Pricelist.Refined * 1.66;
            else
                return Pricelist.Refined * 5;
        }

        public bool IsCraftHat(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            if (inventoryItem.IsNotCraftable)
                return false;

            if (inventoryItem.IsNotTradeable)
                return false;

            else if (!(schemaItem.ItemSlot == "head" || schemaItem.ItemSlot == "misc"))
                return false;

            else if (BannedHats.Contains(inventoryItem.Defindex))
                return false;

            return true;
        }

        public override bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (!IsCraftHat(inventoryItem, schemaItem))
            {
                reason = "Item is not a craft hat.";
                return false;
            }

            if (Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false) < Pricelist.Refined * 1.33)
            {
                reason = "Item is below minimum accepted price.";
                return false;
            }

            if (inventoryItem.Quality != "6")
            {
                reason = "Item is not Unique.";
                return false;
            }

            if (IsGifted(inventoryItem))
            {
                reason = "Item is gifted.";
                return false;
            }
            if (getNumItems(inventoryItem.Defindex, inventoryItem.Quality) >= 3)
            {
                reason = "I have too many of that item.";
                return false;
            }
            reason = "Pass";
            return true;
        }

        public override bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (!IsCraftHat(inventoryItem, schemaItem))
            {
                reason = "Item is not a craft hat.";
                return false;
            }
            else
            {
                reason = "Pass";
                return true;
            }
        }
    }
}
