﻿
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ResearchFrom14.Common.Components;
using ResearchFrom14.Common.UI.Elements;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace ResearchFrom14.Common.UI
{

    public class RecipePanel : UIPanel
    {
        public UIGrid internalGrid;
        public UIScrollbar scrollbar;

        public ResearchUI parent;
        private PathTreePanel changer;
        public PathTree selected = null;
        public Item selectedItem = null;
        public int count = 0;
        public bool hasChanges = false;
        UIText loading = new UIText("Loading...");
        Task t = null;
        public string search = "";
        public bool tooltipSearch = false;
        public bool isSearching = false;
        public string mouseTooltip = "";

        public bool changedToList = false;
        public RecipePanel(ResearchUI panel)
        {
            parent = panel;
            changer = panel.categories;
            selected = changer.selected;
            hasChanges = true;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            BackgroundColor = Color.Blue;
            BackgroundColor.A = 196;
            BorderColor = Color.White;
            internalGrid = new UIGrid();
            internalGrid.OnScrollWheel += ResearchUI.onScrollWheel;
            //scrollbar = new UIScrollbar();
            scrollbar = new InvisibleScrollbar();
            Append(internalGrid);
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(0, 1f);
            scrollbar.Width.Set(1, 0);
            scrollbar.Left.Set(-14, 1f);
            Append(scrollbar);
            internalGrid.SetScrollbar(scrollbar);

            loading.VAlign = 0.5f;
            loading.HAlign = 0.45f;

            //Append(internalGrid);
            hasChanges = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(tooltipSearch != parent.tooltipSearch.doSearch)
            {
                tooltipSearch = parent.tooltipSearch.doSearch;
                changedToList = true;
            }

            if (selected != changer.selected || !parent.search.GetText().Equals(search) || changedToList)
            {
                if (t == null)
                {
                    selected = changer.selected;
                    search = parent.search.GetText();                    
                    RemoveChild(internalGrid);
                    loading.VAlign = 0.5f;
                    loading.HAlign = 0.45f;
                    Append(loading);
                    t = Task.Run(recreateList);
                }
                else
                {
                    selected = changer.selected;
                    search = parent.search.GetText();
                    changedToList = true;
                }
            }
            if (t != null)
            {
                if (t.IsCompleted)
                {
                    RemoveChild(loading);
                    Append(internalGrid);
                    hasChanges = true;
                    t.Dispose();
                    t = null;
                }
            }
            if (hasChanges)
            {
                if (t == null) { 
                    internalGrid.Left.Set(0, 0);
                    internalGrid.Top.Set(0, 0);
                    internalGrid.Width.Set(this.Width.Pixels - 4, 0);
                    internalGrid.Height.Set(this.Height.Pixels - 4, 0);
                    internalGrid.Recalculate();
                }
                Recalculate();
                hasChanges = false;
            }
            else
            {
                Recalculate();
            }
            base.Update(gameTime);
        }


        public void recreateList()
        {
            
            internalGrid.Left.Set(0, 0);
            internalGrid.Top.Set(0, 0);
            internalGrid.Width.Set(this.Width.Pixels - 4, 0);
            internalGrid.Height.Set(this.Height.Pixels - 4, 0);

            ResearchPlayer player = Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>();
            List<Item> toDisplay = new List<Item>();
            while (player.waitingForResearchCache())
            {
                Task.Yield();
            }
            restart:
            changedToList = false;
            toDisplay.Clear();
            internalGrid.Clear();

            foreach (int type in player.researchedCache)
            {
                Item itm = new Item();
                itm.SetDefaults(type);
                itm.stack = 1;
                if (parent.search.GetText() == null || parent.search.GetText().Trim().Length == 0 || itm.Name.ToLower().Contains(parent.search.GetText().ToLower()) ||
                    (tooltipSearch && condensedTooltip(itm).ToLower().Contains(parent.search.GetText().ToLower())))
                {
                    toDisplay.Add(itm);
                }
                if (changedToList)
                    goto restart;
            }
            toDisplay.Sort(new ItemNameComparer());

            if (selected.Equals(changer.allTree))
            {
                foreach (Item itm in toDisplay)
                {
                    internalGrid.Add(new PurchaseItemSlot(itm));
                    if (changedToList)
                        goto restart;
                }
            }
            else
            {
                if (ResearchTable.category.ContainsKey(selected.getFullPath()))
                {
                    // ModLoader.GetMod("ResearchFrom14").Logger.Info("Category " + selected.getFullPath() + " has items:");
                   /* foreach (int cat in ResearchTable.category[selected.getFullPath()])
                    {
                        Item test = new Item();
                        test.SetDefaults(cat);
                        //   ModLoader.GetMod("ResearchFrom14").Logger.Info("  - " + test.Name + " ( id " + test.type + " = "+cat + ")" );   
                    }*/
                    foreach (Item itm in toDisplay)
                    {
                        if (ResearchTable.category[selected.getFullPath()].Contains(itm.type))
                            internalGrid.Add(new PurchaseItemSlot(itm));
                        if (changedToList)
                            goto restart;
                    }
                }
            }
        }

        private string condensedTooltip(Item item)
        {
            mouseTooltip = "";
            isSearching = true;
            try
            {
            Item mouse = Main.HoverItem;
            Main.HoverItem = item;
            Main.instance.MouseText("");
            Main.HoverItem = mouse;
            }catch(Exception ex)
            {
                ModLoader.GetMod("ResearchFrom14").Logger.Info("Could not get tooltip for item " + item.type + ";");
            }
            isSearching = false;
            return mouseTooltip;
        }
    }
    public class ItemNameComparer : IComparer<Item>
    {
        public int Compare(Item x, Item y)
        {
           return x.Name.CompareTo(y.Name);
        }
    }
}
