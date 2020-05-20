using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ResearchFrom14.Configs
{
    public class ExceptionListConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        
        [Label("Custom item values")]
        [Tooltip("Custom research values for items are entered here.\nWrite the ItemID (for Vanilla Terraria items only) or the ItemTag string.\nSet a item to a negative value to prevent it from being researchable.")]
        public Dictionary<String, ExceptionListEntry> customItemValues = new Dictionary<string, ExceptionListEntry>();
    }

    public class ExceptionListEntry
    {
        public int value = 0;
        public List<String> categories = new List<string>();
    }
}
