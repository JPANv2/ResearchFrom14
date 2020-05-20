using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ResearchFrom14.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Research all craftables at once")]
        [Tooltip("When you research an item or crafting table, learns all items that can be crafted with it and the available researched items/crafting tables as well.")]
        [DefaultValue(true)]
        public bool researchRecipes;


        [Label("Research Difficulty")]
        [Tooltip("Multiplier for the number of items one must research to unlock the infinite item. Research ammount will be rounded up, minimum 1 item.")]
        [Range(0.0f, 100f)]
        [Increment(.1f)]
        [DrawTicks]
        [DefaultValue(1.0f)]
        public float difficulty;

        [Label("Research Difficult affects Item exceptions")]
        [Tooltip("If false, items in the exception list will not be multiplied by the difficulty multiplier. Default to true.")]
        [DefaultValue(true)]
        public bool difficultyAffectsExceptions;


        [Label("Tentative Summon Item Detection Drop")]
        [Tooltip("Most boss summon items have a distinct pattern that, while not exclusive to summon items, are a pretty good indication of them. Disable this if items used by holding are being given a boss summon value.")]
        [DefaultValue(true)]
        public bool summonDetection;

        [Label("Display ItemTag on item tooltip")]
        [Tooltip("Shows the mod and item internal name below the item tooltip. Use this if you want to find out how to add an exception to the research value to an item.")]
        [DefaultValue(false)]
        public bool showTag;

    }
}
