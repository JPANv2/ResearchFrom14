using System.Collections.Generic;
using System.IO;
using ResearchFrom14.Common;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ResearchFrom14.Items
{
    public class ResearchErasingBook : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Research Erasing Book");
            Tooltip.SetDefault("A book that will make you forget all that you researched. Right-click it to forget.");
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
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            rp.research = new TagCompound();
            rp.researchedCache.Clear();
            rp.AddAllResearchedItems(new List<int>() { ModContent.ItemType<ResearchErasingBook>(), ModContent.ItemType<ResearchSharingBook>() });

            item.stack--;
        }
    }
}
