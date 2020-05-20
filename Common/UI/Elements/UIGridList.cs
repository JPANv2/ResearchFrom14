using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace ResearchFrom14.Common.UI.Elements
{
    public class UIGridList : UIElement
    {

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public UIGridList()
        {
            this._innerList.OverflowHidden = false;
            this._innerList.Width.Set(0f, 1f);
            this._innerList.Height.Set(0f, 1f);
            this.OverflowHidden = true;
            base.Append(this._innerList);
        }


        public float GetTotalHeight()
        {
            return this._innerListHeight;
        }


        public void Goto(UIGrid.ElementSearchMethod searchMethod, bool center = false)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                if (searchMethod(this._items[i]))
                {
                    this._scrollbar.ViewPosition = this._items[i].Top.Pixels;
                    if (center)
                    {
                        this._scrollbar.ViewPosition = this._items[i].Top.Pixels - base.GetInnerDimensions().Height / 2f + this._items[i].GetOuterDimensions().Height / 2f;
                    }
                    return;
                }
            }
        }


        public virtual void Add(UIElement item)
        {
            this._items.Add(item);
            this._innerList.Append(item);
            //this.UpdateOrder();
            this.UpdateScrollbar();
            this._innerList.Recalculate();
        }


        public virtual void AddRange(IEnumerable<UIElement> items)
        {
            this._items.AddRange(items);
            foreach (UIElement item in items)
            {
                this._innerList.Append(item);
            }
            //this.UpdateOrder();
            this.UpdateScrollbar();
            this._innerList.Recalculate();
        }


        public virtual bool Remove(UIElement item)
        {
            this._innerList.RemoveChild(item);
            //this.UpdateOrder();
            this.UpdateScrollbar();
            return this._items.Remove(item);
        }


        public virtual void Clear()
        {
            this._innerList.RemoveAllChildren();
            this._items.Clear();
        }


        public override void Recalculate()
        {
            base.Recalculate();
            this.UpdateScrollbar();
        }


        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            if (this._scrollbar != null)
            {
                this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue/4;
            }
        }


        public override void RecalculateChildren()
        {
            float availableWidth = base.GetInnerDimensions().Width;
            base.RecalculateChildren();
            float top = 0f;
           // float left = 0f;
            float maxRowHeight = 0f;
            for (int i = 0; i < this._items.Count; i++)
            {
                UIElement uielement = this._items[i];
                CalculatedStyle outerDimensions = uielement.GetOuterDimensions();
                top += maxRowHeight + this.ListPadding;
               // left = 0f;
                maxRowHeight = 0f;
                maxRowHeight = Math.Max(maxRowHeight, outerDimensions.Height);
                //uielement.Left.Set(left, 0f);
                //left += outerDimensions.Width + this.ListPadding;
                uielement.Top.Set(top, 0f);
            }
            this._innerListHeight = top + maxRowHeight;
        }


        private void UpdateScrollbar()
        {
            if (this._scrollbar == null)
            {
                return;
            }
            this._scrollbar.SetView(base.GetInnerDimensions().Height, this._innerListHeight);
        }


        public void SetScrollbar(UIScrollbar scrollbar)
        {
            this._scrollbar = scrollbar;
            this.UpdateScrollbar();
        }


        public void UpdateOrder()
        {
            this._items.Sort(new Comparison<UIElement>(this.SortMethod));
            this.UpdateScrollbar();
        }


        public int SortMethod(UIElement item1, UIElement item2)
        {
            return item1.CompareTo(item2);
        }


        public override List<SnapPoint> GetSnapPoints()
        {
            List<SnapPoint> list = new List<SnapPoint>();
            SnapPoint item;
            if (base.GetSnapPoint(out item))
            {
                list.Add(item);
            }
            foreach (UIElement current in this._items)
            {
                list.AddRange(current.GetSnapPoints());
            }
            return list;
        }

        float scrollPos = 0f;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (this._scrollbar != null)
            {
                this._innerList.Top.Set(-this._scrollbar.GetValue(), 0f);
            }
        }

        
        public List<UIElement> _items = new List<UIElement>();

        
        protected UIScrollbar _scrollbar;

        
        internal UIElement _innerList = new UIInnerList();

      
        private float _innerListHeight;

        
        public float ListPadding = 5f;

        public delegate bool ElementSearchMethod(UIElement element);

        // Token: 0x0200047F RID: 1151
        private class UIInnerList : UIElement
        {
            // Token: 0x0600294D RID: 10573 RVA: 0x00491CC8 File Offset: 0x0048FEC8
            public override bool ContainsPoint(Vector2 point)
            {
                return true;
            }

            // Token: 0x0600294E RID: 10574 RVA: 0x00491CCC File Offset: 0x0048FECC
            protected override void DrawChildren(SpriteBatch spriteBatch)
            {
                Vector2 position = this.Parent.GetDimensions().Position();
                Vector2 dimensions = new Vector2(this.Parent.GetDimensions().Width, this.Parent.GetDimensions().Height);
                foreach (UIElement current in this.Elements)
                {
                    Vector2 position2 = current.GetDimensions().Position();
                    Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
                    if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
                    {
                        current.Draw(spriteBatch);
                    }
                }
            }
        }
    }
}
