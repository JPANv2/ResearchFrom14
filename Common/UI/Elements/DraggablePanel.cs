using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using System;
using Terraria.ModLoader;

namespace ResearchFrom14.Common.UI.Elements
{
    public class DragableUIPanel : UIPanel
    {
        private Vector2 offset;
        private Vector2 mouseStart;
        public bool dragging;
        public bool resizing;
      
        public override void OnInitialize()
        {
            
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            base.MouseDown(evt);
            DragStart(evt);
        }

        public override void MouseUp(UIMouseEvent evt)
        {
            base.MouseUp(evt);
            DragEnd(evt);
        }

        public bool isInBorder(Vector2 pos)
        {
            return (pos.X > this.Left.Pixels && pos.X < this.Left.Pixels + (this.GetOuterDimensions().Width - this.GetInnerDimensions().Width) / 2) ||
                (pos.X < this.Left.Pixels + this.Width.Pixels && pos.X > this.Left.Pixels + this.Width.Pixels - (this.GetOuterDimensions().Width - this.GetInnerDimensions().Width) / 2) ||
                (pos.Y > this.Top.Pixels && pos.Y < this.Top.Pixels + (this.GetOuterDimensions().Height - this.GetInnerDimensions().Height) / 2) ||
                (pos.Y < this.Top.Pixels + this.Height.Pixels && pos.Y > this.Top.Pixels + this.Height.Pixels - (this.GetOuterDimensions().Height - this.GetInnerDimensions().Height) / 2);
        }

        public bool isInBottomRightCorner(Vector2 pos)
        {
            return 
                (pos.X < this.Left.Pixels + this.Width.Pixels && pos.X > this.Left.Pixels + this.Width.Pixels - (this.GetOuterDimensions().Width - this.GetInnerDimensions().Width) / 2) &&
                (pos.Y < this.Top.Pixels + this.Height.Pixels && pos.Y > this.Top.Pixels + this.Height.Pixels - (this.GetOuterDimensions().Height - this.GetInnerDimensions().Height) / 2);
        }

        private void DragStart(UIMouseEvent evt)
        {
            
            if (isInBottomRightCorner(evt.MousePosition))
            {
                resizing = true;
                mouseStart = evt.MousePosition;
            }else if(isInBorder(evt.MousePosition)){
                dragging = true;
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            }
        }

        private void DragEnd(UIMouseEvent evt)
        {
            dragging = false;
            resizing = false;
        }

        public override void Update(GameTime gameTime)
        {
        
            base.Update(gameTime); 
            // Checking ContainsPoint and then setting mouseInterface to true is very common. This causes clicks on this UIElement to not cause the player to use current items. 
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (resizing)
            {
                if (Main.mouseX != mouseStart.X)
                {
                    float totalDrag = Main.mouseX - mouseStart.X;
                    if(totalDrag > 1f || totalDrag < -1f)
                    {
                        mouseStart.X = Main.mouseX;
                        Width.Set(Math.Max(MinWidth.Pixels, Math.Min(MaxWidth.Pixels, Width.Pixels + totalDrag)),0);
                    }
                }
                if (Main.mouseY != mouseStart.Y)
                {
                    float totalDrag = Main.mouseY - mouseStart.Y;
                    if (totalDrag > 1f || totalDrag < -1f)
                    {
                        mouseStart.Y = Main.mouseY;
                        Height.Set(Math.Max(MinHeight.Pixels, Math.Min(MaxHeight.Pixels, Height.Pixels + totalDrag)), 0);
                    }
                }
                Recalculate();
                if(ResearchUI.visible)
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).ActivatePurchaseUI(Main.myPlayer);
                else if (PrefixUI.visible)
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).ActivatePrefixUI(Main.myPlayer);
            }
            else if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
                if (ResearchUI.visible)
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).ActivatePurchaseUI(Main.myPlayer);
                else if (PrefixUI.visible)
                    ((ResearchFrom14)ModLoader.GetMod("ResearchFrom14")).ActivatePrefixUI(Main.myPlayer);
            }

            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}
