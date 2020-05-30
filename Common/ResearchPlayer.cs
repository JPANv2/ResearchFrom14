
using System;
using System.Collections.Generic;
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

        private Task populateCache = null;
        public bool dirtyCache = false;
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

        public void AddAllResearchedItems(List<int> researched)
        {
            foreach(int type in researched)
            {
                if (!researchedCache.Contains(type))
                {
                    Item itm = new Item();
                    itm.SetDefaults(type);
                    AddResearchedAmount(type, Int32.MaxValue - 1000);
                    researchedCache.Add(type);
                    if (ModContent.GetInstance<Config>().researchRecipes)
                    {
                        if (itm.createTile >= 0)
                        {
                            RecipeFinder rf = new RecipeFinder();
                            rf.AddTile(itm.createTile);
                            List<Recipe> res = rf.SearchRecipes();
                            // Main.NewText("Found " + res.Count + "recipes with tile.");
                            foreach (Recipe r in res)
                            {
                                validateAndResearchRecipe(r);
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
                    ((ResearchFrom14)mod).ui.recipes.invalidatedList = true;
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
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                if (IsResearched(i))
                    researchedCache.Add(i);
            }
            if (dirtyCache)
            {
                dirtyCache = false;
                actualRebuildCache();
            }
            if (ResearchUI.visible)
            {
                ((ResearchFrom14)mod).ui.recipes.invalidatedList = true;
                ((ResearchFrom14)mod).ui.recipes.changedToList = true;
            }
            mod.Logger.Info("Player " + player.name + "'s Cache knows " + researchedCache.Count + " Items");

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
                    researchedCache.Add(type);
                    ((ResearchFrom14)mod).ui.recipes.invalidatedList = true;
                    ((ResearchFrom14)mod).ui.recipes.changedToList = true;
                    if (ModContent.GetInstance<Config>().researchRecipes)
                    {
                        Item itm = new Item();
                        itm.SetDefaults(type);
                        if(itm.createTile >= 0)
                        {
                            RecipeFinder rf = new RecipeFinder();
                            rf.AddTile(itm.createTile);
                            List<Recipe> res = rf.SearchRecipes();
                           // Main.NewText("Found " + res.Count + "recipes with tile.");
                            foreach (Recipe r in res)
                            {
                                validateAndResearchRecipe(r);
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
            bool ending = true;
            if (tiles.Length > 0)
            {
                foreach (int i in researchedCache) 
                {
                    if (ResearchTable.createdTiles[i] > -1)
                    {
                        ModTile target = TileLoader.GetTile(ResearchTable.createdTiles[i]);
                        for (int t = 0; t < tiles.Length; t++)
                        {
                            if(tiles[t] == ResearchTable.createdTiles[i])
                            {
                               // Main.NewText("Found tile = " + tiles[t]);
                                tiles[t] = -1;
                                goto label_foundTile;
                            }
                            ModTile source = TileLoader.GetTile(tiles[t]);
                            if(target != null && target.adjTiles!= null && target.adjTiles.Length > 0)
                            {
                                foreach (int adj in target.adjTiles){
                                    if (tiles[t] == adj)
                                    {
                                    //    Main.NewText("Found tile adj= " + tiles[t] + " : " + adj);
                                        tiles[t] = -1;
                                        goto label_foundTile;
                                    }
                                }
                                if (source != null && source.adjTiles != null && source.adjTiles.Length > 0)
                                {
                                    foreach (int adj1 in target.adjTiles)
                                    {
                                        foreach (int adj2 in source.adjTiles)
                                        {
                                            if (adj1 == adj2)
                                            {
                                            //    Main.NewText("Found tile adj2 = " + adj1 + " : " + adj2);
                                                tiles[t] = -1;
                                                goto label_foundTile;
                                            }
                                        }
                                    }
                                }
                            }
                            else if(source != null)
                            {
                                foreach (int adj2 in source.adjTiles)
                                {
                                    if (ResearchTable.createdTiles[i] == adj2)
                                    {
                                      //  Main.NewText("Found tile adj= " + tiles[t] + " : " + adj2);
                                        tiles[t] = -1;
                                        goto label_foundTile;
                                    }
                                }
                            }
                            label_foundTile:
                            continue;
                        }
                        ending = true;
                        for(int t = 0; t< tiles.Length; t++)
                        {
                            ending = ending && tiles[t] == -1;
                        }
                        if (ending)
                        {
                            goto label_checkIngredients;
                        }
                    }
                }
                ending = true;
                for (int t = 0; t < tiles.Length; t++)
                {
                    ending = ending && tiles[t] == -1;
                }
                if (ending)
                {
                    goto label_checkIngredients;
                }
                else
                {
                    return;
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
                    RecipeFinder rf = new RecipeFinder();
                    rf.AddTile(itm.createTile);
                    List<Recipe> res = rf.SearchRecipes();
                    //Main.NewText("Found " + res.Count + "recipes with tile.");
                    foreach (Recipe r2 in res)
                    {
                        validateAndResearchRecipe(r2);
                    }
                }
                RecipeFinder rf2 = new RecipeFinder();
                rf2.AddIngredient(itm.type);
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
