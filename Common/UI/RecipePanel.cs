
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ResearchFrom14.Common.Components;
using ResearchFrom14.Common.UI.Elements;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;

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
        
        public bool hasChanges = false;
        

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
            BackgroundColor = Color.LightGreen;
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
            
            //Append(internalGrid);
            hasChanges = true;
        }

        public override void Update(GameTime gameTime)
        {
            if(selected != changer.selected || selectedItem != parent.destroySlot.item || hasChanges)
            {
                selected = changer.selected;
                selectedItem = parent.destroySlot.item;
                internalGrid.Clear();
                internalGrid.Left.Set(0, 0);
                internalGrid.Top.Set(0, 0);
                internalGrid.Width.Set(this.Width.Pixels - 4, 0);
                internalGrid.Height.Set(this.Height.Pixels - 4, 0);

                ResearchPlayer player = Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>();
                List<Item> toDisplay = new List<Item>();
                foreach (int type in player.researchedCache)
                {
                    Item itm = new Item();
                    itm.SetDefaults(type);
                    itm.stack = 1;
                    if (parent.search.GetText() == null || parent.search.GetText().Length == 0 || itm.Name.ToLower().Contains(parent.search.GetText().ToLower()))
                    {
                        toDisplay.Add(itm);
                    }
                }
                toDisplay.Sort(new ItemNameComparer());

                if (selected.Equals(changer.allTree))
                {
                    foreach(Item itm in toDisplay)
                    {
                        internalGrid.Add(new PurchaseItemSlot(itm));
                    }
                }
                else
                {
                    if (ResearchTable.category.ContainsKey(selected.getFullPath()))
                    {
                       // ModLoader.GetMod("ResearchFrom14").Logger.Info("Category " + selected.getFullPath() + " has items:");
                        foreach(int cat in ResearchTable.category[selected.getFullPath()])
                        {
                            Item test = new Item();
                            test.SetDefaults(cat);
                         //   ModLoader.GetMod("ResearchFrom14").Logger.Info("  - " + test.Name + " ( id " + test.type + " = "+cat + ")" );   
                        }
                        foreach (Item itm in toDisplay)
                        {
                            if (ResearchTable.category[selected.getFullPath()].Contains(itm.type))
                                internalGrid.Add(new PurchaseItemSlot(itm));
                        }
                    }
                }
                internalGrid.Recalculate();
                Recalculate();

                hasChanges = false;
            }
            Recalculate();
            base.Update(gameTime);
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
