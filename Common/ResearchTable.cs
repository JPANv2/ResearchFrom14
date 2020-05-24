using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ResearchFrom14.Configs;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ResearchFrom14.Common
{
    public class ResearchTable
    {

        static Task initTask = null;

        public static int[] totalResearch = new int[0];
        public static Dictionary<string, List<int>> category = new Dictionary<string, List<int>>();
        public static int[] createdTiles = new int[0];
        public static void InitResearchTable()
        {
           // Main.NewText("Initializing Research Table...");
            totalResearch = new int[ItemLoader.ItemCount+1];
            category.Clear();
            createdTiles = new int[ItemLoader.ItemCount + 1];
            ModLoader.GetMod("ResearchFrom14").Logger.Info("Inititalizing Research System...");
            initTask = Task.Run(FillResearchTable);
        }

        private static void FillResearchTable()
        {
            foreach (string tag in ModContent.GetInstance<ExceptionListConfig>().customItemValues.Keys)
            {
                int type = ResearchFrom14.getTypeFromTag(tag);
                if (type > 0)
                {
                    totalResearch[type] = ModContent.GetInstance<ExceptionListConfig>().customItemValues[tag].value;
                    foreach (String s in ModContent.GetInstance<ExceptionListConfig>().customItemValues[tag].categories)
                    {
                        AddCategory(s, type);
                    }
                }
            }

            addExceptionalItems();

            for (int i = 1; i < ItemLoader.ItemCount; i++)
            {
                createdTiles[i] = -1;
                if (totalResearch[i] == 0)
                {
                    Item test = new Item();
                    test.SetDefaults(i);
                    if (test.type == 0)
                    {
                        totalResearch[i] = -1;
                        continue;
                    }
                    if (test.maxStack == 1)
                    {
                        totalResearch[test.type] = 1;
                        if (test.damage > 0)
                        {
                            if (test.melee)
                            {
                                AddCategory("Weapons/Melee", test.type);
                            }
                            else if (test.ranged)
                            {
                                AddCategory("Weapons/Ranged", test.type);
                            }
                            else if (test.magic)
                            {
                                AddCategory("Weapons/Magic", test.type);
                            }
                            else if (test.thrown)
                            {
                                AddCategory("Weapons/Thrown", test.type);
                            }
                            else if (test.summon)
                            {
                                AddCategory("Weapons/Summoner", test.type);
                            }
                            else
                            {
                                AddCategory("Weapons/Other", test.type);
                            }
                        }
                        else if (test.vanity)
                        {
                            if (test.accessory)
                            {
                                if (test.backSlot > 0 || test.wingSlot > 0)
                                {
                                    AddCategory("Vanity/Accessories/Wings and Capes", test.type);
                                    AddCategory("Accessories/Vanity/Wings and Capes", test.type);
                                }
                                else
                                {
                                    AddCategory("Vanity/Accessories", test.type);
                                    AddCategory("Accessories/Vanity", test.type);
                                }
                            }
                            else if (test.headSlot > 0)
                            {
                                AddCategory("Vanity/Armor/Head", test.type);
                                AddCategory("Armor/Vanity/Head", test.type);
                            }
                            else if (test.bodySlot > 0)
                            {
                                AddCategory("Vanity/Armor/Body", test.type);
                                AddCategory("Armor/Vanity/Body", test.type);
                            }
                            else if (test.legSlot > 0)
                            {
                                AddCategory("Vanity/Armor/Legs", test.type);
                                AddCategory("Armor/Vanity/Legs", test.type);
                            }
                            else if (test.mountType > 0)
                            {
                                AddCategory("Vanity/Mounts", test.type);
                                AddCategory("Mounts/Vanity", test.type);
                            }
                            else
                            {
                                AddCategory("Vanity", test.type);
                            }
                        }
                        else if (test.accessory)
                        {
                            if (test.accessory)
                            {
                                if (test.backSlot > 0 || test.wingSlot > 0)
                                {
                                    AddCategory("Accessories/Wings and Capes", test.type);
                                }
                                else
                                {
                                    AddCategory("Accessories", test.type);
                                }
                            }
                        }
                        else if (test.headSlot > 0)
                        {
                            AddCategory("Armor/Head", test.type);

                        }
                        else if (test.bodySlot > 0)
                        {
                            AddCategory("Armor/Body", test.type);

                        }
                        else if (test.legSlot > 0)
                        {
                            AddCategory("Armor/Legs", test.type);
                        }
                        else if (test.mountType > 0)
                        {
                            if (MountID.Sets.Cart[test.mountType])
                            {
                                AddCategory("Minecarts", test.type);
                            }
                            else
                            {
                                AddCategory("Mounts", test.type);
                            }

                        }
                        else if (Main.projHook[test.shoot])
                        {
                            AddCategory("Hooks", test.type);
                        }
                        else if (test.buffType > 0 && Main.vanityPet[test.buffType] && !Main.lightPet[test.buffType])
                        {
                            AddCategory("Pets/Normal Pets", test.type);
                        }
                        else if (test.buffType > 0 && Main.lightPet[test.buffType])
                        {
                            AddCategory("Pets/Light Pets", test.type);
                        }
                    }
                    else if (test.rare == -11)
                    {
                        totalResearch[test.type] = 2;
                        AddCategory("Quest Items", test.type);
                    }
                    else if (isCustomCurrency(test))
                    {
                        totalResearch[test.type] = 50;
                        AddCategory("Currency", test.type);
                    }
                    else if (test.createWall >= 0)
                    {
                        totalResearch[test.type] = 400;
                        AddCategory("Walls", test.type);
                    }
                    else if (test.createTile >= 0)
                    {
                        createdTiles[test.type] = test.createTile;
                        TileObjectData placer = TileObjectData.GetTileData(test.createTile, test.placeStyle, 0);
                        if (test.type == ItemID.Acorn)
                        {
                            totalResearch[test.type] = 50;
                            AddCategory("Tiles", test.type);
                        }
                        else if (placer != null && (placer.Width > 1 || placer.Height > 1))
                        {
                            if (test.Name.EndsWith(" Crate"))
                            {
                                totalResearch[test.type] = 10;
                                AddCategory("Crates", test.type);
                            }
                            else
                            {
                                totalResearch[test.type] = 1;
                                AddCategory("Furniture", test.type);
                            }
                        }
                        else if (placer != null && placer.AnchorBottom != null && placer.AnchorBottom.type == AnchorType.Table)
                        {
                            totalResearch[test.type] = 1;
                            AddCategory("Furniture", test.type);
                        }
                        else if (TileID.Sets.Platforms[test.createTile])
                        {
                            RecipeFinder f = new RecipeFinder();
                            f.SetResult(test.type);
                            List<Recipe> plats = f.SearchRecipes();
                            if (plats == null || plats.Count == 0)
                                totalResearch[test.type] = 20;
                            else
                                totalResearch[test.type] = 200;

                            AddCategory("Platforms", test.type);
                        }
                        else if (test.Name.EndsWith(" Bar") || test.Name.EndsWith(" Ingot") || test.Name.EndsWith(" Alloy"))
                        {
                            totalResearch[test.type] = 25;
                            AddCategory("Bars", test.type);
                        }
                        else if (test.Name.EndsWith(" Seed") || test.Name.EndsWith(" Seeds"))
                        {
                            totalResearch[test.type] = 25;
                            AddCategory("Seeds", test.type);
                        }
                        else if (test.dye > 0 || test.hairDye > 0)
                        {
                            totalResearch[test.type] = 3;
                            AddCategory("Dye", test.type);
                        }
                        else if (isDyeMaterial(test))
                        {
                            totalResearch[test.type] = 3;
                            AddCategory("Dye", test.type);
                        }
                        else
                        {
                            totalResearch[test.type] = 100;
                            AddCategory("Tiles", test.type);
                        }
                    }
                    else
                    {

                        if (isBossBag(test) || isBossSummon(test))
                        {
                            totalResearch[test.type] = 3;
                            AddCategory("Boss Bags and Summons", test.type);
                        }
                        else if (test.makeNPC > 0 || test.bait > 0)
                        {
                            totalResearch[test.type] = 5;
                            AddCategory("Critters and Bait", test.type);
                        }
                        else if (test.dye > 0 || test.hairDye > 0)
                        {
                            totalResearch[test.type] = 3;
                            AddCategory("Dye", test.type);
                        }
                        else if (isDyeMaterial(test))
                        {
                            totalResearch[test.type] = 3;
                            AddCategory("Dye", test.type);
                        }
                        else if (test.healLife > 0 || test.healMana > 0)
                        {
                            totalResearch[test.type] = 30;
                            AddCategory("Potions and Food", test.type);
                        }
                        else if (test.buffType > 0 && test.buffTime > 0)
                        {
                            totalResearch[test.type] = 20;
                            AddCategory("Potions and Food", test.type);
                        }
                        else if (test.damage > 0)
                        {
                            totalResearch[test.type] = Math.Min(99, test.maxStack);
                            if (test.damage > 0)
                            {
                                if (test.melee)
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Melee", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Melee/Stackable", test.type);
                                    }
                                }
                                else if (test.ranged)
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Ranged", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Ranged/Stackable", test.type);
                                    }
                                }
                                else if (test.magic)
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Magic", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Magic/Stackable", test.type);
                                    }
                                }
                                else if (test.thrown)
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Thrown", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Thrown/Stackable", test.type);
                                    }
                                }
                                else if (test.summon)
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Summoner", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Summoner/Stackable", test.type);
                                    }
                                }
                                else
                                {
                                    if (test.ammo > 0)
                                    {
                                        AddCategory("Ammo/Other", test.type);
                                    }
                                    else
                                    {
                                        AddCategory("Weapons/Other/Stackable", test.type);
                                    }
                                }
                            }
                        }
                        else
                        {
                            totalResearch[test.type] = 25;
                        }
                    }
                    totalResearch[test.type] = Math.Min(totalResearch[test.type], test.maxStack);
                    if (test.consumable)
                    {
                        AddCategory("Consumable", test.type);
                    }
                    if (test.material)
                    {
                        AddCategory("Materials", test.type);
                    }
                    /* ModLoader.GetMod("ResearchFrom14").Logger.Info("Item " + test.Name + " ( id " + test.type + ") has categories:");
                     foreach(string cat in category.Keys)
                     {
                         if (category[cat].Contains(test.type))
                         {
                             ModLoader.GetMod("ResearchFrom14").Logger.Info("  - " + cat);
                         }
                     }*/
                    if (!ModContent.GetInstance<Config>().difficultyAffectsExceptions)
                    {
                        totalResearch[test.type] = (int)Math.Max(Math.Ceiling(totalResearch[test.type] * ModContent.GetInstance<Config>().difficulty), 1);
                    }
                }
            }
            ModLoader.GetMod("ResearchFrom14").Logger.Info("Research System: Applying Difficulty...");
            if (!ModContent.GetInstance<Config>().difficultyAffectsExceptions)
            {
                for (int i = 1; i < ItemLoader.ItemCount; i++)
                {
                    if (totalResearch[i] > 0)
                    {
                        totalResearch[i] = (int)Math.Max(Math.Ceiling(totalResearch[i] * ModContent.GetInstance<Config>().difficulty), 1);
                    }
                }
            }
            ModLoader.GetMod("ResearchFrom14").Logger.Info("Research System Initialized.");
        }

        public static void ClearTable()
        {
            totalResearch = new int[0];
            category = new Dictionary<string, List<int>>();
            createdTiles = new int[0];
        }

        private static bool isDyeMaterial(Item test)
        {
            RecipeFinder rf = new RecipeFinder();
            rf.AddIngredient(test.type);
            List<Recipe> found = rf.SearchRecipes();
            foreach(Recipe r in found)
            {
                if (r.createItem.dye > 0)
                    return true;
                if (r.createItem.hairDye > 0)
                    return true;
            }
            return false;
        }

        private static bool isCustomCurrency(Item test)
        {
            if (test.type == ItemID.DefenderMedal)
                return true;
            Type type = typeof(CustomCurrencyManager);
            FieldInfo info = type.GetField("_currencies", BindingFlags.NonPublic | BindingFlags.Static);
            object value = info.GetValue(null);
            Dictionary<int, CustomCurrencySystem> currencies = value as Dictionary<int, CustomCurrencySystem>;
            if(currencies != null)
            {
                foreach (CustomCurrencySystem ccs in currencies.Values)
                {
                    if (ccs.Accepts(test))
                        return true;
                }
            }
            return false;
        }

        private static bool isBossSummon(Item test)
        {
            if (test.type == ItemID.SlimeCrown || test.type == ItemID.SuspiciousLookingEye || test.type == ItemID.WormFood || test.type == ItemID.BloodySpine ||
                test.type == ItemID.Abeemination || test.type == ItemID.MechanicalEye || test.type == ItemID.MechanicalSkull || test.type == ItemID.MechanicalWorm ||
                test.type == ItemID.TruffleWorm || test.type == ItemID.LihzahrdPowerCell || test.type == ItemID.CelestialSigil)
                return true;
            if (test.type == ItemID.NightKey || test.type == ItemID.LightKey)
                return true;
            if (test.type == ItemID.SolarTablet || test.type == ItemID.SnowGlobe)
                return true;
            if (ModContent.GetInstance<Config>().summonDetection && test.type != ItemID.LifeCrystal && test.type != ItemID.ManaCrystal && test.type != ItemID.LifeFruit)
            {
                Type type = typeof(ItemLoader);
                FieldInfo info = type.GetField("HookSetDefaults", BindingFlags.NonPublic | BindingFlags.Static);
                object hookValue = info.GetValue(null);
                Type HookInfo = hookValue.GetType();
                //ModLoader.GetMod("ResearchFrom14").Logger.Warn("Hook is " + HookInfo);
                FieldInfo hookAdd = HookInfo.GetField("arr", BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                //ModLoader.GetMod("ResearchFrom14").Logger.Warn("Hookadd is " + hookAdd);
                object globals = hookAdd.GetValue(hookValue);
                hookAdd.SetValue(hookValue, new GlobalItem[0]);
                Item test2 = new Item();
                test2.SetDefaults(test.type);
                if (test2.useStyle == 4 && test2.useAnimation == test2.useTime && test2.useTime == 30 && test2.maxStack == 20)
                {
                    hookAdd.SetValue(hookValue, globals);
                    return true;
                }
                hookAdd.SetValue(hookValue, globals);
            }
            return false;
        }

        private static bool isBossBag(Item test)
        {
            if (test.type >= 3318 && test.type <= 3332)
                return true;
            if(test.modItem != null)
            {
                return (test.modItem.BossBagNPC > 0);
            }
            return false;
        }

        public static int GetTotalResearch(String itemTag)
        {
            return GetTotalResearch(ResearchFrom14.getTypeFromTag(itemTag));
        }

        public static int GetTotalResearch(Item realItem)
        {
            return GetTotalResearch(realItem.type);
        }

        public static int GetTotalResearch(int type)
        {
            if (totalResearch.Length == 0)
                InitResearchTable();
            if (type < 0 || type > totalResearch.Length)
                return 0;
            return totalResearch[type];
        }

        public static void AddCategory(String cat, int itemID)
        {
            if (cat.Contains("/"))
            {
                string catCopy = cat;
                do
                {
                    catCopy = catCopy.Substring(0, catCopy.LastIndexOf("/"));
                    AddCategoryNoSub(catCopy, itemID);
                }
                while (catCopy.LastIndexOf("/") > 0);
            }
            AddCategoryNoSub(cat, itemID);
        }
        public static void AddCategoryNoSub(String cat, int itemID)
        {
            if (!category.ContainsKey(cat))
            {
                category[cat] = new List<int>();
            }
            if(!category[cat].Contains(itemID))
                category[cat].Add(itemID);
        }


        private static void addExceptionalItems()
        {
            RT_addGems();
            RT_addKeys();
            RT_addTraps();
            RT_addFish();
            RT_addCalamity();
            for (int i = ItemID.CopperCoin; i <= ItemID.PlatinumCoin; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 100;
                    AddCategory("Currency", i);
                }
            }
            if (totalResearch[ItemID.Wire] == 0)
            {
                totalResearch[ItemID.Wire] = 50;
                AddCategory("Wire or Traps", ItemID.Wire);
            }

            if (totalResearch[ItemID.LifeCrystal] == 0)
            {
                totalResearch[ItemID.LifeCrystal] = 10;
                AddCategory("Stat Ups", ItemID.LifeCrystal);
            }

            if (totalResearch[ItemID.ManaCrystal] == 0)
            {
                totalResearch[ItemID.ManaCrystal] = 10;
                AddCategory("Stat Ups", ItemID.ManaCrystal);
            }
            if (totalResearch[ItemID.LifeFruit] == 0)
            {
                totalResearch[ItemID.LifeFruit] = 10;
                AddCategory("Stat Ups", ItemID.LifeFruit);
            }
            if (totalResearch[ItemID.FallenStar] == 0)
            {
                totalResearch[ItemID.FallenStar] = 20;
                AddCategory("Ammo/Ranged", ItemID.FallenStar);
            }

            if (totalResearch[ItemID.Hook] == 0)
            {
                totalResearch[ItemID.Hook] = 10;
                AddCategory("Materials", ItemID.Hook);
            }

            if (totalResearch[ItemID.Bottle] == 0)
            {
                totalResearch[ItemID.Bottle] = 25;
                AddCategory("Materials", ItemID.Bottle);
            }
            if (totalResearch[ItemID.BottledWater] == 0)
            {
                totalResearch[ItemID.BottledWater] = 25;
                AddCategory("Materials", ItemID.BottledWater);
            }
            if (totalResearch[ItemID.Spike] == 0)
            {
                totalResearch[ItemID.Spike] = 25;
                AddCategory("Tiles", ItemID.Spike);
            }

            if (totalResearch[ItemID.HerbBag] == 0)
            {
                totalResearch[ItemID.HerbBag] = 5;
                AddCategory("Crates", ItemID.HerbBag);
            }
            if (totalResearch[ItemID.LockBox] == 0)
            {
                totalResearch[ItemID.LockBox] = 5;
                AddCategory("Crates", ItemID.LockBox);
            }
            if (totalResearch[ItemID.EmptyBucket] == 0)
            {
                totalResearch[ItemID.EmptyBucket] = 25;
                AddCategory("Buckets", ItemID.EmptyBucket);
            }
            if (totalResearch[ItemID.WaterBucket] == 0)
            {
                totalResearch[ItemID.WaterBucket] = 1;
                AddCategory("Buckets", ItemID.WaterBucket);
            }
            if (totalResearch[ItemID.LavaBucket] == 0)
            {
                totalResearch[ItemID.LavaBucket] = 1;
                AddCategory("Buckets", ItemID.LavaBucket);
            }
            if (totalResearch[ItemID.HoneyBucket] == 0)
            {
                totalResearch[ItemID.HoneyBucket] = 1;
                AddCategory("Buckets", ItemID.HoneyBucket);
            }

            if (totalResearch[ItemID.BottomlessBucket] == 0)
            {
                totalResearch[ItemID.BottomlessBucket] = 1;
                AddCategory("Buckets", ItemID.BottomlessBucket);
            }

            if (totalResearch[ItemID.BlackLens] == 0)
            {
                totalResearch[ItemID.BlackLens] = 1;
                AddCategory("Materials", ItemID.BlackLens);
            }

            if (totalResearch[ItemID.IllegalGunParts] == 0)
            {
                totalResearch[ItemID.IllegalGunParts] = 1;
                AddCategory("Materials", ItemID.IllegalGunParts);
            }

            if (totalResearch[ItemID.DarkShard] == 0)
            {
                totalResearch[ItemID.DarkShard] = 1;
                AddCategory("Materials", ItemID.DarkShard);
            }

            if (totalResearch[ItemID.LightShard] == 0)
            {
                totalResearch[ItemID.LightShard] = 1;
                AddCategory("Materials", ItemID.LightShard);
            }

            if (totalResearch[ItemID.TurtleShell] == 0)
            {
                totalResearch[ItemID.TurtleShell] = 1;
                AddCategory("Materials", ItemID.TurtleShell);
            }

            if (totalResearch[ItemID.Ectoplasm] == 0)
            {
                totalResearch[ItemID.Ectoplasm] = 10;
                AddCategory("Materials", ItemID.Ectoplasm);
            }

            for (int i = ItemID.GiantHarpyFeather; i <= ItemID.TatteredBeeWing; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 1;
                    AddCategory("Materials", i);
                }
            }
            if (totalResearch[ItemID.ButterflyDust] == 0)
            {
                totalResearch[ItemID.ButterflyDust] = 1;
                AddCategory("Materials", ItemID.ButterflyDust);
            }
            if (totalResearch[ItemID.SpookyTwig] == 0)
            {
                totalResearch[ItemID.SpookyTwig] = 1;
                AddCategory("Materials", ItemID.SpookyTwig);
            }
            if (totalResearch[ItemID.BlackFairyDust] == 0)
            {
                totalResearch[ItemID.BlackFairyDust] = 1;
                AddCategory("Materials", ItemID.BlackFairyDust);
            }


            if (totalResearch[ItemID.GoodieBag] == 0)
            {
                totalResearch[ItemID.GoodieBag] = 5;
                AddCategory("Crates", ItemID.GoodieBag);
            }
            if (totalResearch[ItemID.Present] == 0)
            {
                totalResearch[ItemID.Present] = 5;
                AddCategory("Crates", ItemID.Present);
            }

            if (totalResearch[ItemID.FrostCore] == 0)
            {
                totalResearch[ItemID.FrostCore] = 3;
                AddCategory("Materials", ItemID.FrostCore);
            }

            if (totalResearch[ItemID.AncientBattleArmorMaterial] == 0)
            {
                totalResearch[ItemID.AncientBattleArmorMaterial] = 3;
                AddCategory("Materials", ItemID.AncientBattleArmorMaterial);
            }
            if (totalResearch[ItemID.BrokenHeroSword] == 0)
            {
                totalResearch[ItemID.BrokenHeroSword] = 1;
                AddCategory("Materials", ItemID.BrokenHeroSword);
            }
            int type = ResearchFrom14.getTypeFromTag("ThoriumMod:BrokenHeroFragment");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 1;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:MalignantThread");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:AlienTech");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Materials", type);
                }
            }
            
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:LifeCell");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 5;
                    AddCategory("Materials", type);
                }
            }

            type = ResearchFrom14.getTypeFromTag("ThoriumMod:StormFlare");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:KeyOfFire");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:KeyOfFungus");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:KeyOfTides");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
        }

        private static void AddCategory(string v, object wire)
        {
            throw new NotImplementedException();
        }

        private static void RT_addCalamity()
        {
            int type = ResearchFrom14.getTypeFromTag("CalamityMod:AncientBoneDust");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:DemonicBoneAsh");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Materials", type);
                }
            }

            type = ResearchFrom14.getTypeFromTag("CalamityMod:MurkySludge");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 10;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:MurkyPaste");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 10;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:ManeaterBulb");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 10;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:TrapperBulb");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 10;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:GrandScale");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:StormlionMandible");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:GypsyPowder");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 1;
                    AddCategory("Materials", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:DriedSeafood");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("CalamityMod:Teratoma");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }

            type = ResearchFrom14.getTypeFromTag("CalamityMod:OverloadedSludge");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 3;
                    AddCategory("Boss Bags and Summons", type);
                }
            }
        }

        private static void RT_addFish()
        {
            if (totalResearch[ItemID.Bass] == 0)
            {
                totalResearch[ItemID.Bass] = 5;
                AddCategory("Fish", ItemID.Bass);
            }

            for (int i = ItemID.Trout; i <= ItemID.Stinkfish; i++)
            {
                if (totalResearch[i] == 0)
                {
                    Item itm = new Item();
                    itm.SetDefaults(i);
                    totalResearch[i] = Math.Min(5,itm.maxStack);
                    AddCategory("Fish", i);
                }
            }

            for (int i = ItemID.OldShoe; i <= ItemID.TinCan; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 3;
                    AddCategory("Junk", i);
                }
            }
        }

        private static void RT_addTraps()
        {
            if (totalResearch[ItemID.DartTrap] == 0)
            {
                totalResearch[ItemID.DartTrap] = 5;
                AddCategory("Wire or Traps", ItemID.DartTrap);
            }

            for (int i = ItemID.GreenPressurePlate; i <= ItemID.BrownPressurePlate; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Wire or Traps", i);
                }
            }
            for (int i = ItemID.YellowPressurePlate; i <= ItemID.BluePressurePlate; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Wire or Traps", i);
                }
            }

            for (int i = ItemID.SuperDartTrap; i <= ItemID.LihzahrdPressurePlate; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Wire or Traps", i);
                }
            }
            
            if (totalResearch[ItemID.WeightedPressurePlatePink] == 0)
            {
                totalResearch[ItemID.WeightedPressurePlatePink] = 5;
                AddCategory("Wire or Traps", ItemID.WeightedPressurePlatePink);
            }

            for (int i = ItemID.WeightedPressurePlateOrange; i <= ItemID.WeightedPressurePlateCyan; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Wire or Traps", i);
                }
            }

            if (totalResearch[ItemID.RedPressurePlate] == 0)
            {
                totalResearch[ItemID.RedPressurePlate] = 5;
                AddCategory("Wire or Traps", ItemID.RedPressurePlate);
            }

            if (totalResearch[ItemID.ProjectilePressurePad] == 0)
            {
                totalResearch[ItemID.ProjectilePressurePad] = 5;
                AddCategory("Wire or Traps", ItemID.ProjectilePressurePad);
            }

            if (totalResearch[ItemID.GeyserTrap] == 0)
            {
                totalResearch[ItemID.GeyserTrap] = 5;
                AddCategory("Wire or Traps", ItemID.GeyserTrap);
            }

            if (totalResearch[ItemID.Explosives] == 0)
            {
                totalResearch[ItemID.Explosives] = 5;
                AddCategory("Wire or Traps", ItemID.Explosives);
            }

            if (totalResearch[ItemID.Detonator] == 0)
            {
                totalResearch[ItemID.Detonator] = 5;
                AddCategory("Wire or Traps", ItemID.Detonator);
            }

            for (int i = ItemID.Timer1Second; i <= ItemID.Timer5Second; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Wire or Traps", i);
                }
            }
            RT_addCalamityFish();
        }

        private static void RT_addCalamityFish()
        {
            List<int> fish = new List<int>()
            {
                ResearchFrom14.getTypeFromTag("CalamityMod:AldebaranAlewife"),
                ResearchFrom14.getTypeFromTag("CalamityMod:Bloodfin"),
                ResearchFrom14.getTypeFromTag("CalamityMod:BrimstoneFish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:ChaoticFish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:CharredLasher"),
                ResearchFrom14.getTypeFromTag("CalamityMod:CoastalDemonfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:CragBullhead"),
                ResearchFrom14.getTypeFromTag("CalamityMod:CoralskinFoolfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:EnchantedStarfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:FishOfEleum"),
                ResearchFrom14.getTypeFromTag("CalamityMod:FishOfFlight"),
                ResearchFrom14.getTypeFromTag("CalamityMod:FishOfLight"),
                ResearchFrom14.getTypeFromTag("CalamityMod:FishOfNight"),
                ResearchFrom14.getTypeFromTag("CalamityMod:GlimmeringGemfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:GreenwaveLoach"),
                ResearchFrom14.getTypeFromTag("CalamityMod:PrismaticGuppy"),
                ResearchFrom14.getTypeFromTag("CalamityMod:ProcyonidPrawn"),
                ResearchFrom14.getTypeFromTag("CalamityMod:ScarredAngelfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:Shadowfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:StuffedFish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:SunbeamFish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:SunkenSailfish"),
                ResearchFrom14.getTypeFromTag("CalamityMod:TwinklingPollox"),
                ResearchFrom14.getTypeFromTag("CalamityMod:Xerocodile")
            };

            foreach (int i in fish)
            {
                if (i >0 && totalResearch[i] == 0)
                {
                    totalResearch[i] = 5;
                    AddCategory("Fish", i);
                }
            }
        }

        private static void RT_addKeys()
        {
            if (totalResearch[ItemID.GoldenKey] == 0)
            {
                totalResearch[ItemID.GoldenKey] = 5;
                AddCategory("Keys", ItemID.GoldenKey);
            }
            if (totalResearch[ItemID.ShadowKey] == 0)
            {
                totalResearch[ItemID.ShadowKey] = 1;
                AddCategory("Keys", ItemID.ShadowKey);
            }
            for (int i = ItemID.JungleKey; i <= ItemID.FrozenKey; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 1;
                    AddCategory("Keys", i);
                }
            }
            int type = ResearchFrom14.getTypeFromTag("ThoriumMod:DesertBiomeKey");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 1;
                    AddCategory("Keys", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:UnderworldBiomeKey");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 1;
                    AddCategory("Keys", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:AquaticDepthsBiomeKey");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 1;
                    AddCategory("Keys", type);
                }
            }

            type = ResearchFrom14.getTypeFromTag("CalamityMod:RustyLockpick");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 5;
                    AddCategory("Keys", type);
                }
            }
        }

        private static void RT_addGems()
        {
            for(int i = ItemID.Sapphire; i<= ItemID.Diamond; i++)
            {
                if (totalResearch[i] == 0)
                {
                    totalResearch[i] = 15;
                    AddCategory("Gem", i);
                }
            }

            if (totalResearch[ItemID.Amber] == 0)
            {
                totalResearch[ItemID.Amber] = 15;
                AddCategory("Gem", ItemID.Amber);
            }
            int type = ResearchFrom14.getTypeFromTag("ThoriumMod:LifeQuartz");
            if ( type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Gem", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:Onyx");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Gem", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:Opal");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Gem", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:Pearl");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 15;
                    AddCategory("Gem", type);
                }
            }

            type = ResearchFrom14.getTypeFromTag("ThoriumMod:SandStone");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 25;
                    AddCategory("Bars", type);
                }
            }
            type = ResearchFrom14.getTypeFromTag("ThoriumMod:MagmaCore");
            if (type > 0)
            {
                if (totalResearch[type] == 0)
                {
                    totalResearch[type] = 25;
                    AddCategory("Bars", type);
                }
            }

        }
    }
}
