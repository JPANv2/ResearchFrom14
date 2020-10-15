using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResearchFrom14.Common;
using ResearchFrom14.Configs;
using Terraria;
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
            if (ModContent.GetInstance<Config>().autoTrashResearched && rp.IsResearched(item))
            {
                return true;
            }
            return base.ItemSpace(item,player);
        }

        public override bool CanPickup(Item item, Player player)
        {
            ResearchPlayer rp = player.GetModPlayer<ResearchPlayer>();
            if (ModContent.GetInstance<Config>().autoTrashResearched && rp.IsResearched(item))
            {
                item.stack = 0;
                item.TurnToAir();
                return true;
            }
            return base.CanPickup(item, player);
        }
    }
}
