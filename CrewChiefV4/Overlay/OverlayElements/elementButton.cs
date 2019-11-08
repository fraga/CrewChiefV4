using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GameOverlay.Drawing;
using GameOverlay.PInvoke;
using GameOverlay.Windows;
using static CrewChiefV4.Overlay.CrewChiefOverlayWindow;

namespace CrewChiefV4.Overlay
{
    public class ElementButton : OverlayElement
    {
        public event EventHandler<OverlayElementClicked> OnElementLMButtonClicked;
        public ElementButton(Graphics gfx, string elementTitle, Font font, System.Windows.Rect rectangle,
            OverlaySettings.ColorScheme colorScheme, EventHandler<OverlayElementClicked> OnElementLMButtonClicked = null) :
            base( gfx, elementTitle, font, rectangle, colorScheme)
        {
            if(OnElementLMButtonClicked != null)
            {
                this.OnElementLMButtonClicked += OnElementLMButtonClicked;
            }            
        }

        public override void drawElement()
        {
            if (!this.elementEnabled)
                return;

            if (base.title == "ButtonClose" && !mouseOver)
            {
                gfx.DrawBox2D(base.secondaryBrush, base.primaryBrush, new Rectangle(base.rectangle), 1);
                gfx.DrawCrosshair(base.secondaryBrush, (float)rectangle.X+7, (float)rectangle.Y+7, 7, 1, CrosshairStyle.Diagonal);
            }
            else if(base.title == "ButtonClose")
            {
                gfx.DrawBox2D(base.primaryBrush, base.secondaryBrush, new Rectangle(base.rectangle), 1);
                gfx.DrawCrosshair(base.primaryBrush, (float)rectangle.X + 7, (float)rectangle.Y + 7, 7, 1, CrosshairStyle.Diagonal);
            }
            else
            {
                Rect rect = base.rectangle;
                if (parent != null)
                {
                    rect.Y += parent.rectangle.Y;
                    rect.X += parent.rectangle.X;
                }               
                gfx.DrawBox2D(base.secondaryBrush, base.primaryBrush, new Rectangle(rect), mouseOver ? 2 : 1);
                gfx.DrawTextCenterInRect(font, secondaryBrush, rect, title);
            }
            return;
        }
        public override void OnWindowMessage(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            if (mouseOver)
            {
                if (message == WindowMessage.Lbuttondown)
                {
                    mousePressed = true;
                }
                if (message == WindowMessage.Lbuttonup && mousePressed)
                {
                    mousePressed = false;
                    this.OnElementLMButtonClicked?.Invoke(this, new OverlayElementClicked(gfx));
                }
            }
            else
            {
                mousePressed = false;
            }

        }
    }
}
