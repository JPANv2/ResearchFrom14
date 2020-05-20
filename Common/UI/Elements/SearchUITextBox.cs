using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace ResearchFrom14.Common.UI.Elements
{
	public class SearchUITextBox : UIPanel
	{
		internal bool focused = false;

        public string hintText = "Search";
		internal string currentString = "";
		private int textBlinkerCount;
		private int textBlinkerState;

		public event Action OnFocus;

		public event Action OnUnfocus;

		public event Action OnTextChanged;

		public event Action OnTabPressed;

		public event Action OnEnterPressed;

		public SearchUITextBox(string text = "")
		{
			currentString = text;
			SetPadding(0);
			BackgroundColor = Color.White;
			BorderColor = Color.White;
		}

		public override void Click(UIMouseEvent evt)
		{
			Focus();
			base.Click(evt);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			base.RightClick(evt);
			SetText("");
		}

		public void Unfocus()
		{
			if (focused)
			{
				focused = false;
				Main.blockInput = false;

				OnUnfocus?.Invoke();
			}
		}

		public void Focus()
		{
			if (!focused)
			{
				Main.clrInput();
				focused = true;
				Main.blockInput = true;

				OnFocus?.Invoke();
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight))
			{	
				Unfocus();
			}
			base.Update(gameTime);
		}

		public void SetText(string text)
		{
			if (currentString != text)
			{
				currentString = text;
				OnTextChanged?.Invoke();
			}
		}

        public string GetText()
        {
            return currentString;
        }

		
		private static bool JustPressed(Keys key)
		{
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			base.DrawSelf(spriteBatch);

			if (focused)
			{
                PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (!newString.Equals(currentString))
				{
					currentString = newString;
					OnTextChanged?.Invoke();
				}
				else
				{
					currentString = newString;
				}

				if (JustPressed(Keys.Enter))
				{
					Main.drawingPlayerChat = false;
					Unfocus();
					OnEnterPressed?.Invoke();
				}
				if (JustPressed(Keys.Escape))
				{
					Unfocus();
					OnEnterPressed?.Invoke();
				}
				if (JustPressed(Keys.Tab))
				{
					Unfocus();
					OnEnterPressed?.Invoke();
				}
				if (++textBlinkerCount >= 20)
				{
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
				Main.instance.DrawWindowsIMEPanel(new Vector2(98f, (float)(Main.screenHeight - 36)), 0f);
			}
			string displayString = currentString;
			if (this.textBlinkerState == 1 && focused)
			{
				displayString = displayString + "|";
			}
			CalculatedStyle space = base.GetDimensions();
			Color color = Color.Black;
			if (currentString.Length == 0)
			{
			}
			Vector2 drawPos = space.Position() + new Vector2(4, 2);
			if (currentString.Length == 0 && !focused)
			{
				color *= 0.5f;
				spriteBatch.DrawString(Main.fontMouseText, hintText, drawPos, color);
			}
			else
            { 
				spriteBatch.DrawString(Main.fontMouseText, displayString, drawPos, color);
			}
		}
	}
}