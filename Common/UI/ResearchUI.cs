using Microsoft.Xna.Framework;
using ResearchFrom14.Common.UI.Elements;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ResearchFrom14.Common.UI
{
    public class ResearchUI: UIState
    {
        public static bool visible;
        public DragableUIPanel panel;
        public float oldScale;

        public ResearchSlot destroySlot;
        public UIText totalText;
        public SearchUITextBox search;
        public ResearchButton destroyButton;
        public CloseButton closeButton;
        public PathTreePanel categories;
        public RecipePanel recipes;

        public override void OnInitialize()
        {           
            visible = false;

            panel = new DragableUIPanel();
            panel.BackgroundColor = Color.CornflowerBlue;
            panel.BorderColor = Color.White;

            panel.Top.Set(Main.screenHeight / 2 - 150, 0);
            panel.Left.Set(Main.screenWidth / 2 - 300, 0);
            panel.Width.Set(600, 0);
            panel.Height.Set(300, 0);
            panel.MinWidth.Set(600, 0);
            panel.MinHeight.Set(300, 0);
            panel.MaxWidth.Set(1920, 0);
            panel.MaxHeight.Set(1080, 0);

            destroySlot = new ResearchSlot(new Item());
            destroySlot.Top.Set(0, 0);
            destroySlot.Left.Set(0, 0);
            panel.Append(destroySlot);

            totalText = new UIText("", 0.75f);
            totalText.Top.Set(0, 0f);
            totalText.Left.Set(0, 0f);
            panel.Append(totalText);

            search = new SearchUITextBox();
            search.Top.Set(0, 0);
            search.Left.Set(destroySlot.Width.Pixels + destroySlot.MarginLeft + destroySlot.MarginRight + 112, 0);
            search.Width.Set(panel.GetInnerDimensions().Width - (destroySlot.Width.Pixels + destroySlot.MarginLeft + destroySlot.MarginRight + 12), 0);
            search.Height.Set(destroySlot.GetInnerDimensions().Height / 2, 0);
            search.OnTextChanged += () => setVisible(true);
            panel.Append(search);

            destroyButton = new ResearchButton();
            destroyButton.BackgroundColor = Color.Blue;
            destroyButton.Top.Set(search.Top.Pixels + search.Height.Pixels + 2, 0);
            destroyButton.Left.Set(destroySlot.Width.Pixels + 12, 0);
            destroyButton.Height.Set(destroySlot.GetInnerDimensions().Height / 2, 0);
            destroyButton.Width.Set(search.Width.Pixels/6, 0);

            panel.Append(destroyButton);

            closeButton = new CloseButton();
            closeButton.Top.Set(search.Top.Pixels + search.Height.Pixels + 2, 0);
            closeButton.Left.Set(panel.GetInnerDimensions().Width - 20, 0);
            panel.Append(closeButton);

            categories = new PathTreePanel();
            categories.Top.Set(destroySlot.Height.Pixels + 32, 0);
            categories.Left.Set(0, 0);
            categories.Width.Set(panel.GetInnerDimensions().Width / 3 - 4, 0);
            categories.Height.Set(panel.GetInnerDimensions().Height - categories.Top.Pixels, 0);
            panel.Append(categories);

            recipes = new RecipePanel(this);
            recipes.Top.Set(destroySlot.Height.Pixels + 12, 0);
            recipes.Left.Set(panel.Width.Pixels / 3, 0);
            recipes.Width.Set(panel.GetInnerDimensions().Width * 2 / 3, 0);
            recipes.Height.Set(panel.GetInnerDimensions().Height - (recipes.Top.Pixels + 32), 0);
            panel.Append(recipes);

            Append(panel);

        }

     

        public void resize()
        {
            destroySlot.Top.Set(0, 0);
            destroySlot.Left.Set(0, 0);

            search.Top.Set(0, 0);
            search.Left.Set(destroySlot.Width.Pixels + destroySlot.MarginLeft + destroySlot.MarginRight + 12, 0);
            search.Width.Set(panel.GetInnerDimensions().Width - (destroySlot.Width.Pixels + destroySlot.MarginLeft + destroySlot.MarginRight + 12), 0);
            search.Height.Set(destroySlot.GetInnerDimensions().Height / 2, 0);

            destroyButton.Top.Set(search.Top.Pixels + search.Height.Pixels + 2, 0);
            destroyButton.Left.Set(destroySlot.Width.Pixels + 12, 0);
            destroyButton.Height.Set(destroySlot.GetInnerDimensions().Height / 2, 0);
            destroyButton.Width.Set(search.Width.Pixels / 6, 0);

            totalText.Top.Set(destroySlot.Height.Pixels+6, 0f);
            totalText.Left.Set(20, 0f);

            closeButton.Top.Set(search.Top.Pixels + search.Height.Pixels + 2, 0);
            closeButton.Left.Set(panel.GetInnerDimensions().Width - 20, 0);

            categories.Top.Set(destroySlot.Height.Pixels + 24, 0);
            categories.Left.Set(0, 0);
            categories.Width.Set(panel.GetInnerDimensions().Width / 3 - 4, 0);
            categories.Height.Set(panel.GetInnerDimensions().Height - categories.Top.Pixels, 0);

            recipes.Top.Set(destroySlot.Height.Pixels + 12, 0);
            recipes.Left.Set(panel.Width.Pixels / 3, 0);
            recipes.Width.Set(panel.GetInnerDimensions().Width*2 / 3, 0);
            recipes.Height.Set(panel.GetInnerDimensions().Height - recipes.Top.Pixels, 0);
        }

        public override void Update(GameTime gameTime)
        {
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
            categories.hasChanged = true;
            recipes.hasChanges = true;
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

    public class CloseButton : UIText
    {
        public CloseButton():base("X") 
        {
            this.TextColor = Color.Red;
        }

        public override void Click(UIMouseEvent evt)
        {
            if (IsMouseHovering)
            {
                ResearchUI.visible = false;
            }
        }
    }
}
