using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ResearchFrom14.Common;
using ResearchFrom14.Common.UI;
using ResearchFrom14.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ResearchFrom14
{
    public class ResearchFrom14 : Mod
    {

        public Config mainConfig = ModContent.GetInstance<Config>();
        public static ModHotKey hotkey;
        public UserInterface purchaseUI;

        public ResearchUI ui;

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
        }


        public void ActivatePurchaseUI(int player)
        {
            if (player == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                ui?.setVisible(true);
                Main.playerInventory = true;
                Main.recBigList = false;
            }
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
            itm.SetDefaults(id, true);
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
                    Main.player[player].GetModPlayer<ResearchPlayer>().research = TagIO.Read(reader);
                    return;
                }
            }

        }
    }
}