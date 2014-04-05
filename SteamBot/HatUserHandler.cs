using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;
using SteamTrade;

namespace SteamBot
{
    class HatUserHandler : StrangeBankV2UserHandler
    {
        private HashSet<int> BannedHats = new HashSet<int>() { 125, 279, 584, 189, 240, 268, 269, 270, 272, 272, 273, 274, 275, 276, 277, 292, 299 };

        public HatUserHandler(Bot bot, SteamID sid) : base(bot, sid) 
        { 
            Pricelist.LoadBlacklist();
        }

        public override int GetMyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            try
            {
                return ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override int GetOtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            int value = ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality));
            //int highValue = ToScrap(Pricelist.getHighPrice(inventoryItem.Defindex, inventoryItem.Quality));

            if (value < ToScrap(new Pricelist.Price(1.33, "metal")))
                return 0;
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

            if (ToScrap(Pricelist.getLowPrice(inventoryItem.Defindex, inventoryItem.Quality)) < 12)
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
            reason = null;
            return true;
        }

        public override bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            if (GetMyValue(inventoryItem, schemaItem) == 0)
            {
                reason = "I could not find the price of that item";
                return false;
            }
            else if (!IsCraftHat(inventoryItem, schemaItem))
            {
                reason = "Item is not a craft hat.";
                return false;
            }
            else
            {
                reason = "I could not find the price of that item";
                return true;
            }
        }
    }
}
