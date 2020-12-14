
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
        
        public List<int> sharingWithWho = new List<int>();
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
            if(item.prefix > 0)
            {
                return IsResearchedPrefix(item) && (GetResearchedAmount(item) >= res);
            }
            return GetResearchedAmount(item) >= res;
        }

        public bool IsResearched(int itemType)
        {
            int res = ResearchTable.GetTotalResearch(itemType);
            if (res < 1)
                return false;
            return GetResearchedAmount(itemType) >= res;
        }

        public bool IsResearchedPrefix(Item item)
        {
            String iTag = ResearchFrom14.ItemToTag(item) + ":p";
            if (research.ContainsKey(iTag))
            {
                if (research[iTag] is int || research[iTag] is byte)
                {
                    if ((byte)(research[iTag]) == item.prefix)
                        return true;
                }
                else
                {
                    byte[] ba = research.GetByteArray(ResearchFrom14.ItemToTag(item) + ":p");
                    foreach (byte b in ba)
                    {
                        if (b == item.prefix)
                            return true;
                    }
                }
            }
            return item.prefix == 0;
        }

        public void AddResearchPrefix(int itm, byte prefix)
        {
            if (prefix == 0)
                return;
            List<byte> prefixes = new List<byte>();
            String iTag = ResearchFrom14.ItemIDToTag(itm) + ":p";
            if (research.ContainsKey(iTag))
            {
                if ((research[iTag] as byte?) != null)
                {
                    prefixes.Add((research[iTag] as byte?).Value);
                }
                else if ((research[iTag] as byte[]) != null)
                {
                    prefixes.AddRange(research.GetByteArray(iTag));
                }

            }
            if (!prefixes.Contains(prefix))
            {

                prefixes.Add(prefix);
                prefixes.Sort();
                research[iTag] = prefixes.ToArray();
                /*
                CheckForTeammates();
                foreach (int plr in sharingWithWho)
                {
                    ModPacket pk = mod.GetPacket();
                    pk.Write((byte)12);
                    pk.Write((byte)plr);
                    pk.Write(1);
                    pk.Write((byte)(prefix));
                    if (Main.netMode == NetmodeID.Server)
                    {
                        pk.Send(plr);
                    }
                    else
                    {
                        pk.Send();
                    }
                }*/
            }
            return;
        }

        public void AddResearchPrefix(Item item)
        {
            if (item.prefix == 0)
                return;
            List<byte> prefixes = new List<byte>();
            String iTag = ResearchFrom14.ItemToTag(item) + ":p";
            if (research.ContainsKey(iTag))
            {
                if((research[iTag] as byte?) != null)
                {
                    prefixes.Add((research[iTag] as byte?).Value);
                }
                else if((research[iTag] as byte[]) != null)
                {
                    prefixes.AddRange(research.GetByteArray(iTag));
                }
            }
            if (!prefixes.Contains(item.prefix))
            {

                prefixes.Add(item.prefix);
                prefixes.Sort();
                research[iTag] = prefixes.ToArray();
                /*
                CheckForTeammates();
                foreach(int plr in sharingWithWho)
                {
                    ModPacket pk = mod.GetPacket();
                    pk.Write((byte)12);
                    pk.Write((byte)plr);
                    pk.Write(1);
                    pk.Write((byte)(item.prefix));
                    if (Main.netMode == NetmodeID.Server)
                    {
                        pk.Send(plr);
                    }
                    else
                    {
                        pk.Send();
                    }
                }*/
            }
            return;
        }

        public byte[] GetResearchedPrefixes(Item item)
        {
            return GetResearchedPrefixes(ResearchFrom14.ItemToTag(item));
        }

        public byte[] GetResearchedPrefixes(String itemTag)
        {
            if (research.ContainsKey(itemTag + ":p"))
            {
                if (research[itemTag + ":p"] is int || research[itemTag + ":p"] is byte)
                {
                    return new byte[] { (byte)(research[itemTag + ":p"]) };
                }
                else
                {
                    return research.GetByteArray(itemTag + ":p");
                }
            }
                
            return new byte[0];
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
            if (itemTag == "0")
            {
                if (research.ContainsKey(itemTag))
                    research.Remove(itemTag);
                return amount;
            }
                

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
            
            int retval = AddResearchedAmount(ResearchFrom14.ItemToTag(realItem), realItem.stack);
            CheckForTeammates();
            foreach (int plr in sharingWithWho)
            {
                ModPacket pk = mod.GetPacket();
                pk.Write((byte)11);
                pk.Write((byte)(plr));
                pk.Write((byte)(Main.myPlayer));
                pk.Write(realItem.type);
                pk.Write(realItem.stack - retval);
                pk.Send(plr);
            }
            return retval;
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
                    CheckRecipesForItem(type);
                }
                rebuildingCache = false;
                ((ResearchFrom14)mod).ui.recipes.changedToList = true;
            }
        }
        private int playerToSendCacheTo = -1;
        public virtual void CheckForTeammates()
        {
            mod.Logger.Info("Checking for teammates...");
            if (Main.netMode == NetmodeID.MultiplayerClient && ModContent.GetInstance<Config>().shareWithTeam && player.team > 0)
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    if (sharingWithWho.Contains(i))
                    {

                        if (!Main.player[i].active || Main.player[i].team != player.team)
                        {
                            sharingWithWho.Remove(i);
                            mod.Logger.Info("Player " + i + " was in " + player.team + " team.");
                        }
                    }
                    else
                    {
                        if (i != player.whoAmI && Main.player[i].active && Main.player[i].team == player.team)
                        {
                            mod.Logger.Info("Player " + i + " was just found in " + player.team + " team.");
                            shareCacheTo(i);
                            sharingWithWho.Add(i);
                        }
                    }
                }
            }
        }

        private List<string> researchedParts = new List<string>();
        public void CheckRecipesForItem(int type)
        {
            ((ResearchFrom14)mod).ui.recipes.changedToList = true;
            if (ModContent.GetInstance<Config>().researchRecipes)
            {
                Item itm = new Item();
                itm.SetDefaults(type);

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
            Mod rare = ModLoader.GetMod("ARareItemSwapJPANs");
            if (rare != null && ModContent.GetInstance<Config>().PartsCompat)
            {
                List<string> parts = rare.Call("GetPartList") as List<string>;
                if (parts == null)
                    return;
                parts.RemoveAll((x) => !IsResearched(x));
                if(parts.Count > 0)
                {
                    bool allPartsResearched = true;
                    foreach(string part in parts)
                    {
                        allPartsResearched = allPartsResearched && researchedParts.Contains(part);
                    }
                    if (!allPartsResearched)
                    {
                        
                        List<Item> results = rare.Call("GetMaxPurchasesAvailable", parts) as List<Item>;
                       
                        if (results == null) 
                            return;
                        foreach (string part in parts)
                        {
                            if (!researchedParts.Contains(part))
                                researchedParts.Add(part);
                        }
                        foreach (Item result in results)
                        {
                           
                            if (!IsResearched(result))
                            {
                                AddResearchedAmount(result.type, Int32.MaxValue - 1000);
                                researchedCache.Add(result.type);
                                CheckRecipesForItem(result.type);
                            }
                        }
                    }
                }
            }
        }

        public void shareCacheTo( int target)
        {
            playerToSendCacheTo = target;
            Task.Run(spreadCache);
        }
        private void spreadCache()
        {
            int i = playerToSendCacheTo;
            actualRebuildCache();

            ModPacket pk = mod.GetPacket();
            pk.Write((byte)10);
            pk.Write((byte)i);
            pk.Write(researchedCache.Count);
            for (int k = 0; k < researchedCache.Count; k++)
                pk.Write(researchedCache[k]);
            if (Main.netMode == NetmodeID.Server)
                pk.Send(i);
            else
                pk.Send();
           // mod.Logger.Info("Sent message 10 to " + i);
           /*
            for (int itm = 0; itm < ItemLoader.ItemCount; itm++)
            {
                String iTag = ResearchFrom14.ItemIDToTag(itm) + ":p";
                if (research.ContainsKey(iTag))
                {

                    List<byte> prefixes = new List<byte>();

                    if (research.ContainsKey(iTag))
                    {
                        if((research[iTag] as byte?) != null)
                        {
                            prefixes.Add((research[iTag] as byte?).Value);
                        }
                        else if ((research[iTag] as byte[]) != null)
                        {
                            prefixes.AddRange(research.GetByteArray(iTag));
                        }

                    }
                    if (prefixes.Count > 0)
                    {
                        pk = mod.GetPacket();
                        pk.Write((byte)12);
                        pk.Write((byte)i);

                        pk.Write(prefixes.Count);
                        for (int k = 0; k < prefixes.Count; k++)
                            pk.Write((byte)(prefixes[k]));
                        if (Main.netMode == NetmodeID.Server)
                        {
                            pk.Send(i);
                        }
                        else
                        {
                            pk.Send();
                        }
                    }
                }
            }*/
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
                    CheckRecipesForItem(type);
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
                if (IsResearched(i) && !researchedCache.Contains(i))
                {
                    researchedCache.Add(i);
                    Item itm = new Item();
                    try
                    {
                        if (!ResearchFrom14.invalidSetDefaults.Contains(i))
                        {
                            itm.SetDefaults(i);
                            if (itm.createTile >= 0)
                                AdjTiles(itm.createTile);
                        }
                    }catch (Exception ex)
                    {
                        mod.Logger.Warn("Item " + i +" threw excetpion during setDefaults:\n"+ ex.ToString() +"\n"+  ex.StackTrace);
                        ResearchFrom14.invalidSetDefaults.Add(i);
                    }
                    
                }
                
                if (dirtyCache)
                    goto rebuildCacheReset;
            }
            
            ((ResearchFrom14)mod).ui.recipes.changedToList = true;
            rebuildingCache = false;
            mod.Logger.Info("Player " + player.name + "'s Cache knows " + researchedCache.Count + " Items");

        }

        public List<int> AdjTiles(int type)
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
            if (!IsResearched(destroyingItem))
            {
                bool needsCacheRebuilt = true;
                destroyingItem.stack = AddResearchedAmount(destroyingItem);
                AddResearchPrefix(destroyingItem);
                
                if (destroyingItem.stack == stack)
                {
                    destroyingItem.stack--;
                    needsCacheRebuilt = false;
                }
                if (destroyingItem.stack != stack)
                {
                    if (destroyingItem.stack <= 0)
                    {
                        destroyingItem.TurnToAir();
                    }
                    else if (ResearchFrom14.PlaceInInventory(player, destroyingItem))
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
                        if (needsCacheRebuilt)
                        {
                            rebuildingCache = true;
                            researchedCache.Add(type);
                            CheckRecipesForItem(type);
                            rebuildingCache = false;
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
            CheckRecipesForItem(r.createItem.type);
        }

        public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
        {
            if (ResearchUI.visible && inventory != null && inventory[slot] != null && !inventory[slot].IsAir && 
                context != Terraria.UI.ItemSlot.Context.ShopItem && context != Terraria.UI.ItemSlot.Context.ChatItem)
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
                if (ModContent.GetInstance<Config>().autoShiftResearch && IsResearchable(destroyingItem))
                    Research();
                return true;
            }else if (PrefixUI.visible && inventory != null && inventory[slot] != null && !inventory[slot].IsAir)
            {
                Item working = ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item;
                if (working == null || working.IsAir)
                {
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item = inventory[slot];
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.realItem = inventory[slot];
                    inventory[slot] = new Item();
                }
                else if (working.type == inventory[slot].type && working.stack < working.maxStack)
                {
                    int total = working.stack + inventory[slot].stack;
                    if (total <= working.maxStack)
                    {
                        working.stack = total;
                        inventory[slot] = new Item();
                    }
                    else
                    {
                        total = inventory[slot].stack - (working.maxStack - working.stack);
                        inventory[slot].stack = total;
                        working.stack = working.maxStack;
                    }
                }
                else
                {
                    Item temp = inventory[slot];
                    inventory[slot] = working;
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item = temp;
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.realItem = temp;
                }
                return true;
            }
            return false;

        }

        public bool IsResearchable(Item destroyingItem)
        {
            return ResearchTable.GetTotalResearch(destroyingItem) > 0;
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
            research["0.prefix"] = ItemIO.Save(((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item);
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
            if (research.ContainsKey("0.prefix"))
            {
                ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item = ItemIO.Load(research.GetCompound("0.prefix"));
                ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.realItem = ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).preUI.itemSlot.item;
                research.Remove("0.prefix");
            }

            if (!research.ContainsKey(ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchSharingBook>())) || 
                !research.ContainsKey(ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchErasingBook>())))
            {
                research[ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchSharingBook>())] = Int32.MaxValue - 1000;
                research[ResearchFrom14.ItemIDToTag(ModContent.ItemType<ResearchErasingBook>())] = Int32.MaxValue - 1000;
            }
            researchedCache = new List<int>();
            researchedTileCache = new List<int>();
            researchedTileAdj = new bool[TileLoader.TileCount];
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
            if (ResearchFrom14.preHotkey.JustPressed && (Main.playerInventory || ModContent.GetInstance<Config>().buttonAlwaysOn))
            {
                if (!PrefixUI.visible)
                {
                    Main.playerInventory = true;
                    (mod as ResearchFrom14).ActivatePrefixUI(player.whoAmI);
                }
                else
                    (mod as ResearchFrom14).preUI.setVisible(false);
            }
        }
    }
}
