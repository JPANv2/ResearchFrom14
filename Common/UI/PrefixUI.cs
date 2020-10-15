using Microsoft.Xna.Framework;
using ResearchFrom14.Common.UI.Elements;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ResearchFrom14.Common.UI
{
    public class PrefixUI: UIState
    {
        public static bool visible;
        public DragableUIPanel panel;
        public float oldScale;

        public UIItemSlot itemSlot;
        public PrefixScrollablePanel prefixPanel;
        public CloseButton closeButton;

        public override void OnInitialize()
        {           
            visible = false;

            panel = new DragableUIPanel();
            panel.BackgroundColor = Color.CornflowerBlue;
            panel.BorderColor = Color.White;

            panel.Top.Set(Main.screenHeight / 2 - 150, 0);
            panel.Left.Set(Main.screenWidth / 2 - 300, 0);
            panel.Width.Set(350, 0);
            panel.Height.Set(300, 0);
            panel.MinWidth.Set(350, 0);
            panel.MinHeight.Set(300, 0);
            panel.MaxWidth.Set(1920, 0);
            panel.MaxHeight.Set(1080, 0);

            itemSlot = new UIItemSlot(new Item());
            itemSlot.Top.Set(0, 0);
            itemSlot.Left.Set(0, 0);
            panel.Append(itemSlot);

            closeButton = new CloseButton();
            closeButton.Top.Set(0, 0);
            closeButton.Left.Set(panel.GetInnerDimensions().Width - 20, 0);
            panel.Append(closeButton);

            prefixPanel = new PrefixScrollablePanel();
            prefixPanel.Top.Set(itemSlot.Height.Pixels + 32, 0);
            prefixPanel.Left.Set(0, 0);
            prefixPanel.Width.Set(0, 1f);
            prefixPanel.Height.Set(0, 0.75f);
            panel.Append(prefixPanel);
            
            Append(panel);

        }

     

        public void resize()
        {

           
            itemSlot.Top.Set(0, 0);
            itemSlot.Left.Set(0, 0);
           
            closeButton.Top.Set(0, 0);
            closeButton.Left.Set(panel.GetInnerDimensions().Width - 20, 0);
          

            prefixPanel = new PrefixScrollablePanel();
            prefixPanel.Top.Set(itemSlot.Height.Pixels + 32, 0);
            prefixPanel.Left.Set(0, 0);
            prefixPanel.Width.Set(0, 1f);
            prefixPanel.Height.Set(0, 0.75f);
            /* prefixPanel.Width.Set(panel.GetInnerDimensions().Width, 0);
             prefixPanel.Height.Set(panel.GetInnerDimensions().Height - prefixPanel.Top.Pixels, 0);*/
        }

        public override void Update(GameTime gameTime)
        {
            if (itemSlot.item != prefixPanel.selected)
            {
                prefixPanel.hasChanged = true;
            }
            base.Update(gameTime);
            if (oldScale != Main.inventoryScale)
            {
                oldScale = Main.inventoryScale;
                Recalculate();
            }
        }

        public void setVisible(bool vis = true)
        {
            visible = vis;
            prefixPanel.hasChanged = true;
            resize();
        }

        public static void onScrollWheel(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 120);
        }

        internal static void onScrollWheelForUIText(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            Main.LocalPlayer.ScrollHotbar(Terraria.GameInput.PlayerInput.ScrollWheelDelta / 360);
        }
    }
}
