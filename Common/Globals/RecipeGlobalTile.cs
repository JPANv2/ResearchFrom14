using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using ResearchFrom14.Configs;
using Terraria.ModLoader;

namespace ResearchFrom14.Common.Globals
{
    class RecipeGlobalTile : GlobalTile
    {
        public override int[] AdjTiles(int type)
        {
            if (ModContent.GetInstance<Config>().allowCraftFromResearch && !Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().rebuildingCache)
            {
                return Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().researchedTileCache.ToArray();
            }
            return base.AdjTiles(type);
        }
    }
}
