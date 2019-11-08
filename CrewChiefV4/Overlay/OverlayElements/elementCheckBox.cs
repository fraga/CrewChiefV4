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
    public class ElementCheckBox : OverlayElement
    {
        public bool enabled = false;
        public string subscriptionDataField;
        public event EventHandler<OverlayElementClicked> OnElementLMButtonClicked;

        public ElementCheckBox(Graphics gfx, string elementTitle, Font font, Rect rectangle,
            OverlaySettings.ColorScheme colorScheme, EventHandler<OverlayElementClicked> OnElementLMButtonClicked = null, string subscriptionDataField = "", bool initialEnabled = false) 
            : base(gfx, elementTitle, font, rectangle, colorScheme)
        {
            if(OnElementLMButtonClicked != null)
            {
                this.OnElementLMButtonClicked += OnElementLMButtonClicked;
            }
            this.subscriptionDataField = subscriptionDataField;
            this.enabled = initialEnabled;
            this.includeFontRectInMouseOver = true;
        }
        public override void drawElement()
        {
            if (!this.elementEnabled)
                return;
            Rect rect = base.rectangle;
            if (parent != null)
            {
                rect.Y += parent.rectangle.Y;
                rect.X += parent.rectangle.X;
            }

            gfx.DrawBox2D(base.secondaryBrush, base.primaryBrush, new Rectangle(rect), mouseOver ? 2 : 1);
            gfx.DrawText(base.font, 12, base.secondaryBrush, (float)rect.Right + 4, (float)rect.Y, base.title);

            if (enabled)
            {
                rect.Y += (rectangle.Height / 2);
                rect.X += (rectangle.Width / 2);
                gfx.DrawCrosshair(base.secondaryBrush, (float)rect.X, (float)rect.Y, (float)(rectangle.Width / 2), 1, CrosshairStyle.Diagonal);                
            }
            return;
        }
        public override void OnWindowMessage(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            //base.OnWindowMessage(message, wParam, lParam);
            if (mouseOver)
            {
                if (message == WindowMessage.Lbuttondown)
                {
                    mousePressed = true;
                }
                if (message == WindowMessage.Lbuttonup && mousePressed)
                {
                    mousePressed = false;
                    enabled = !enabled;
                    this.OnElementLMButtonClicked?.Invoke(this, new OverlayElementClicked(gfx, enabled: enabled, subscriptionDataField: subscriptionDataField));
                }
            }
            else
            {
                mousePressed = false;
            }

        }
    }
}
