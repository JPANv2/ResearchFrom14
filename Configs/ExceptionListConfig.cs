using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ResearchFrom14.Configs
{
    public class ExceptionListConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [Label("Force-reload Values")]
        [Tooltip("Toggling this to true will make it so the research values input below will be re-applied without restarting the game, as soon as the research ui is opened. Will disable itself afterwards.")]
        [DefaultValue(false)]
        public bool forceReload;

        [Label("Custom item values")]
        [Tooltip("Custom research values for items are entered here.\nWrite the ItemID (for Vanilla Terraria items only) or the ItemTag string.\nSet a item value to zero to prevent it from being researchable.")]
        public Dictionary<string, ExceptionListEntry> customItemValues = new Dictionary<string, ExceptionListEntry>();
    }

    public class ExceptionListEntry
    {
        public int value = -1;
        public List<string> categories = new List<string>();
    }
}
