using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOverlay.Drawing;
using GameOverlay.PInvoke;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace CrewChiefV4.Overlay
{
   
    class ElementImage : OverlayElement 
    {
        private Image image = null;
        float imageAlpha = 1;
        bool outlined = false;
        bool mmButton = false;
        public ChartContainer chartContainer;
        public event EventHandler<OverlayElementMouseWheel> OnElementMWheel;
        public event EventHandler<OverlayElementClicked> OnElementMMButtonClicked;
        public ElementImage(Graphics gfx, string elementTitle, Font font, System.Windows.Rect rectangle,ColorScheme colorScheme, 
            EventHandler<OverlayElementMouseWheel> OnElementMWheel = null, EventHandler<OverlayElementClicked> OnElementMMButtonClicked = null,
            ChartContainer chartContainer = null, float imageAlpha = 1, bool outlined = false) :
            base(gfx, elementTitle, font, rectangle, colorScheme)
        {
            this.chartContainer = chartContainer;
            this.imageAlpha = imageAlpha;
            this.outlined = outlined;
            this.OnElementMWheel += OnElementMWheel;
            this.OnElementMMButtonClicked += OnElementMMButtonClicked;
            if(this.chartContainer != null)
            {
                this.image = new Image(gfx, this.chartContainer.data);
            }
        }
        public void UpdateImage(ChartContainer newImage, Point point)
        {
            rectangle.X = point.X;
            rectangle.Y = point.Y;
            DisposeImage();
            image = new Image(gfx, newImage.data);
            title = newImage.subscriptionId;
        }
        public override void drawElement()
        {
            if (!this.elementEnabled)
               return;
            System.Windows.Rect rect = base.rectangle;
            if (parent != null)
            {
                rect.Y += parent.rectangle.Y;
                rect.X += parent.rectangle.X;
            }
            if(image!= null)
                gfx.DrawImage(image, new Point((float)rect.X, (float)rect.Y), imageAlpha);
            if(outlined)
                gfx.DrawRectangle(secondaryBrush, new Rectangle(rect), 1);
            return;
        }
        // this should really be called something else
        public override void Dispose()
        {
            base.Dispose();
        }

        public void DisposeImage()
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }
        public override void OnWindowMessage(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            if(mouseOver)
            {
                if (message == WindowMessage.Mousewheel)
                {
                    int UpDown = WindowNativeMethods.GET_WHEEL_DELTA_WPARAM(wParam);
                    OnElementMWheel?.Invoke(this, new OverlayElementMouseWheel(gfx, UpDown: UpDown));
                }
                if(message == WindowMessage.Mbuttondown)
                {
                    mmButton = true;
                }
                if (message == WindowMessage.Mbuttonup && mmButton)
                {
                    mmButton = false;
                    OnElementMMButtonClicked?.Invoke(this, new OverlayElementClicked(gfx));
                }
            }
            else
            {
                mmButton = false;
            }
        }
    }

}
