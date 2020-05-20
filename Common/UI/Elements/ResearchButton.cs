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

}
