using SteamTrade;
using System.Collections.Generic;
using SteamKit2;
using System;
using System.Net.Mail;
using System.Net;

namespace SteamBot
{
    public class StrangeBankUserHandler : UserHandler
    {
        private int otherItemValue;
        private int otherMetal;
        private int otherKeys;

        private int myItemValue;
        private int myMetal;
        private int myKeys;

        private bool isBalanced;
        private bool donate;
        private bool trades = true;
        private bool tradeReady = false;

        private Dictionary<int, int> metal = new Dictionary<int, int>() { { 5000, 1 }, { 5001, 3 }, { 5002, 9 } };

        public StrangeBankUserHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override void OnLoginCompleted()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        {
            Log.Info(Bot.SteamFriends.GetFriendPersonaName(sender) + ": " + message);
            base.OnChatRoomMessage(chatID, sender, message);
        }

        public override bool OnFriendAdd()
        {
            if (Bot.SteamFriends.GetFriendCount() > 100)
            {
                RemoveFriend:
                int i = new Random().Next(Bot.SteamFriends.GetFriendCount());
                ulong sid = Bot.SteamFriends.GetFriendByIndex(i);
                if (sid == Bot.Admins[0])
                {
                    goto RemoveFriend;
                }
                else
                {
                    Bot.SteamFriends.SendChatMessage(sid, EChatEntryType.ChatMsg, "Sorry, but I am removing you to make room in my friends list. Don't worry, you can always add me again, and make sure to join my group here: http://steamcommunity.com/groups/strangebank");
                    Bot.SteamFriends.RemoveFriend(sid);
                }
            }
            return true;
        }

        public override bool OnGroupAdd()
        {
            return true;
        }

        public override void OnFriendRemove() { }

        public override void OnMessage(string message, EChatEntryType type)
        {
            message = message.ToLower();
            if (Trade != null && OtherSID.Equals(Trade.OtherSID) && tradeReady && message.StartsWith(String.Format("http://steamcommunity.com/profiles/{0}/inventory", Trade.MySteamId.ConvertToUInt64())))
            {
                ulong itemID;
                if (message[62] == '/')
                {
                    if (! UInt64.TryParse(message.Substring(70), out itemID))
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Bad URL, make sure you didn't type anything after it.");
                    }
                }
                else
                {
                    if (!UInt64.TryParse(message.Substring(69), out itemID))
                    {
                        Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Bad URL, make sure you didn't type anything after it.");
                    }
                }
                Inventory.Item item = Trade.MyInventory.GetItem(itemID);

                if (item == null)
                {
                    Trade.SendMessage(String.Format("I could not find the item {0} in my inventory.", itemID));
                    return;
                }

                Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);

                string reason;
                
                if (ShouldSell(item, schemaItem, out reason) && Pricelist.HasPrice(item.Defindex, item.Quality) && Trade.AddItem(itemID))
                {
                    Price value = MyValue(item, schemaItem);
                    Trade.SendMessage(String.Format("Adding {0}. Cost: {1}", schemaItem.Name, value));
                    myItemValue += value.Scrap;
                    balanceTrade();
                }
                else
                {
                    Trade.SendMessage("Sorry, I can not sell that item. Reason: " + reason);
                }
            }
            else if (message.StartsWith("note") && IsAdmin)
            {
                string[] msg = message.Split(' ');
                string name = msg[1];
                string quality = msg[2];
                Schema.Item schemaItem = schemaSearch(name);

                if (schemaItem == null)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry, I could not find an item with that name in the schema.");
                    return;
                }

                Inventory.Item inventoryItem = null;

                if (inventoryItem == null)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry, I could not find that item in my inventory.");
                    return;
                }
                Bot.GetInventory();
                foreach (Inventory.Item item in Bot.MyInventory.Items)
                {
                    if (item.Defindex == schemaItem.Defindex && item.Quality == quality)
                    {
                        inventoryItem = item;
                    }
                }
                sendSteamNotification(Bot.Admins[0], inventoryItem);
            }
            else if (message.Contains("fuck") || message.Contains("suck") || message.Contains("dick") || message.Contains("cock") || message.Contains("tit") || message.Contains("boob") || message.Contains("pussy") || message.Contains("vagina") || message.Contains("cunt") || message.Contains("penis"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry, but as a robot I cannot perform sexual functions.");
            }

            else if (message.Contains("decline") || message.Contains("accept"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "If I declined a trade it means that I am busy performing calculations (or something). Hold on a second and try again.");
            }

            else if (message.StartsWith("add") || message.StartsWith("list") || message.StartsWith("remove"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "That is a trade chat command. It only works in the trade chat.");
            }

            else if (message.Contains("hi") || message.Contains("hello"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, Bot.ChatResponse);
            }

            else
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry, that is not a valid chat command.");
            }
        }

        public override bool OnTradeRequest()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
            //Bot.log.Info(String.Format("Trade from: {0} ({1})", Bot.SteamFriends.GetFriendPersonaName(OtherSID), OtherSID));
            tradeReady = false;
            return trades;
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Oh, there was an error: " + error + "."
                                              );
            //Bot.log.Warn(error);
        }

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg,
                                              "Sorry, but you were AFK and the trade was canceled.");
            //Bot.log.Info("User was kicked because he was AFK.");
        }

        public override void OnTradeInit()
        {
            Trade.SendMessage(String.Format("Hello {0}! Please wait while I load the pricelist.", Bot.SteamFriends.GetFriendPersonaName(OtherSID)));
            Pricelist.RefreshData();

            myItemValue = 0;
            myKeys = 0;
            myMetal = 0;

            otherItemValue = 0;
            otherKeys = 0;
            otherMetal = 0;

            isBalanced = false;
            donate = false;

            Trade.SendMessage("Done! Add your items, or request items by using the add / remove commands. Type help for more info.");

            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, 
                String.Format("You can also add items by going to http://steamcommunity.com/profiles/{0}/inventory/ and dragging items into this chat window.", 
                Trade.MySteamId.ConvertToUInt64()));

            tradeReady = true;
        }

        public override void OnTradeClose()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thank you for using the Strange Bank! Join our group for updates and occasional giveaways: http://steamcommunity.com/groups/strangebank");
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
            Bot.log.Warn("[USERHANDLER] TRADE CLOSED");
            tradeReady = false;
            Bot.CloseTrade();
        }

        public override void OnTradeSuccess()
        {
            Log.Success("Trade Complete.");
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {
            string reason;
            if (metal.ContainsKey(inventoryItem.Defindex))
                otherMetal += metal[inventoryItem.Defindex];
            else if (inventoryItem.Defindex == 5021)
                otherKeys++;
            else if (ShouldBuy(inventoryItem, schemaItem, out reason) && Pricelist.HasPrice(inventoryItem.Defindex, inventoryItem.Quality))
            {
                Price p = OtherValue(inventoryItem, schemaItem);
                int value = p.Scrap;
                otherItemValue += value;
                Trade.SendMessage(String.Format("Added: {0}. Paying {1} each.", schemaItem.Name, p));
            }
            else
                Trade.SendMessage(String.Format("Item: {0} could not be accepted. Reason: {1}", schemaItem.Name, reason));
            balanceTrade();
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) 
        {
            string reason;
            if (metal.ContainsKey(inventoryItem.Defindex))
                otherMetal -= metal[inventoryItem.Defindex];
            else if (inventoryItem.Defindex == 5021)
                otherKeys--;
            else if (ShouldBuy(inventoryItem, schemaItem, out reason) && Pricelist.HasPrice(inventoryItem.Defindex, inventoryItem.Quality))
                otherItemValue -= OtherValue(inventoryItem, schemaItem).Scrap;
            balanceTrade();
        }

        protected int getNumItems(int defindex, string quality)
        {
            int count = 0;
            Bot.GetInventory();
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                if (item.Defindex == defindex && item.Quality == quality)
                {
                    count++;
                }
            }
            return count;
        }

        private void balanceTrade()
        {
            isBalanced = false;

            if (donate)
            {
                Trade.RemoveAllItems();
                isBalanced = true;
                return;
            }

            int metalNeeded = otherItemValue + otherMetal + otherKeys * Pricelist.Key.Scrap - myItemValue;

            while (myMetal < metalNeeded)
            {
                if (metalNeeded - myMetal >= 9 && Trade.AddItemByDefindex(5002))
                    myMetal += 9;
                else if (metalNeeded - myMetal >= 3 && Trade.AddItemByDefindex(5001))
                    myMetal += 3;
                else if (Trade.AddItemByDefindex(5000))
                    myMetal++;
                else
                {
                    Trade.SendMessage("Error: either I am out of metal or I do not have exact change.");
                    return;
                }
            }
            while (myMetal > metalNeeded)
            {
                if (myMetal - metalNeeded >= 9 && Trade.RemoveItemByDefindex(5002))
                    myMetal -= 9;
                else if (myMetal - metalNeeded >= 3 && Trade.RemoveItemByDefindex(5001))
                    myMetal -= 3;
                else if (Trade.RemoveItemByDefindex(5000))
                    myMetal--;
                else
                {
                    Trade.SendMessage("You must add " + new Price(myMetal - metalNeeded) + " worth of items to balance the trade.");
                    return;
                }
            }

            isBalanced = true;
        }

        public virtual void SendHelpMenu()
        {
            if (Trade != null)
            {
                Trade.SendMessage("--- Help Menu: ---");
                Trade.SendMessage("- add <defindex> (adds an item with the specified definition index)");
                Trade.SendMessage("- add <searchkey> (adds an item whose name contains the specified text)");
                Trade.SendMessage("- remove <defindex> (removes an item with the specified definition index)");
                Trade.SendMessage("- remove <searchkey> (removes an item whose name contains the specified text)");
                Trade.SendMessage("- list (lists all buyable items in my inventory, along with their definition indeces)");
            }
        }

        public virtual void SendInventoryList()
        {
            if (Trade != null)
            {
                Trade.SendMessage("My Inventory:");
                foreach (Inventory.Item item in Trade.MyInventory.Items)
                {
                    Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);

                    string reason;
                    if (ShouldSell(item, Trade.CurrentSchema.GetItem(item.Defindex), out reason) && Pricelist.HasPrice(item.Defindex, item.Quality))
                    {
                        Trade.SendMessage("Name: " + schemaItem.Name + ". Index: " + item.Defindex + ". Price: " + MyValue(item, schemaItem));
                    }
                }
            }
        }

        private void SmeltMetal()
        {
            trades = false;

            Bot.GetInventory();
            List<Inventory.Item> scrap = Bot.MyInventory.GetItemsByDefindex(5000);
            List<Inventory.Item> reclaimed = Bot.MyInventory.GetItemsByDefindex(5001);
            List<Inventory.Item> refined = Bot.MyInventory.GetItemsByDefindex(5002);

            int scrapNeeded = 9 - scrap.Count;
            int reclaimedNeeded = 9 + (scrapNeeded / 3) - reclaimed.Count;

            if (reclaimedNeeded > 0 || scrapNeeded > 0)
            {
                Bot.SetGamePlaying(440);

                int position = 0;
                while (position < refined.Count && reclaimedNeeded > 0)
                {
                    TF2GC.Crafting.CraftItems(Bot, refined[position].Id);
                    reclaimedNeeded -= 3;
                    position++;
                }

                Bot.GetInventory();
                reclaimed = Bot.MyInventory.GetItemsByDefindex(5001);

                while (position < reclaimed.Count && scrapNeeded > 0)
                {
                    TF2GC.Crafting.CraftItems(Bot, reclaimed[position].Id);
                    scrapNeeded -= 3;
                    position++;
                }
            }
            trades = true;
        }

        private bool addItem(string addMessage)
        {
            string[] msg = addMessage.Split(' ');

            if (msg.Length < 2)
                return false;
            string key;
            if (msg[1] == "the" || msg[1] == "strange" || msg[1] == "vintage")
            {
                if (msg.Length < 3)
                    return false;
                key = msg[2];
            }
            else
            {
                key = msg[1];
            }

            int defindex;
            Inventory.Item inventoryItem;

            if (Int32.TryParse(key, out defindex))
            {
                inventoryItem = InventorySearch(defindex);
            }
            else
            {
                inventoryItem = InventorySearch(key);
            }

            if (inventoryItem == null)
            {
                return false;
            }

            Schema.Item schemaItem = Trade.CurrentSchema.GetItem(inventoryItem.Defindex);

            string reason;

            if (ShouldSell(inventoryItem, schemaItem, out reason) && Pricelist.HasPrice(inventoryItem.Defindex, inventoryItem.Quality) && Trade.AddItem(inventoryItem.Id))
            {
                Price value = MyValue(inventoryItem, schemaItem);
                Trade.SendMessage(String.Format("Adding {0}. Cost: {1}", schemaItem.Name, value));
                myItemValue += value.Scrap;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool removeItem(string removeMessage)
        {
            string[] msg = removeMessage.Split(' ');

            if (msg.Length < 2)
                return false;
            string key;
            if (msg[1] == "the" || msg[1] == "strange" || msg[1] == "vintage")
            {
                if (msg.Length < 3)
                    return false;
                key = msg[2];
            }
            else
            {
                key = msg[1];
            }

            int defindex;

            if (Int32.TryParse(key, out defindex))
            {
                foreach (Inventory.Item item in Trade.MyInventory.Items)
                {
                    if (item.Defindex == defindex && Trade.RemoveItem(item.Id))
                    {
                        myItemValue -= MyValue(item, Trade.CurrentSchema.GetItem(item.Defindex)).Scrap;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (Inventory.Item item in Trade.MyInventory.Items)
                {
                    Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                    if (schemaItem.Name.ToLower().Contains(key) && Trade.RemoveItem(item.Id))
                    {
                        myItemValue -= MyValue(item, schemaItem).Scrap;
                        return true;
                    }
                }
                return false;
            }
        }

        public Inventory.Item InventorySearch(string filter)
        {
            Inventory.Item[] items = Trade.MyInventory.Items;
            foreach (Inventory.Item item in items)
            {
                Schema.Item schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                string reason;
                if (ShouldSell(item, schemaItem, out reason) && schemaItem.Name.ToLower().Contains(filter.ToLower()))
                {
                    return item;
                }
            }
            return null;
        }

        public Inventory.Item InventorySearch(int defindex)
        {
            List<Inventory.Item> results = Trade.MyInventory.GetItemsByDefindex(defindex);
            Schema.Item schemaItem = Trade.CurrentSchema.GetItem(defindex);
            foreach (Inventory.Item item in results)
            {
                string reason;
                if (ShouldSell(item, schemaItem, out reason))
                {
                    return item;
                }
            }
            return null;
        }

        /*
        private void sendEmailNotifiaction(string addr, string msg)
        {
            MailMessage message = new MailMessage();
            message.To.Add(addr);
            message.Subject = "Items you requested are now available";
            message.From = new System.Net.Mail.MailAddress("norbert.the.nutjob@gmail.com");
            message.Body = msg;
            SmtpClient smtp = new SmtpClient();
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("norbert.the.nutjob@gmail.com", "luny0731");
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = false;
            smtp.Send(message);
        }
        */

        private void sendSteamNotification(SteamID recipient, Inventory.Item inventoryItem)
        {
            Bot.SteamFriends.SendChatMessage(recipient, EChatEntryType.ChatMsg, "An item you requested are now available: ");
            Schema.Item schemaItem = SteamTrade.Trade.CurrentSchema.GetItem(inventoryItem.Defindex);
            String quality = "unknown";
            if (inventoryItem.Quality == "11")
            {
                quality = "strange";
            }
            else if (inventoryItem.Quality == "6")
            {
                quality = "unique";
            }
            Bot.SteamFriends.SendChatMessage(recipient, EChatEntryType.ChatMsg, String.Format("Item: {0}, Quality: {1}", schemaItem.Name, quality));
        }

        private Schema.Item schemaSearch(string key)
        {
            Schema schema = SteamTrade.Trade.CurrentSchema;

            foreach (Schema.Item schemaItem in schema.Items)
            {
                if (schemaItem.Name.Contains(key))
                {
                    return schemaItem;
                }
            }
            return null;
        }

        public override void OnTradeMessage(string message) 
        {
            if (!tradeReady)
            {
                Trade.SendMessage("Please wait until the pricelist is loaded.");
                return;
            }
            message = message.ToLower().Replace("/", "").Replace("\\", "").Replace("!", "").Replace("<", "").Replace(">", "").Replace("sticky", "pipe").Replace("kukri", "club");
            //Log.Info("Trade message from " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + ": " + message);

            if (message.Equals("help"))
            {
                SendHelpMenu();
            }
            
            else if (message.Equals("list"))
            {
                try
                {
                    SendInventoryList();
                }
                catch (Exception ex)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, ex.Message + ", stack: " + ex.StackTrace);
                }
            }

            else if (message.Equals("donate"))
            {
                donate = true;
                balanceTrade();
            }

            else if (message.Equals("authorize") && IsAdmin)
            {
                isBalanced = true;
            }

            else if (message.StartsWith("addmetal") && IsAdmin)
            {
                string[] msg = message.Split(' ');
                int amount = (int)(Convert.ToDouble(msg[1]) * 9 + 0.5);
                otherMetal += amount;
                balanceTrade();
            }

            else if (message.StartsWith("addkey") && IsAdmin)
            {
                Trade.AddAllItemsByDefindex(5021);
                balanceTrade();
            }

            else if (message.StartsWith("add"))
            {
                if (!donate)
                {
                    if (!addItem(message))
                    {
                        Trade.SendMessage("Sorry, I could not find that item in my inventory.");
                    }
                    balanceTrade();
                }
            }

            else if (message.StartsWith("remove"))
            {
                if (!donate)
                {
                    if (!removeItem(message))
                    {
                        Trade.SendMessage("Sorry, I could not find that item in my offerings.");
                    }
                    balanceTrade();
                }
            }
            else
            {
                Trade.SendMessage("Sorry, that is not a recognized trade command. Type \"help\" if you need assistance.");
            }
        }

        public override void OnTradeReady(bool ready)
        {
            //Because SetReady must use its own version, it's important
            //we poll the trade to make sure everything is up-to-date.
            Trade.Poll();
            if (!ready)
            {
                Trade.SetReady(false);
            }
            else
            {
                if (Validate())
                {
                    Trade.SetReady(true);
                }
            }
        }

        public override void OnTradeAccept()
        {
            if (Validate() || IsAdmin)
            {
                //Even if it is successful, AcceptTrade can fail on
                //trades with a lot of items so we use a try-catch
                try
                {
                    Trade.AcceptTrade();
                }
                catch
                {
                    Log.Warn("The trade might have failed, but we can't be sure.");
                }

                Log.Success("Trade Complete!");
                SmeltMetal();
            }

            OnTradeClose();
        }

        public bool Validate()
        {
            List<string> errors = new List<string>();
            // send the errors
            if (!isBalanced)
                errors.Add("Could not balance the trade.");
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");
            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }

            return errors.Count == 0;
        }

        public bool IsGifted(Inventory.Item inventoryItem)
        {
            try
            {
                foreach (Inventory.ItemAttribute attribute in inventoryItem.Attributes)
                {
                    if (attribute.Defindex == 186)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool ShouldBuy(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            reason = "This bot does not buy items";
            return false;
        }

        public virtual bool ShouldSell(Inventory.Item inventoryItem, Schema.Item schemaItem, out string reason)
        {
            reason = "None";
            return true;
        }

        public virtual Price OtherValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, false);
        }

        public virtual Price MyValue(Inventory.Item inventoryItem, Schema.Item schemaItem)
        {
            return Pricelist.Get(inventoryItem.Defindex, inventoryItem.Quality, true);
        }
    }
}
