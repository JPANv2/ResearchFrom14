
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ResearchFrom14.Common.UI;
using ResearchFrom14.Configs;
using ResearchFrom14.Items;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ResearchFrom14.Common
{
    public class ResearchPlayer : ModPlayer
    {
        public TagCompound research = new TagCompound();
        public Item destroyingItem = new Item();
        public List<int> researchedCache = new List<int>();
        public List<int> researchedTileCache = new List<int>();
        public bool[] researchedTileAdj = new bool[TileLoader.TileCount];

        private Task populateCache = null;
        public bool dirtyCache = false;
        public bool rebuildingCache = false;
        public bool IsResearched(String itemTag)
        {
            int res = ResearchTable.GetTotalResearch(itemTag);
            if (res < 1)
                return false;
            return GetResearchedAmount(itemTag) >= res;
        }

        public bool IsResearched(Item item)
        {
            int res = ResearchTable.GetTotalResearch(item);
            if (res < 1)
                return false;
            return GetResearchedAmount(item) >= res;
        }

        public bool IsResearched(int itemType)
        {
            int res = ResearchTable.GetTotalResearch(itemType);
            if (res < 1)
                return false;
            return GetResearchedAmount(itemType) >= res;
        }

        public int GetResearchedAmount(String itemTag)
        {
            if (research.ContainsKey(itemTag))
                return research.GetAsInt(itemTag);
            return 0;
        }

        public int GetResearchedAmount(Item realItem)
        {
            return GetResearchedAmount(ResearchFrom14.ItemToTag(realItem));
        }

        public int GetResearchedAmount(int type)
        {
            return GetResearchedAmount(ResearchFrom14.ItemIDToTag(type));
        }

        

        public int AddResearchedAmount(String itemTag, int amount)
        {
            int available = 0;
            if (research.ContainsKey(itemTag))
                available = research.GetAsInt(itemTag);

            int max = ResearchTable.GetTotalResearch(itemTag);
            if (available >= max)
                return amount;
            if(available + amount > max)
            {
                research[itemTag] = max;
                return (amount + available) - max;
            }
            else
            { 
              research[itemTag] = available + amount;
              return 0;
            }
        }

        public int AddResearchedAmount(Item realItem)
        {
            return AddResearchedAmount(ResearchFrom14.ItemToTag(realItem), realItem.stack);
        }

        public int AddResearchedAmount(int type, int amount)
        {
            return AddResearchedAmount(ResearchFrom14.ItemIDToTag(type), amount);
        }

        public void refreshResearch()
        {
            if (ModContent.GetInstance<Config>().researchRecipes)
            {
                rebuildingCache = true;
                List<int> oldResearchCache = new List<int>();
                oldResearchCache.AddRange(researchedCache);
                foreach (int type in oldResearchCache)
                {
                    Item itm = new Item();
                    itm.SetDefaults(type);
                    rebuildingCache = true;

                    if (itm.createTile >= 0)
                    {
                        List<int> tiles = AdjTiles(itm.createTile);
                        foreach (int t in tiles)
                        {
                            RecipeFinder rf = new RecipeFinder();
                            rf.AddTile(t);
                            List<Recipe> res = rf.SearchRecipes();
                            // Main.NewText("Found " + res.Count + "recipes with tile.");
                            foreach (Recipe r in res)
                            {
                                validateAndResearchRecipe(r);
                            }
                        }
                    }
                    RecipeFinder rf2 = new RecipeFinder();
                    rf2.AddIngredient(itm.type);
                    List<Recipe> res2 = rf2.SearchRecipes();
                    // Main.NewText("Found " + res2.Count + "recipes with item.");
                    foreach (Recipe r in res2)
                    {
                        validateAndResearchRecipe(r);
                    }
                    rebuildingCache = false;
                    ((ResearchFrom14)mod).ui.recipes.changedToList = true;
                }
            }
        }
        public void AddAllResearchedItems(List<int> researched)
        {
            foreach(int type in researched)
            {
                if (!researchedCache.Contains(type))
                {
                    rebuildingCache = true;
                    Item itm = new Item();
                    itm.SetDefaults(type);
                    AddResearchedAmount(type, Int32.MaxValue - 1000);
                    researchedCache.Add(type);
                    if (ModContent.GetInstance<Config>().researchRecipes)
                    {
                        if (itm.createTile >= 0)
                        {
                            List<int> tiles = AdjTiles(itm.createTile);
                            foreach (int t in tiles)
                            {
                                RecipeFinder rf = new RecipeFinder();
                                rf.AddTile(t);
                                List<Recipe> res = rf.SearchRecipes();
                                // Main.NewText("Found " + res.Count + "recipes with tile.");
                                foreach (Recipe r in res)
                                {
                                    validateAndResearchRecipe(r);
                                }
                            }
                        }
                        RecipeFinder rf2 = new RecipeFinder();
                        rf2.AddIngredient(itm.type);
                        List<Recipe> res2 = rf2.SearchRecipes();
                        // Main.NewText("Found " + res2.Count + "recipes with item.");
                        foreach (Recipe r in res2)
                        {
                            validateAndResearchRecipe(r);
                        }
                    }
                    rebuildingCache = false;
                    ((ResearchFrom14)mod).ui.recipes.changedToList = true;

                }
            }
        }

        public bool waitingForResearchCache()
        {
            return populateCache != null && !populateCache.IsCompleted;
        }

        public void RebuildCache()
        {
            if (!waitingForResearchCache())
            {
                populateCache = Task.Run(actualRebuildCache);
            }
            else
            {
                dirtyCache = true;
            }
        }

        private void actualRebuildCache()
        {
            rebuildCacheReset:
            rebuildingCache = true;
            dirtyCache = false;
            researchedCache.Clear();
            researchedTileCache.Clear();
            researchedTileAdj = new bool[TileLoader.TileCount];
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (IsResearched(i))
                {
                    researchedCache.Add(i);
                    Item itm = new Item();
                    try
                    {
                        itm.SetDefaults(i);
                        if (itm.createTile >= 0)
                            AdjTiles(itm.createTile);

                    }catch (Exception ex)
                    {
                        mod.Logger.Warn("Item " + i +" threw excetpion during setDefaults:\n"+ ex.ToString() +"\n"+  ex.StackTrace);
                    }
                    
                }
                
                if (dirtyCache)
                    goto rebuildCacheReset;
            }
            
            ((ResearchFrom14)mod).ui.recipes.changedToList = true;
            rebuildingCache = false;
            mod.Logger.Info("Player " + player.name + "'s Cache knows " + researchedCache.Count + " Items");

        }

        private List<int> AdjTiles(int type)
        {
            List<int> ans = new List<int>() { type };
            if (!researchedTileCache.Contains(type))  
                researchedTileCache.Add(type);

            researchedTileAdj[type] = true;

            if (type == 302 || type == 77 || type == 133)
            {
                ans.AddRange(AdjTiles(17));
            }
            if (type == 133)
            {
                ans.AddRange(AdjTiles(77));
            }
            if (type == 134)
            {
                ans.AddRange(AdjTiles(16));
            }
            if (type == 354 ||type == 469 || type == 355)
            {
                ans.AddRange(AdjTiles(14));
            }
            if (type == 355)
            {
                ans.AddRange(AdjTiles(13));
            }
            List<int>ans2 = new List<int>();
            foreach(int i in ans)
            {
                if (!ans2.Contains(i))
                    ans2.Add(i);
            }
            ans = ans2;
            ModTile tile = TileLoader.GetTile(type);
            if (tile != null)
            {
                foreach (int num in tile.adjTiles)
                {
                    if (!ans.Contains(num))
                        ans.Add(num);
                    if (!researchedTileCache.Contains(num))
                        researchedTileCache.Add(num);
                    researchedTileAdj[num] = true;
                }
            }
            Type typeOfLoader = typeof(TileLoader);
            FieldInfo info = typeOfLoader.GetField("HookAdjTiles", BindingFlags.NonPublic | BindingFlags.Static);
            Func<int, int[]>[] hookAdjTiles = (Func<int, int[]>[])info.GetValue(null);
            for (int i = 0; i < hookAdjTiles.Length; i++)
            {
                foreach (int num2 in hookAdjTiles[i](type))
                {
                    if (!ans.Contains(num2))
                        ans.Add(num2);
                    if (!researchedTileCache.Contains(num2))
                        researchedTileCache.Add(num2);
                    researchedTileAdj[num2] = true;
                }
            }
            return ans;
        }

        public void Research()
        {
            if (destroyingItem == null || destroyingItem.IsAir)
                return;
            int type = destroyingItem.type;
            int stack = destroyingItem.stack;
            destroyingItem.stack = AddResearchedAmount(destroyingItem);
            if (destroyingItem.stack != stack)
            {
                if (destroyingItem.stack <= 0)
                {
                    destroyingItem.TurnToAir();
                }
                else if(ResearchFrom14.PlaceInInventory(player, destroyingItem))
                {
                    destroyingItem = new Item();
                }

                if (GetResearchedAmount(type) < ResearchTable.GetTotalResearch(type))
                {
                    Main.PlaySound(SoundID.Grab);
                }
                else
                {
                    Main.PlaySound(SoundID.Item4);
                    rebuildingCache = true;
                    researchedCache.Add(type);
                    ((ResearchFrom14)mod).ui.recipes.changedToList = true;
                    if (ModContent.GetInstance<Config>().researchRecipes)
                    {
                        Item itm = new Item();
                        itm.SetDefaults(type);
                        
                        if(itm.createTile >= 0)
                        {
                            List<int> tiles = AdjTiles(itm.createTile);
                            foreach (int t in tiles)
                            {
                                RecipeFinder rf = new RecipeFinder();
                                rf.AddTile(t);
                                List<Recipe> res = rf.SearchRecipes();
                                // Main.NewText("Found " + res.Count + "recipes with tile.");
                                foreach (Recipe r in res)
                                {
                                    validateAndResearchRecipe(r);
                                }
                            }
                        }
                        RecipeFinder rf2 = new RecipeFinder();
                        rf2.AddIngredient(itm.type);
                        List<Recipe> res2 = rf2.SearchRecipes();
                       // Main.NewText("Found " + res2.Count + "recipes with item.");
                        foreach (Recipe r in res2)
                        {
                            validateAndResearchRecipe(r);
                        }
                    }
                    rebuildingCache = false;
                }
            }
        }

        private void validateAndResearchRecipe(Recipe r)
        {
            if (IsResearched(r.createItem.type))
                return;

            if(r.requiredTile == null || r.requiredTile.Length == 0)
            {
                goto label_checkIngredients;
            }
            int[] tiles = new int[r.requiredTile.Length];
            for (int t = 0; t < tiles.Length; t++)
            {
               tiles[t] = r.requiredTile[t];
                //Main.NewText("Req tile = " + tiles[t]);
            }
            if (tiles.Length > 0)
            {
                for(int i = 0; i< tiles.Length; i++)
                {
                    if(tiles[i] >= 0)
                    {
                        if (tiles[i] >= researchedTileAdj.Length)
                            return;
                        if (!researchedTileAdj[tiles[i]])
                            return;
                    }
                }
            }

            label_checkIngredients:
            if (r.requiredItem == null || r.requiredItem.Length == 0)
            {
                goto label_direct_add;
            }
            Item[] ing = r.requiredItem;
            for (int i = 0; i < ing.Length; i++)
            {
                if(ing[i] == null || ing[i].IsAir)
                {
                   // Main.NewText("Was air");
                    continue;
                }
                if (IsResearched(ing[i].type))
                {
                   // Main.NewText("Found item= " + ing[i].Name);
                    continue;
                }

                for(int g = 0; g < researchedCache.Count; g++)
                {
                    if(r.AcceptedByItemGroups(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                    if(r.anyIronBar && r.useIronBar(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                    if (r.anyWood && r.useWood(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                    if (r.anySand && r.useSand(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                    if (r.anyPressurePlate && r.usePressurePlate(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                    if (r.anyFragment && r.useFragment(researchedCache[g], ing[i].type))
                    {
                        goto label_next_recipe_item;
                    }
                }
                return;

                label_next_recipe_item:
                //Main.NewText("Found item replacement= " + ing[i].Name);
                continue;
            }
            label_direct_add:
            
            AddResearchedAmount(r.createItem.type, Int32.MaxValue - 1000);
            researchedCache.Add(r.createItem.type);
            if (ModContent.GetInstance<Config>().researchRecipes)
            {
                Item itm = new Item();
                itm.SetDefaults(r.createItem.type);
                if (itm.createTile >= 0)
                {
                    List<int> tiless = AdjTiles(itm.createTile);
                    foreach (int t in tiless)
                    {
                        RecipeFinder rf = new RecipeFinder();
                        rf.AddTile(t);
                        List<Recipe> res = rf.SearchRecipes();
                        // Main.NewText("Found " + res.Count + "recipes with tile.");
                        foreach (Recipe r2 in res)
                        {
                            validateAndResearchRecipe(r2);
                        }
                    }
                }
                RecipeFinder rf2 = new RecipeFinder();
                rf2.AddIngredient(r.createItem.type);
                List<Recipe> res2 = rf2.SearchRecipes();
                // Main.NewText("Found " + res2.Count + "recipes with item.");
                foreach (Recipe r2 in res2)
                {
                    validateAndResearchRecipe(r2);
                }
            }
        }

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            if (ResearchUI.visible && inventory != null && inventory[slot] != null && !inventory[slot].IsAir)
            {
                if(destroyingItem == null || destroyingItem.IsAir)
                {
                    destroyingItem = inventory[slot];
                    inventory[slot] = new Item();
                }else if(destroyingItem.type == inventory[slot].type && destroyingItem.stack < destroyingItem.maxStack)
                {
                    int total = destroyingItem.stack + inventory[slot].stack;
                    if(total <= destroyingItem.maxStack)
                    {
                        destroyingItem.stack = total;
                        inventory[slot] = new Item();
                    }
                    else
                    {
                        total = inventory[slot].stack - (destroyingItem.maxStack - destroyingItem.stack);
                        inventory[slot].stack = total;
                        destroyingItem.stack = destroyingItem.maxStack;
                    }
                }
                else
                {
                    Item temp = inventory[slot];
                    inventory[slot] = destroyingItem;
                    destroyingItem = temp;
                }
                if (ModContent.GetInstance<Config>().autoShiftResearch)
                    Research();
                return true;
            }
            return false;

        }

        #region save_load_update
        public override TagCompound Save()
        {

            TagCompound s = new TagCompound();
            if (research == null || research.Count <= 0)
                return s;
            if (destroyingItem == null)
                destroyingItem = new Item();
            research["0.research"] = ItemIO.Save(destroyingItem);
            return research;

        }

        public override void Load(TagCompound tag)
        {
            research = tag;
            if (research == null)
                research = new TagCompound();
            if (research.ContainsKey("0.research"))
            {
                destroyingItem = ItemIO.Load(research.GetCompound("0.research"));
                research.Remove("0.research");
            }
            if (!research.ContainsKey(ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchSharingBook>())) || 
                !research.ContainsKey(ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchErasingBook>())))
            {
                research[ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchSharingBook>())] = Int32.MaxValue - 1000;
                research[ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchErasingBook>())] = Int32.MaxValue - 1000;
            }
            researchedCache = new List<int>();
            mod.Logger.Info("Player " + player.name + " knows " + research.Count + " Items"); 
            populateCache = Task.Run(actualRebuildCache);
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = mod.GetPacket();
            int cnt = 0;
            TagCompound research2 = new TagCompound();
            IEnumerator<KeyValuePair<string, object>> enume = research.GetEnumerator();
            enume.Reset();
            while (enume.MoveNext())
            {
                research2[enume.Current.Key] = research.GetAsInt(enume.Current.Key);
                cnt += (enume.Current.Key.ToByteArray().Length/128) + 1;
                if (cnt >= 500)
                {
                    packet.Write((byte)0);
                    packet.Write((byte)player.whoAmI);
                    TagIO.Write(research2, packet);
                    packet.Send(toWho, fromWho);
                    cnt = 0;
                    packet = mod.GetPacket();
                    research2.Clear();
                }
            }
            if (cnt > 0)
            {
                packet.Write((byte)0);
                packet.Write((byte)player.whoAmI);
                TagIO.Write(research2, packet);
                packet.Send(toWho, fromWho);
            }
            packet = mod.GetPacket();
            packet.Write((byte)1);
            packet.Write((byte)player.whoAmI);
            packet.Send(toWho, fromWho);

        }
        #endregion

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (ResearchFrom14.hotkey.JustPressed && (Main.playerInventory|| ModContent.GetInstance<Config>().buttonAlwaysOn))
            {
                if (!ResearchUI.visible)
                {
                    Main.playerInventory = true;
                    (mod as ResearchFrom14).ActivatePurchaseUI(player.whoAmI);
                }
                else
                    (mod as ResearchFrom14).ui.setVisible(false);
            }
        }
    }
}
