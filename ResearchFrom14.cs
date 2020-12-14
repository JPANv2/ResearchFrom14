using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using ResearchFrom14.Common;
using ResearchFrom14.Common.UI;
using ResearchFrom14.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ResearchFrom14
{
    public class ResearchFrom14 : Mod
    {
        public static List<int> invalidSetDefaults = new List<int>();
        public Config mainConfig = ModContent.GetInstance<Config>();
        public static ModHotKey hotkey;
        public static ModHotKey preHotkey;
        public UserInterface purchaseUI;
        public UserInterface prefixUI;
        public ResearchUI ui;
        public PrefixUI preUI;


        public ResearchFrom14()
        {

        }

        public override void Load()
        {
            base.Load();
            ResearchTable.ClearTable();
            invalidSetDefaults.Clear();
            if (!Main.dedServ)
            {
                purchaseUI = new UserInterface();
                ui = new ResearchUI();
                ui.Activate();
                purchaseUI.SetState(ui);
                hotkey = RegisterHotKey("Open or Close ResearchUI", "Add");
                prefixUI = new UserInterface();
                preUI = new PrefixUI();
                preUI.Activate();
                prefixUI.SetState(preUI);
                preHotkey = RegisterHotKey("Open or Close Prefix Assignment", "Subtract");
                    
            }
        }

        public override void Unload()
        {
            ResearchTable.ClearTable();
            invalidSetDefaults.Clear();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            base.UpdateUI(gameTime);
            if (purchaseUI?.CurrentState != null)
            {
                if (ResearchUI.visible)
                {
                    purchaseUI.Update(gameTime);
                }
            }
            if (prefixUI?.CurrentState != null)
            {
                if (PrefixUI.visible)
                {
                    prefixUI.Update(gameTime);
                }
            }
        }


        public void ActivatePurchaseUI(int player)
        {
            if (player == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                
                ui?.setVisible(true);
                Main.playerInventory = true;
                Main.recBigList = false;
                if (ModContent.GetInstance<ExceptionListConfig>().forceReload)
                {
                    ResearchTable.InitResearchTable();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket pk = this.GetPacket();
                        pk.Write((byte)99);
                        pk.Write((byte)Main.myPlayer);
                        pk.Send(-1, -1);
                        Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().RebuildCache();
                        ModContent.GetInstance<ExceptionListConfig>().forceReload = false;
                    }
                    else
                    {
                        ModContent.GetInstance<ExceptionListConfig>().forceReload = false;
                        SaveListConfig();
                    }
                    
                }
            }
        }

        public void ActivatePrefixUI(int player)
        {
            if (player == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {

                preUI?.setVisible(true);
                Main.playerInventory = true;
                Main.recBigList = false;
            }
        }

        public static void SaveListConfig()
        {
            Type typeOfManager = typeof(ConfigManager);
            MethodInfo info = typeOfManager.GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Static);
            info.Invoke(null, new object[] { ModContent.GetInstance<ExceptionListConfig>() });
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "ResearchFrom14: ResearchUI",
                    delegate
                    {
                        if (!Main.playerInventory)
                            ResearchUI.visible = false;

                        if (ResearchUI.visible)
                        {
                            ui.Draw(Main.spriteBatch);
                        }
                        return true;
                    },
                       InterfaceScaleType.UI));

                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "ResearchFrom14: PrefixUI",
                    delegate
                    {
                        if (!Main.playerInventory)
                            PrefixUI.visible = false;

                        if (PrefixUI.visible)
                        {
                            preUI.Draw(Main.spriteBatch);
                        }
                        return true;
                    },
                       InterfaceScaleType.UI));
            }
        }

        public static int getTypeFromTag(string tag)
        {
            int type = 0;
            if (!Int32.TryParse(tag, out type))
            {
                Mod m = ModLoader.GetMod(tag.Split(':')[0]);
                if (m != null)
                    type = m.ItemType(tag.Split(':')[1]);
            }
            return type;
        }

        public override void PostAddRecipes()
        {
            
        }

        public static Item getItemFromTag(string tag, bool noMatCheck = false)
        {
            Item ret = new Item();
            int type = getTypeFromTag(tag);
            if (type != 0)
                ret.SetDefaults(type, noMatCheck);
            return ret;
        }

        public static string ItemToTag(Item itm)
        {
            String type = "" + itm.type;
            if (itm.modItem != null)
            {
                type = itm.modItem.mod.Name + ":" + itm.modItem.Name;
            }

            return type;
        }
        public static string ItemIDToTag(int id)
        {
            Item itm = new Item();
            try
            {
                if(invalidSetDefaults.Contains(id))
                    return "";
                itm.SetDefaults(id, true);
            }
            catch (Exception ex)
            {
                ModLoader.GetMod("ResearchFrom14").Logger.Warn("Item id: " + id + " Threw an exception on SetDefaults:\n" + ex.StackTrace);
                invalidSetDefaults.Add(id);
                return "";
            }
            
            return ItemToTag(itm);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int messageID = reader.ReadByte();
            Logger.Info("Received Message id " + messageID);
            /*message 0: Share research with server (not used)
            int Player
            TAGCompound Research*/
            if (messageID == 0)
            {
                int player = reader.ReadByte();
                if (Main.netMode == NetmodeID.Server || player != Main.myPlayer)
                {
                    TagCompound tempResearch = TagIO.Read(reader);
                    foreach (KeyValuePair<string, object> read in tempResearch)
                    {
                        Main.player[player].GetModPlayer<ResearchPlayer>().research[read.Key] = tempResearch.GetAsInt(read.Key);
                    }
                    Main.player[player].GetModPlayer<ResearchPlayer>().dirtyCache = true;
                    Main.player[player].GetModPlayer<ResearchPlayer>().RebuildCache();
                    return;
                }
            }
            /*message 1: dirty cache
             int player
            refreshes the cache*/
            if (messageID == 1)
            {
                int player = reader.ReadByte();
                Main.player[player].GetModPlayer<ResearchPlayer>().dirtyCache = true;
                Main.player[player].GetModPlayer<ResearchPlayer>().RebuildCache();
                return;
            }
            /*message 10 : P2P item cache sharing
             byte player 
             int count
             int[count] itemIDs
            Share the cache with another player. Meant to share multiple, fully researched items at once.
            Will send another such message to the sender (with this whoAmI) with the researched items,
            and will keep sending them to one another until the cache is equal*/
            if(messageID == 10) //Shared item Cache
            {
                
                ResearchPlayer rp = Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>();
                int sendTo = reader.ReadByte();
                //Logger.Info("Received shared cache Message to "+i);
                List<int> oldCache = new List<int>();
                oldCache.AddRange(rp.researchedCache);

                int count = reader.ReadInt32();
                for(int i = 0; i< count; i++)
                {
                    int id = reader.ReadInt32();
                    if (!rp.IsResearched(id))
                    {
                        rp.AddResearchedAmount(id, ResearchTable.GetTotalResearch(id));
                        rp.CheckRecipesForItem(id);
                    }
                }
                if(Main.netMode == NetmodeID.Server || sendTo != Main.myPlayer)
                {
                    rp.shareCacheTo(sendTo);
                }
                return;
            }
            /*message 11: Share single item with intermediate result
             byte player to send to
            byte player that sent the message
            int item
            int research count

            Adds the research count to the player's research, and replies with the same message with its previous research count
             */
            if (messageID == 11)
            {
                ResearchPlayer rp = Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>();
                int sendTo = reader.ReadByte();
                int orgSender = reader.ReadByte();
                int itm = reader.ReadInt32();
                int amount = reader.ReadInt32();
                int prevResearch = rp.GetResearchedAmount(itm);
                int res = rp.AddResearchedAmount(itm, amount);
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket pk = GetPacket();
                    pk.Write((byte)11);
                    pk.Write((byte)(sendTo));
                    pk.Write((byte)(orgSender));
                    pk.Write(itm);
                    pk.Write(amount);
                    pk.Send(sendTo);
                    return;
                }
                //Logger.Info("Received share single item Message from " + sender + ". Item " + itm + " with amount "+ amount);
                
                if (rp.IsResearched(itm) && res != amount)
                {
                    rp.CheckRecipesForItem(itm);
                  
                    //Logger.Info("Item "+ itm + " was fully researched with " + amount);
                    rp.RebuildCache();
                    if (orgSender == Main.myPlayer)
                        return;
                    ModPacket pk = GetPacket();
                    pk.Write((byte)11);
                    pk.Write((byte)(orgSender));
                    pk.Write((byte)(orgSender));
                    pk.Write(itm);
                    pk.Write(prevResearch + amount);
                    pk.Send();
                }
                else
                {
                    //Logger.Info("Item " + itm + " was not fully researched with " + amount);
                }
                return;
            }
            /*message 12: Share prefixes
             byte player to send to
            int itm
            int count
            byte[count] prefixes
            
             Sends all prefixes for item itm, with no replies. Only adds prefixes.*/
            if (messageID == 12)
            {
                ResearchPlayer rp = Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>();
                int sendTo = reader.ReadByte();
                int itm = reader.ReadInt32();
                int count = reader.ReadInt32();
                List<byte> prefixes = new List<byte>();
                for(int i = 0; i < count; i++)
                {
                    byte p = reader.ReadByte();
                    prefixes.Add(p);
                    rp.AddResearchPrefix(itm, p);
                }
                if(Main.netMode == NetmodeID.Server)
                {
                    ModPacket pk = GetPacket();
                    pk.Write((byte)12);
                    pk.Write((byte)(sendTo));

                    pk.Write(prefixes.Count);
                    for (int k = 0; k < prefixes.Count; k++)
                        pk.Write((byte)(prefixes[k]));
                    pk.Send(sendTo);
                }
                return;
            }
         
            if (messageID == 99)
            {
                int player = reader.ReadByte();
                ResearchTable.InitResearchTable();
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket pk = this.GetPacket();
                    pk.Write((byte)99);
                    pk.Write((byte)player);
                    pk.Send(-1, player);
                    ModContent.GetInstance<ExceptionListConfig>().forceReload = false;
                    SaveListConfig();
                }
                else
                {
                    Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().RebuildCache();
                }
                ModContent.GetInstance<ExceptionListConfig>().forceReload = false;
                return;
            }
        }

        internal static bool PlaceInInventory(Player player, Item item)
        {
            if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
            {
                player.SellItem(item.type == ItemID.PlatinumCoin ? 5000000 : item.type == ItemID.GoldCoin ? 50000 : item.type == ItemID.SilverCoin ? 500: 5, item.stack);
                return true;
            }

            for (int i = 0; i< 58; i++)
            {
                if(player.inventory[i].type  == item.type && player.inventory[i].maxStack< player.inventory[i].stack)
                {
                    if(item.stack +player.inventory[i].stack <= player.inventory[i].maxStack)
                    {
                        player.inventory[i].stack += item.stack;
                        item.stack = 0;
                        item.TurnToAir();
                        return true;
                    }
                    else
                    {
                        item.stack -= (player.inventory[i].maxStack - player.inventory[i].stack);
                        player.inventory[i].stack = player.inventory[i].maxStack;
                    }
                }
            }
            if (item.ammo != AmmoID.None)
            {
                for (int i = 54; i < 58; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item;
                        return true;
                    }
                }
            }
            for (int i = 49; i >= 0; i--)
            {
                if (player.inventory[i].IsAir)
                {
                    player.inventory[i] = item;
                    return true;
                }
            }
            return false;
        }

        public override object Call(params object[] args)
        {
            if (args.Length == 0 || !(args[0] is string) || (((args[0]) as string) == null))
            {
                Logger.Info("Mod call was Empty. This will display, in log, what functions are currently available. All function calls are case-insensitive, but arguments are not.");
                Logger.Info("Player arguments accept the Player object or the Player.whoAmI (position in the Main.player array)");
                Logger.Info("Item arguments take Item objects, ModItem object, the int Type of that item or the Item Tag of the item)");
                Logger.Info("Int values accept int or its string representation");

                Logger.Info("Functions available");
                Logger.Info(" - IsResearched (Player, Item) ");
                Logger.Info("Returns -1 if the item is irresearchable, 0 if it is totally researched or the number of items remaining before becoming fully researched.");
                Logger.Info(" - AddResearch (Player, Item, Int) ");
                Logger.Info("Adds Int to the researched amount of Item for the given Player. Send Int32.Max for fully research the item regardless of current amount. This will not research items considered unresearchable. Returns the research amount remaining after adding.");
                Logger.Info(" - SetDefaultMaxResearch (Item, Int) ");
                Logger.Info("Sets the default, starting value for the specified item's research needed. If this value is -1, makes it impossible to research, 0 makes this mod's algorithm decide, positive values are set as the amount needed.\n" +
                    "Note: This function does not alter the values outright. After finishing calling all the SetDefault* you need, call ResetTable.");
                Logger.Info(" - SetDefaultCategories (Item, List<string>) or SetDefaultCategories (Item, string ...) ");
                Logger.Info("Sets the default category to a custom value. This does not stop it from showing up in other categories, it's just a way to make custom categories for mods possible.\n" +
                    "Note: This function does not alter the values outright. After finishing calling all the SetDefault* you need, call ResetTable.");
                Logger.Info(" - ResetTable() ");
                Logger.Info("Resets the table, applying the new defaults that have been set. This function is required to apply the changes made by the two SetDefault functions. Use it at the end of all your Item Changes.");
                return null;
            }
                
            string function = args[0] as string;

            /* IsResearched 
             - Params 
               - Player
               - item
             - Returns -1 if unresearchable, 0 if researched, the remaining items to reseach otherwise.*/
            if (function.ToLower().Equals("isresearched"))
            {
                if (args.Length != 3)
                {
                    Logger.Error("Error in ModCall IsResearched: Invalid parameter number (Player, Item)");
                    return null;
                }
                Player p = getPlayerFromObject(args[1]);
                if (p == null)
                {
                    Logger.Error("Error in ModCall IsResearched: Player is null (means not really a Player or a player.whoAmI)");
                    return null;
                }
                    
                int? itemID = getItemTypeFromObject(args[2]);
                if(itemID == null)
                {
                    Logger.Error("Error in ModCall IsResearched: Item is null (means not really an Item, ModItem, int or valid ItemTag string)");
                    return null;
                }
                if(itemID.Value == 0)
                {
                    Logger.Error("Error in ModCall IsResearched: Item type is 0. This value is not valid (probably invalid ItemTag string)");
                    return null;
                }

                ResearchPlayer player = p.GetModPlayer<ResearchPlayer>();
                int researched = player.GetResearchedAmount(itemID.Value);
                int total = ResearchTable.GetTotalResearch(itemID.Value);
                if (total < 1)
                    return -1;
                if (researched <= total)
                    return total - researched;
                return 0;
            }
            /* AddResearch
              - Params 
                - Player
                - item
                - amount
              - Adds research amount to item for Player*/
            if (function.ToLower().Equals("addresearch"))
            {
                if (args.Length != 4)
                {
                    Logger.Error("Error in ModCall AddResearch: Invalid parameter number (Player, Item, int amount)");
                    return null;
                }
                Player p = getPlayerFromObject(args[1]);
                if (p == null)
                {
                    Logger.Error("Error in ModCall AddResearch: Player is null (means not really a Player or a player.whoAmI)");
                    return null;
                }

                int? itemID = getItemTypeFromObject(args[2]);
                if (itemID == null)
                {
                    Logger.Error("Error in ModCall AddResearch: Item is null (means not really an Item, ModItem, int or valid ItemTag string)");
                    return null;
                }
                if (itemID.Value == 0)
                {
                    Logger.Error("Error in ModCall AddResearch: Item type is 0. This value is not valid (probably invalid ItemTag string)");
                    return null;
                }

                int? amount = getIntFromObject(args[3]);
                if(amount == null)
                {
                    Logger.Error("Error in ModCall AddResearch: amount is null (means not really an int or an int-parsable string)");
                    return null;
                }

                ResearchPlayer player = p.GetModPlayer<ResearchPlayer>();
                if (ResearchTable.GetTotalResearch(itemID.Value) <= 0)
                    return -1;
                try
                {
                    Item itm = new Item();
                    itm.SetDefaults(itemID.Value);
                    itm.stack = amount.Value == Int32.MaxValue ? (amount.Value - 1000): amount.Value;
                    int total = player.AddResearchedAmount(itm);
                    return total;
                }catch (Exception ex)
                {
                    Logger.Error("Error in ModCall AddResearch: Item cannot be SetDefault");
                }
                return null;
            }

            /* SetDefaultMaxResearch
            - Params 
              - item
              - int amount
             Sets the default value for the required research for the given item to become infinite
            - Returns nothing*/
            if (function.ToLower().Equals("setdefaultmaxresearch"))
            {
                if (args.Length != 3)
                {
                    Logger.Error("Error in ModCall SetDefaultMaxResearch: Invalid parameter number (Item, int)");
                    return null;
                }
                
                int? itemID = getItemTypeFromObject(args[1]);
                if (itemID == null)
                {
                    Logger.Error("Error in ModCall SetDefaultMaxResearch: Item is null (means not really an Item, ModItem, int or valid ItemTag string)");
                    return null;
                }
                if (itemID.Value == 0)
                {
                    Logger.Error("Error in ModCall SetDefaultMaxResearch: Item type is 0. This value is not valid (probably invalid ItemTag string)");
                    return null;
                }

                int? amount = getIntFromObject(args[2]);
                if (amount == null)
                {
                    Logger.Error("Error in ModCall SetDefaultMaxResearch: amount is null (means not really an int or an int-parsable string)");
                    return null;
                }

                ResearchTable.defaultValues[itemID.Value] = amount.Value;
                return ResearchTable.defaultValues[itemID.Value];
            }
            /* SetDefaultCategories
            - Params 
              - item
              - List<string> or string...
             Sets the default categories for the given item
            - Returns true if successfully added category*/
            if (function.ToLower().Equals("setdefaultcategories"))
            {
                if (args.Length < 3)
                {
                    Logger.Error("Error in ModCall SetDefaultCategories: Invalid parameter number (Item, List<string> or string...)");
                    return null;
                }
                int? itemID = getItemTypeFromObject(args[1]);
                if (itemID == null)
                {
                    Logger.Error("Error in ModCall SetDefaultCategories: Item is null (means not really an Item, ModItem, int or valid ItemTag string)");
                    return null;
                }
                if (itemID.Value == 0)
                {
                    Logger.Error("Error in ModCall SetDefaultCategories: Item type is 0. This value is not valid (probably invalid ItemTag string)");
                    return null;
                }

                List<string> cats = getListOfStringFromObjects(args, 2);
                if(cats.Count == 0)
                {
                    Logger.Error("Error in ModCall SetDefaultCategories: no strings found");
                    return null;
                }
                if (ResearchTable.defaultCategories.ContainsKey(itemID.Value))
                {
                    ResearchTable.defaultCategories[itemID.Value].AddRange(cats);
                }
                else
                {
                    ResearchTable.defaultCategories[itemID.Value] = cats;
                }
                return true;
            }
            if (function.ToLower().Equals("resettable"))
            {
                ResearchTable.InitResearchTable();
            }

            Logger.Error("Error in ModCall: function \"" +function +"\" does not exist.");
            return null;
        }

        private List<string> getListOfStringFromObjects(object[] args, int v)
        {
            List<string> ans = new List<string>();
            for(int i = v; i < args.Length; i++)
            {
                if(args[i] is List<string>)
                {
                    ans.AddRange((args[i]) as List<string>);
                    continue;
                }else if (args[i] is string[])
                {
                    ans.AddRange((args[i]) as string[]);
                    continue;
                }else if(args[i] is string)
                {
                    ans.Add((args[i]) as string);
                    continue;
                }
                else
                {
                    
                }
            }
            return ans;
        }

        private int? getIntFromObject(object o)
        {
            if (o == null)
                return null;
            if (o as int? != null)
                return o as int?;
            if(o as string != null)
            {
                int res;
                if (Int32.TryParse(o as string, out res))
                    return res;
            }
            return null;
        }

        private int? getItemTypeFromObject(object o)
        {
            if (o == null)
                return null;
            if (o is string)
                return getTypeFromTag(o as string);
            else if (o is Item)
                return (o as Item).type;
            else if (o is ModItem)
                return (o as ModItem).item.type;
            else if (o is int)
                return (int)o;
            return null;
        }

        private Player getPlayerFromObject(object o)
        {
            if (o == null)
                return null;
            if (o is int && ((int)o < 256))
                return Main.player[(int)o];
            if (o is Player)
                return o as Player;
            return null;
        }
    }
}
