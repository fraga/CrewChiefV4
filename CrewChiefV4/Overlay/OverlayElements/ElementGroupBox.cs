﻿using System;
using System.Collections.Generic;
using GameOverlay.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CrewChiefV4.Overlay.CrewChiefOverlayWindow;

namespace CrewChiefV4.Overlay
{
    class ElementGroupBox : OverlayElement
    {
        bool outlined;
        public ElementGroupBox(Graphics gfx, string elementTitle, Font font, System.Windows.Rect rectangle,
            OverlaySettings.ColorScheme colorScheme, bool initialEnableState = true, bool outlined = true) :
            base(gfx, elementTitle, font, rectangle, colorScheme, initialEnableState)
        {
            parent = this;
            this.outlined = outlined;
        }
        public override void drawElement()
        {
            if (!this.elementEnabled)
                return;
                       
            if(outlined)
                gfx.DrawBox2D(base.secondaryBrush, base.primaryBrush, new Rectangle(base.rectangle), 1);
            else
                gfx.FillRectangle(base.primaryBrush, new Rectangle(base.rectangle));

            foreach (var child in children)
            {
                child.drawElement();
            }

            return;
        }
    }
}
