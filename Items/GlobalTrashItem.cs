using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchFrom14.Common;
using ResearchFrom14.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ResearchFrom14.Items
{
    public class GlobalTrashItem : GlobalItem
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

        public override bool ItemSpace(Item item, Player player)
        {
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            if (ModContent.GetInstance<TrashConfig>().autoTrashResearched && Main.netMode != NetmodeID.Server && rp.IsResearched(item))
            {
                return true;
            }
            return base.ItemSpace(item,player);
        }

        public override bool CanPickup(Item item, Player player)
        {
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            if (ModContent.GetInstance<TrashConfig>().autoTrashResearched && Main.netMode != NetmodeID.Server && !isItemNotTrasheable(item)) {
                if (rp.IsResearched(item))
                {
                    item.stack = 0;
                    //item.TurnToAir();
                    return true;
                }else if (ModContent.GetInstance<TrashConfig>().autoTrashResearchPrefix && rp.IsResearched(item.type))
                {
                    Item curDestroy = rp.destroyingItem;
                    Item clone = item.DeepClone();
                    rp.destroyingItem = clone;
                    rp.Research();
                    item.stack = 0;
                    rp.destroyingItem = curDestroy;
                    return true;
                }
            }
            return base.CanPickup(item, player);
        }

        public virtual bool isItemNotTrasheable(Item item)
        {
            if (item.type == ModContent.ItemType<ResearchSharingBook>())
                return true;
            if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
                return true;
            if(item.type == ItemID.DD2EnergyCrystal) //etherian mana
                return true;

            foreach(string k in ModContent.GetInstance<TrashConfig>().exceptionList)
            {
                if (item.type == ResearchFrom14.getTypeFromTag(k))
                    return true;
            }
            return false;
        }

        public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            if (((ResearchFrom14)mod).ui.recipes.tooltipSearch && ((ResearchFrom14)mod).ui.recipes.isSearching)
            {
                ((ResearchFrom14)mod).ui.recipes.mouseTooltip = "";
                foreach (TooltipLine line in lines)
                {
                    ((ResearchFrom14)mod).ui.recipes.mouseTooltip += line.text + "\n";
                }
                return false;
            }
            return base.PreDrawTooltip(item, lines, ref x, ref y);
        }
    }
}
