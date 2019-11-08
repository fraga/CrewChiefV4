using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GameOverlay.Drawing;
using GameOverlay.PInvoke;
using static CrewChiefV4.Overlay.CrewChiefOverlayWindow;

namespace CrewChiefV4.Overlay
{
    class ElementRadioButton : OverlayElement
    {
        public bool enabled = false;
        private string costumCommand = null;
        public event EventHandler<OverlayElementClicked> OnElementLMButtonClicked;
        public ElementRadioButton(Graphics gfx, string elementTitle, Font font, System.Windows.Rect rectangle,
            OverlaySettings.ColorScheme colorScheme, EventHandler<OverlayElementClicked> OnElementLMButtonClicked = null, bool isChecked = false, string costumCommand = null) :
            base(gfx, elementTitle, font, rectangle, colorScheme)
        {
            if (OnElementLMButtonClicked != null)
            {
                this.OnElementLMButtonClicked += OnElementLMButtonClicked;
            }
            this.elementEnabled = true;
            this.costumCommand = costumCommand;
            this.enabled = isChecked;
            this.includeFontRectInMouseOver = true;
        }
        public override void initialize()
        {
            if (enabled)
                this.OnElementLMButtonClicked?.Invoke(this, new OverlayElementClicked(gfx, enabled, costumCommand));
        }
        public override void drawElement()
        {
             if (!this.elementEnabled)
                 return;
            Rect rect = base.rectangle;
            if(parent != null)
            {
                rect.Y += parent.rectangle.Y;
                rect.X += parent.rectangle.X;
            }

            float X = (float)rect.X + (float)(rect.Width / 2);
            float Y = (float)rect.Y + (float)(rect.Height / 2);
            gfx.DrawCircle(base.secondaryBrush, X, Y, (float)(rect.Width / 2), mouseOver ? 2 : 1);
            if (enabled)
            {
                gfx.DrawCircle(base.secondaryBrush, X, Y, 1, 5);
            }
            gfx.DrawText(base.font, 12, base.secondaryBrush, (float)rect.Right + 4, (float)rect.Y, base.title);
        }
        public override void OnWindowMessage(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            //base.OnWindowMessage(message, wParam, lParam);
            if(mouseOver)
            {
                if (message == WindowMessage.Lbuttondown)
                {
                    mousePressed = true;
                }
                if (message == WindowMessage.Lbuttonup && mousePressed)
                {
                    mousePressed = false;                    
                    if (!enabled)
                    {
                        enabled = !enabled;
                        if (parent != null)
                        {
                            foreach (ElementRadioButton child in parent.children.Where(c => c.GetType() == typeof(ElementRadioButton) && c != this))
                            {
                                child.enabled = !enabled;
                            }
                        }
                        this.OnElementLMButtonClicked?.Invoke(this, new OverlayElementClicked(gfx, enabled, costumCommand));
                    }                        
                }
            }
            else
            {
                mousePressed = false;
            }

        }
    }
}
