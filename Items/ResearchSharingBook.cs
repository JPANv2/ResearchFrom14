using System.Collections.Generic;
using System.IO;
using ResearchFrom14.Common;
using Terraria;
using Terraria.ModLoader;

namespace ResearchFrom14.Items
{
    public class ResearchSharingBook : ModItem
    {
        public override bool CloneNewInstances => true;

        public List<int> knowledge = null;
        public string playerName = null;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Research Sharing Book");
            Tooltip.SetDefault("A book containing all your knowledge at time of creation. If someone else right-clicks it, it will learn all that you researched.");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.value = 0;
            item.maxStack = 1;
        }

        public override bool CanRightClick()
        {
            return true;
        }
        public override void RightClick(Player player)
        {
            if (knowledge == null || playerName == null)
                return;
            Main.NewText("Knows " + knowledge.Count + " items.");
            if (player.name.Equals(playerName))
                return;
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            rp.AddAllResearchedItems(knowledge);

            item.stack--;
        }

        public override ModItem Clone()
        {
            ResearchSharingBook itm = (ResearchSharingBook)this.MemberwiseClone();
            itm.knowledge = knowledge;
            itm.playerName = playerName;
            return itm;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(playerName == null ? "" : playerName);
            if (knowledge != null)
            {
                writer.Write(knowledge.Count);
                foreach (int itm in knowledge)
                {
                    writer.Write(itm);
                }
            }
            else
            {
                writer.Write(0);
            }
            base.NetSend(writer);
        }

        public override void NetRecieve(BinaryReader reader)
        {
            playerName = reader.ReadString();
            int cnt = reader.ReadInt32();
            knowledge = new List<int>();
            for(int i = 0; i < cnt; i++)
            {
                knowledge.Add(reader.ReadInt32());
            }
        }
    }
}
