using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ResearchFrom14.Configs
{
    public class TrashConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Auto-Trash picked up Researched items")]
        [Tooltip("Automatically deletes items that are researched when picked up. Pick up priority is Mod Name Based, so they may be caught up by other mods. Researched items with prefixes you have not researched yet will not be destroyed.")]
        [DefaultValue(false)]
        public bool autoTrashResearched;

        [Label("Auto-Trash research prefixes")]
        [Tooltip("If auto-trash is on, and you pick up a weapon that has a new prefix, it auto-researches that prefix and deletes the weapon.")]
        [DefaultValue(false)]
        public bool autoTrashResearchPrefix;

        [Label("Auto-trash Blacklist")]
        [Tooltip("Place the tags of the items you don't want to trash in this list.")]
        public List<string> exceptionList = new List<string>();
    }

}
