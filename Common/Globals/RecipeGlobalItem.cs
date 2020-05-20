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

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<Config>().showTag)
            {
                tooltips.Add(new TooltipLine(this.mod, "tagID", "<" + ResearchFrom14.ItemToTag(item) + ">"));
            }
        }
    }
}
