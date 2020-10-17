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
        [Tooltip("Percentage of the base number of items one must research to unlock the infinite item. Research ammount will be rounded up, minimum 1 item.")]       
        [DefaultValue(100)]
        [Range(1,1000000)]
        [ReloadRequired]
        public int difficulty;

        [Label("Research Difficult affects Item exceptions")]
        [Tooltip("If false, items in the exception list will not be multiplied by the difficulty multiplier. Default to true.")]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool difficultyAffectsExceptions;

        [Label("Research when shift-clicked")]
        [Tooltip("When you shift-click an item into the Research slot, automatically research it, instead of having to manually click the research button.")]
        [DefaultValue(false)]
        public bool autoShiftResearch;

        [Label("Auto-Trash picked up Researched items")]
        [Tooltip("Automatically deletes items that are researched when picked up. Pick up priority is Mod Name Based, so they may be caught up by other mods. Researched items with prefixes you have not researched yet will not be destroyed.")]
        [DefaultValue(false)]
        public bool autoTrashResearched;

        [Label("Auto-Trash research prefixes")]
        [Tooltip("If auto-trash is on, and you pick up a weapon that has a new prefix, it auto-researches that prefix and deletes the weapon.")]
        [DefaultValue(false)]
        public bool autoTrashResearchPrefix;

        [Label("Craft from Researched stations")]
        [Tooltip("If true, will allow use of the researched crafting stations as if they were around the player.")]
        [DefaultValue(false)]
        public bool allowCraftFromResearch;

        [Label("Tentative Summon Item Detection Drop")]
        [Tooltip("Most boss summon items have a distinct pattern that, while not exclusive to summon items, are a pretty good indication of them. Disable this if items used by holding are being given a boss summon value.")]
        [DefaultValue(true)]
        public bool summonDetection;

        [Label("Researched Ammo is infinite")]
        [Tooltip("If you have researched a type of ammo, that ammo is considered infinte, and will not be consumed.")]
        [DefaultValue(true)]
        public bool infiniteAmmo;

        [Label("Researched Consumables are infinite")]
        [Tooltip("If you have researched a type of consumable item (such as blocks, potions, furniture, etc...), that item and will not be consumed when used.")]
        [DefaultValue(true)]
        public bool infiniteItems;

        [Label("Hotkey works outside inventory")]
        [Tooltip("Shows the research button whenever you press the button, opening the inventory if it was not open. Required true to work with autopause on.")]
        [DefaultValue(false)]
        public bool buttonAlwaysOn;

        [Label("Display Research amount on item tooltip")]
        [Tooltip("Shows how much research is reqiired for the selected item to be fully researched.")]
        [DefaultValue(true)]
        public bool showResearch;

        [Label("Display Researched! tooltip")]
        [Tooltip("Shows \"Researched!\" on the item if it was already researched. Make false to ignore it.")]
        [DefaultValue(true)]
        public bool showResearched;

        [Label("Display ItemTag on item tooltip")]
        [Tooltip("Shows the mod and item internal name below the item tooltip. Use this if you want to find out how to add an exception to the research value to an item.")]
        [DefaultValue(false)]
        public bool showTag;

    }
}
