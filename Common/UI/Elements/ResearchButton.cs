using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ResearchFrom14.Common.UI.Elements
{
    public class ResearchButton : UIPanel
    {

        private UIText buttonText;

        public override void OnInitialize()
        {
            buttonText = new UIText("Research", 0.50f);
            buttonText.HAlign = buttonText.VAlign = 0.5f;
            this.Append(buttonText);
            base.OnInitialize();
        }

        public override void Click(UIMouseEvent evt)
        {
            Main.player[Main.myPlayer].GetModPlayer<ResearchPlayer>().Research();
            ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).ActivatePurchaseUI(Main.myPlayer);
        }
    }
    public class ClearTextButton : UIPanel
    {
        public ClearTextButton(SearchUITextBox toClear) : base()
        {
            this.toClear = toClear;
        }

        private UIText buttonText;
        private SearchUITextBox toClear;

        public override void OnInitialize()
        {
            buttonText = new UIText("Clear Search", 0.50f);
            buttonText.HAlign = buttonText.VAlign = 0.5f;
            this.Append(buttonText);
            base.OnInitialize();
        }

        public override void Click(UIMouseEvent evt)
        {
            toClear.SetText("");
        }
    }

    public class TooltipSearchButton : UIPanel
    {

        private UIText buttonText;
        public bool doSearch = false;
        public override void OnInitialize()
        {
            buttonText = new UIText("Tooltip Search", 0.50f);
            buttonText.HAlign = buttonText.VAlign = 0.5f;
            this.Append(buttonText);
            this.BorderColor = Color.DarkGray; 
            base.OnInitialize();
        }

        public override void Click(UIMouseEvent evt)
        {
            doSearch = !doSearch;
            if (doSearch)
            {
                this.BorderColor = Color.LimeGreen;
                buttonText.TextColor = Color.LimeGreen;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
                buttonText.TextColor = Color.White;
            }

        }
    }
}
