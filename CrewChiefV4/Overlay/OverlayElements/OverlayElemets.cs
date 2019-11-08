using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using GameOverlay.PInvoke;
using static CrewChiefV4.Overlay.CrewChiefOverlayWindow;

namespace CrewChiefV4.Overlay
{
    #region event args
    public class OverlayElementClicked : EventArgs
    {

        private OverlayElementClicked()
        {
        }
        public bool enabled { get; private set; }
        public string subscriptionDataField { get; private set; }
        public Graphics graphics { get; private set; }
        public OverlayElementClicked(Graphics graphics, bool enabled = false, string subscriptionDataField = null)
        {
            this.enabled = enabled;
            this.subscriptionDataField = subscriptionDataField;
            this.graphics = graphics;
        }        
    }
    public class OverlayElementMouseWheel : EventArgs
    {

        private OverlayElementMouseWheel()
        {
        }

        public Graphics graphics { get; private set; }
        public int UpDown { get; private set; }
        
        public OverlayElementMouseWheel(Graphics graphics, int UpDown)
        {
            this.graphics = graphics;
            this.UpDown = UpDown;
        }
    }

    public class OverlayElementDrawUpdate: EventArgs
    {

        private OverlayElementDrawUpdate()
        {
        }
        public Graphics graphics { get; private set; }
        public Rectangle rect { get; private set; }
        public OverlayElementDrawUpdate(Graphics graphics, Rectangle rect)
        {
            this.graphics = graphics;
            this.rect = rect;
        }
    }
    #endregion

    public class OverlayElement
    {
        public string title;
        public Rect rectangle;
        public Rect fontRectangle;
        public SolidBrush primaryBrush;
        public SolidBrush secondaryBrush;
        public bool mousePressed;
        public bool mouseOver;
        public Font font;
        public Graphics gfx;
        public OverlayElement parent;
        public List<OverlayElement> children { get; private set; }
        public OverlaySettings.ColorScheme colorScheme;
        public bool elementEnabled;
        public System.Windows.Point mousePosition = new System.Windows.Point(0, 0);
        public bool includeFontRectInMouseOver = false;
        public OverlayElement(Graphics gfx, string elementTitle, Font font, Rect rectangle, OverlaySettings.ColorScheme colorScheme, bool initialState = true)
        {
            this.title = elementTitle;
            this.rectangle = rectangle;
            this.colorScheme = colorScheme;
            this.primaryBrush = gfx.CreateSolidBrush(colorScheme.backgroundColor);
            this.secondaryBrush = gfx.CreateSolidBrush(colorScheme.fontColor);
            this.font = font;
            this.gfx = gfx;
            this.children = new List<OverlayElement>();
            this.elementEnabled = initialState;
            System.Drawing.SizeF fontSize = font.MeasureString(elementTitle, font.FontSize);
            this.fontRectangle = new Rect(rectangle.Right, rectangle.Y, fontSize.Width + 4, fontSize.Height);
        }
        public virtual void initialize()
        {
            foreach (var element in children)
            {
                element.initialize();
            }
        }
        public virtual void updateInputs(int overlayWindowX, int overlayWindowY)
        {
        }
        public virtual void drawElement()
        {
        }
        public virtual void Dispose() { }
        public virtual OverlayElement AddChildElement(OverlayElement child)
        {
            child.parent = this;
            children.Add(child);
            return children.Last();
        }
        public virtual void OnWindowMessage(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            if (!elementEnabled)
                return;
            bool mouseOverChild = false;
            if (message == WindowMessage.Mousemove)
            {                
                mousePosition.X = WindowNativeMethods.GET_X_LPARAM(lParam);
                mousePosition.Y = WindowNativeMethods.GET_Y_LPARAM(lParam);
                foreach (var child in children)
                {
                    System.Windows.Rect rect = child.rectangle;
                    System.Windows.Rect fontRect = child.fontRectangle;
                    if (parent != null)
                    {
                        rect.Y += parent.rectangle.Y;
                        rect.X += parent.rectangle.X;
                        fontRect.Y += parent.rectangle.Y;
                        fontRect.X += parent.rectangle.X;
                    }
                    if (rect.Contains(mousePosition))
                    {
                        child.mouseOver = true;
                        mouseOverChild = true;
                    }
                    else if(child.includeFontRectInMouseOver && fontRect.Contains(mousePosition))
                    {
                        child.mouseOver = true;
                        mouseOverChild = true;
                    }
                    else
                    {
                        child.mouseOver = false;
                    }
                }
            }
            if(!mouseOverChild)
            {
                if (rectangle.Contains(mousePosition) || (includeFontRectInMouseOver && fontRectangle.Contains(mousePosition)))
                {
                    mouseOver = true;
                }
                else
                {
                    mouseOver = false;
                }
            }
            else
            {
                mouseOver = false;
            }
            foreach (var child in children)
            {
                child.OnWindowMessage(message, wParam, lParam);
            }
        }
    }
}
