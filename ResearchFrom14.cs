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
                itm.SetDefaults(id, true);
            }
            catch (Exception ex)
            {
                ModLoader.GetMod("ResearchFrom14").Logger.Warn("Item id: " + id + " Threw an exception on SetDefaults:\n" + ex.StackTrace);
                return "";
            }
            
            return ItemToTag(itm);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            int messageID = reader.ReadByte();
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
            if (messageID == 1)
            {
                int player = reader.ReadByte();
                Main.player[player].GetModPlayer<ResearchPlayer>().dirtyCache = true;
                Main.player[player].GetModPlayer<ResearchPlayer>().RebuildCache();
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
    }
}
