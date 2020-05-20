using System.Collections.Generic;
using ResearchFrom14.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ResearchFrom14.Common.Globals
{
    public class ResearchGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity
        {
            get
            {
                return false;
            }
        }

        public override bool CloneNewInstances
        {
            get
            {
                return false;
            }
        }

        public override bool ConsumeAmmo(Item item, Player player)
        {
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            if(item.ammo > 0 && rp.IsResearched(item))
            {
                return false;
            }
            return base.ConsumeAmmo(item, player);
        }

        public override bool ConsumeItem(Item item, Player player)
        {
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            if (ModContent.GetInstance<Config>().infiniteItems && rp.IsResearched(item))
            {
                return false;
            }
            return base.ConsumeItem(item, player);
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<Config>().showTag)
            {
                tooltips.Add(new TooltipLine(this.mod, "tagID", "<" + ResearchFrom14.ItemToTag(item) + ">"));
            }
        }
    }
}
