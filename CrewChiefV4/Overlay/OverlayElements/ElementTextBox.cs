using GameOverlay.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static CrewChiefV4.Overlay.CrewChiefOverlayWindow;

namespace CrewChiefV4.Overlay
{
    class ElementTextBox: OverlayElement
    {
        EventHandler<OverlayElementDrawUpdate> OnElementUpdate = null;

        public ElementTextBox(Graphics gfx, string elementTitle, Font font, Rect rectangle,
            OverlaySettings.ColorScheme colorScheme, EventHandler<OverlayElementDrawUpdate> OnElementUpdate = null, bool initialEnableState = true) :
            base(gfx, elementTitle, font, rectangle, colorScheme, initialEnableState)
        {
            if(OnElementUpdate != null)
            {
                this.OnElementUpdate += OnElementUpdate;
            }
            parent = this;
        }
        public override void drawElement()
        {
            if (!this.elementEnabled)
                return;
            gfx.DrawBox2D(base.secondaryBrush, base.primaryBrush, new Rectangle(base.rectangle), 1);
            OnElementUpdate?.Invoke(this, new OverlayElementDrawUpdate(gfx, new Rectangle(base.rectangle)));
            foreach (var child in children)
            {
                child.drawElement();
            }
            return;
        }
    }
}
