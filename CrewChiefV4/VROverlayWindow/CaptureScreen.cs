
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Graphics = System.Drawing.Graphics;
using Device = SharpDX.Direct3D11.Device;
using SharpDX;
using D2D = SharpDX.Direct2D1;
using Valve.VR;
using SharpDX.Mathematics.Interop;
using CrewChiefV4;
using CrewChiefV4.Events;
using CrewChiefV4.VirtualReality;

namespace CrewChiefV4.ScreenCapture
{
    public class Direct3D11CaptureSource : IDisposable
    {
        private OutputDuplicationSource[] outputDuplications;
        private Device device;
        private DeviceManager deviceManager;
        private CVRSystem _vrSystem = null;
        private VROverlayConfiguration _VRConfig = null;

        public Direct3D11CaptureSource(DeviceManager deviceManager, VROverlayConfiguration config, CVRSystem vrSystem)
        {
            _VRConfig = config;
            device = deviceManager.device;
            this.deviceManager = deviceManager;
            _vrSystem = vrSystem;
            Initialize();
        }

        private void Initialize()
        {
            int adapterIndex = 0;
            if (_vrSystem != null)
            {
                _vrSystem.GetDXGIOutputInfo(ref adapterIndex);
            }

            using (var factory = new Factory4())
            using (var adapter = factory.GetAdapter(adapterIndex))
            {
                // properties in SharpDX are very deceptive
                // calling adapter.Outputs will construct N new instances of Output that must be disposed by OutputDuplicationSource
                outputDuplications = adapter.Outputs.Select(o => OutputDuplicationSource.FromOutput(device, o)).ToArray();
            }
        }

        private void OnDeviceAccessLost()
        {
            Dispose();
            Initialize();
        }

        public void Capture(List<VROverlayWindow> windows)
        {
            if (device == null)
            {
                return;
            }

            foreach (var dub in outputDuplications)
            {
                bool captureDone = false;
                List<VROverlayWindow> currentBatch = new List<VROverlayWindow>();
                foreach (VROverlayWindow w in windows)
                {
                    var info = new Win32Stuff.WINDOWINFO();
                    info.cbSize = (uint)Marshal.SizeOf(info);
                    if (!w.isDisplay)
                    {
                        var gotWindowInfo = Win32Stuff.GetWindowInfo(w.hWnd, ref info);
                        if (!gotWindowInfo || info.dwStyle.HasFlag(Win32Stuff.WindowStyle.Iconic) || !info.dwStyle.HasFlag(Win32Stuff.WindowStyle.Visible))
                            continue;

                        if (info.rcClient.Width < 1 || info.rcClient.Height < 1)
                            continue;

                        if (!dub.Contains(info.rcClient))
                            continue;
                        if (w.forceTopMost)
                            w.MakeTopmost();
                    }
                    else if (w.Name != dub.deviceName)
                        continue;


                    // get the screen size for mouse capture
                    w.rectScreen = w.isDisplay
                                   ? dub.rectangle.AsSharpDX()
                                   : info.ToScreenRect();

                    // update the window size for display, may need to create a new texture
                    var rect = w.isDisplay
                                    ? new SharpDX.Rectangle(0, 0, dub.width, dub.height)
                                    : info.ToAbsScreenRect(dub.rectangle);

                    w.TryUpdateSize(rect, () => new Texture2D(device, new Texture2DDescription
                    {
                        CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                        BindFlags = BindFlags.None,
                        Format = dub.outputDuplication.Description.ModeDescription.Format,
                        Width = w.rectAbs.Width,
                        Height = w.rectAbs.Height,
                        OptionFlags = ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Staging
                    })
                    {
                        DebugName = nameof(w.copiedScreenTexture)
                    });

                    currentBatch.Add(w);
                }

                if (currentBatch.Count < 1)
                    continue;

                const int MAX_CAPTURE_RETRY_COUNT = 5;
                int captureRetry = 0;
                for (; !captureDone && captureRetry < MAX_CAPTURE_RETRY_COUNT; captureRetry++)
                {
                    if (!VROverlayController.vrUpdateThreadRunning || VROverlayController.vrOverlayRenderThreadSuspended)
                        return;

                    try
                    {
                        OutputDuplicateFrameInformation duplicateFrameInformation;
                        var result = dub.outputDuplication.TryAcquireNextFrame(100, out duplicateFrameInformation, out SharpDX.DXGI.Resource screenResource);
                        if (result.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                        {
                            OnDeviceAccessLost();
                            Console.WriteLine("CaptureScreen.Capture: device access lost = " + SharpDX.DXGI.ResultCode.AccessLost.ApiCode);
                            continue;
                        }
                        else if (result.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                        {
                            continue;
                        }
                        else if (result.Success)
                        {
                            using (var screenTexture = screenResource.QueryInterface<Texture2D>())
                            {
                                screenTexture.DebugName = "screenTexture";
                                foreach (var overlayWindow in currentBatch)
                                {
                                    using (var imageProcessor = new OverlayDecorator(this, overlayWindow, dub)
                                    {
                                        HighlightColor = ColorTranslator.FromHtml(_VRConfig.HighlightColor),
                                        MousePoint = overlayWindow.GetMousePoint()
                                    })
                                    {
                                        imageProcessor.Draw(screenTexture);
                                    }

                                    overlayWindow.shouldDraw = true;
                                }
                            }
                            captureDone = true;
                            screenResource?.Dispose();
                            screenResource = null;
                            dub.outputDuplication.ReleaseFrame();
#if DEBUG
                            Trace.WriteLine($"ObjectTracker.FindActiveObjects: {SharpDX.Diagnostics.ObjectTracker.FindActiveObjects().Count}");
#endif
                        }
                        else if (result.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                        {
                            Console.WriteLine("CaptureScreen.Capture: screen capturing failed = " + result.Code.ToString());
                        }
                    }
                    catch (SharpDXException e)
                    {
                        if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                        {
                            OnDeviceAccessLost();
                            Console.WriteLine("CaptureScreen.Capture: device access lost = " + e.Message);

                            return;  // OnDeviceAccessLost modifies outputDuplicationSource collection.  Simply return and next frame will pick refresh up correctly.
                        }
                        else if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                        {
                            Console.WriteLine("CaptureScreen.Capture: screen capturing failed = " + e.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("CaptureScreen.Capture: screen capturing failed = " + ex.Message);
                        if (dub.outputDuplication == null
                            || dub.outputDuplication.NativePointer == IntPtr.Zero
                            || dub.outputDuplication.IsDisposed)
                        {
                            OnDeviceAccessLost();
                            Console.WriteLine("CaptureScreen.Capture: lost outputDuplication");

                            return;  // OnDeviceAccessLost modifies outputDuplicationSource collection.  Simply return and next frame will pick refresh up correctly.
                        }
                    }
                }

                if (captureRetry == MAX_CAPTURE_RETRY_COUNT)
                {
                    // It turns out that depending on what is in the focus, TryAcquireNextFrame
                    // can timeout getting the next frame, indefinitely.  Notably, if SimHub or other WPF window is in the focus,
                    // it times out until something changes on the screen, for example, mouse cursor moves.  That means, that we won't process
                    // toggle requests, indefinitely.  So as a workaround, return here if we exceeded
                    // the retry threashold.
                    //
                    // Display all old overlays using old textures, until the next successful refresh.
#if DEBUG
                    Console.WriteLine("CaptureScreen.Capture: Capture retry count exceeded.");
#endif
                    foreach (var w in currentBatch)
                    {
                        // Pretend we fully succeeded, outherwise VROverlayWindow.Draw will hide the overlay.
                        w.shouldDraw = true;
                    }
                }
            }

            return;
        }

        public void Dispose()
        {
            if (outputDuplications != null)
            {
                foreach (var dub in outputDuplications)
                {
                    try
                    {
                        dub.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // This still crashes, wtf?
                        Utilities.ReportException(ex, "Direct3D11CaptureSource.Dispose crashed.", needReport: false);
                    }
                }
            }
            outputDuplications = null;

            //copiedScreenTexture.Dispose();
            //copiedScreenTexture = null;
        }

        internal class OutputDuplicationSource : IDisposable
        {
            public OutputDuplication outputDuplication { get; private set; }
            public System.Drawing.Rectangle rectangle;
            public string deviceName;
            public int width;
            public int height;
            Output output;

            public OutputDuplicationSource(OutputDuplication outputSource, Output output)
                : base()
            {
                this.output = output;
                var rectangle = output.Description.DesktopBounds;
                var deviceName = output.Description.DeviceName;
                this.outputDuplication = outputSource;

                this.rectangle = System.Drawing.Rectangle.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
                this.deviceName = deviceName;
                Console.WriteLine($" {deviceName}: X = {rectangle.Left } " +
                    $" Y = { rectangle.Top } " +
                    $" Right = { this.rectangle.Right } " +
                    $" Bottom = { this.rectangle.Bottom } " +
                    $"Width = { this.rectangle.Width } " +
                    $"Height = { this.rectangle.Height } ");
                width = Math.Abs(rectangle.Right - rectangle.Left);
                height = Math.Abs(rectangle.Bottom - rectangle.Top);
            }

            public bool Contains(RECT rect)
            {
                return rectangle.Contains(System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom));
            }                     

            public void Dispose()
            {
                if (this.outputDuplication != null
                           && this.outputDuplication.NativePointer != IntPtr.Zero
                           && !this.outputDuplication.IsDisposed)
                {
                    this.outputDuplication.Dispose();
                    //this.outputDuplication = null;
                }
                output?.Dispose();
                output = null;
            }

            public static OutputDuplicationSource FromOutput(Device device, Output output)
            {
                using (var output1 = output.QueryInterface<Output1>())
                {
                    return new OutputDuplicationSource(output1.DuplicateOutput(device), output); ;
                }
            }
        }

        public class OverlayDecorator : IDisposable
        {
            public static IntPtr cursorIcon = Win32Stuff.LoadCursor(IntPtr.Zero, (int)Win32Stuff.IDC_STANDARD_CURSORS.IDC_ARROW);
            public DeviceManager Devices { get; set; }
            public VROverlayWindow Overlay { get; set; }
            public Size2 Size { get; private set; }
            public System.Drawing.Color HighlightColor { get; set; } = System.Drawing.Color.Yellow;
            public System.Drawing.Point MousePoint { get; set; } = System.Drawing.Point.Empty;

            List<IDisposable> _disposables = new List<IDisposable>();
            Format BitmapFormat = Format.B8G8R8A8_UNorm;

            internal OverlayDecorator(Direct3D11CaptureSource source, VROverlayWindow overlay, OutputDuplicationSource duplication)
                : this(source, overlay, overlay.isDisplay
                          ? new Size2(duplication.width, duplication.height)
                          : new Size2(overlay.rectAbs.Width, overlay.rectAbs.Height))
            {
                BitmapFormat = duplication.outputDuplication.Description.ModeDescription.Format;
            }

            public OverlayDecorator(Direct3D11CaptureSource source, VROverlayWindow overlay, Size2 size)
                : this(source.deviceManager, overlay, size)
            {

            }

            public OverlayDecorator(DeviceManager deviceManager, VROverlayWindow overlay, Size2 size)
            {
                Devices = deviceManager;
                Overlay = overlay;
                Size = size;
            }

            public Texture2D NewTexture(ResourceOptionFlags flags = ResourceOptionFlags.None, Format format = Format.B8G8R8A8_UNorm, string debugName = "")
            {
                var texture = new Texture2D(Devices.device, new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = Size.Width,
                    Height = Size.Height,
                    OptionFlags = flags,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default
                })
                {
                    DebugName = debugName
                };
                _disposables.Add(texture);
                return texture;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="screenTexture"></param>
            public void Draw(Texture2D screenTexture)
            {
                // simple case, nothing to draw, just copy the texture
                if (MousePoint.IsEmpty && !Overlay.Chromakey && !Overlay.IsSelected)
                {
                    CopyScreen(screenTexture, Overlay.copiedScreenTexture);
                    return;
                }

                // if we're only drawing the mouse, then the target texture should be GdiCompatible
                bool shouldDraw = Overlay.Chromakey || Overlay.IsSelected;
                var flags = !MousePoint.IsEmpty && !shouldDraw
                                            ? ResourceOptionFlags.GdiCompatible
                                            : ResourceOptionFlags.None;

                // first create a new texture that we can draw to and copy the screen to it
                Texture2D source = screenTexture;
                Texture2D target = NewTexture(flags, BitmapFormat, $"{Overlay.Name}-DrawTexture");
                CopyScreen(source, target);

                if (!MousePoint.IsEmpty && !shouldDraw)
                {
                    DrawMouseIcon(target, Overlay.copiedScreenTexture);
                    return;
                }

                // in the case we have to draw the mouse and/or Chromakey/IsSelected
                // then we need to Draw to a Gdi texture 
                // otherwise we can render straight to the Overlay.copiedScreenTexture
                source = target;
                target = MousePoint.IsEmpty
                    ? Overlay.copiedScreenTexture
                    : NewTexture(ResourceOptionFlags.GdiCompatible, debugName: $"{Overlay.Name}-GdiTexture");

                if (shouldDraw)
                    Draw2D(source, target);

                if (!MousePoint.IsEmpty)
                {
                    DrawMouseIcon(target, Overlay.copiedScreenTexture);
                }
            }

            OverlayDecorator Draw2D(Texture2D source, Texture2D target)
            {
                var pixelFormat = new D2D.PixelFormat(Format.B8G8R8A8_UNorm, D2D.AlphaMode.Premultiplied);
                using (var drawingTexture = NewTexture())
                using (var drawingSurface = drawingTexture.QueryInterface<Surface1>())
                using (var sourceBitmap = new D2D.Bitmap1(Devices.DeviceContext2D, drawingSurface))
                using (var bufferBitmap = new D2D.Bitmap1(Devices.DeviceContext2D, Size, new D2D.BitmapProperties1 { PixelFormat = pixelFormat }))
                using (var drawingBitmap = new D2D.Bitmap1(Devices.DeviceContext2D, Size, new D2D.BitmapProperties1 { PixelFormat = pixelFormat, BitmapOptions = D2D.BitmapOptions.Target | D2D.BitmapOptions.CannotDraw }))
                {
                    Devices.device.ImmediateContext.CopyResource(source, drawingTexture);

                    // Pull the screen capture into a D2D bitmap object then copy it to the buffer                                                        
                    bufferBitmap.CopyFromBitmap(sourceBitmap);

                    Devices.DeviceContext2D.Target = drawingBitmap;

                    Devices.DeviceContext2D.BeginDraw();
                    Devices.DeviceContext2D.Clear(SharpDX.Color.Transparent);

                    if (Overlay.Chromakey)
                    {
                        using (var chromaKey = new ChromaKey(Devices.DeviceContext2D)
                        {
                            Tolerance = Overlay.Chromakey ? (float)Overlay.ChromakeyTolerance : 0,
                            Color = ColorTranslator.FromHtml(Overlay.ChromakeyColor).ToRawColor3()
                        })
                        {
                            chromaKey.SetInput(0, bufferBitmap, false);
                            Devices.DeviceContext2D.DrawImage(chromaKey);
                        }
                    }

                    if (Overlay.IsSelected)
                    {
                        if (!Overlay.Chromakey)
                            Devices.DeviceContext2D.DrawBitmap(bufferBitmap, 1.0f, D2D.InterpolationMode.Linear);

                        using (var brush = new D2D.SolidColorBrush(Devices.DeviceContext2D, HighlightColor.ToRawColor4()))
                            Devices.DeviceContext2D.FillRectangle(new RawRectangleF(0, 0, Size.Width - 1, 3), brush);
                    }

                    Devices.DeviceContext2D.EndDraw();

                    using (var sur = drawingBitmap.Surface)
                    using (var targetTexture2D = sur.QueryInterface<Texture2D>())
                        Devices.device.ImmediateContext.CopyResource(targetTexture2D, target);
                }
                return this;
            }

            public OverlayDecorator DrawMouseIcon(Texture2D cursorIconTarget, Texture2D target)
            {
                if (MousePoint.IsEmpty)
                    return this;

                using (var gidSurface = cursorIconTarget.QueryInterface<Surface1>())
                {
                    var HDC = gidSurface.GetDC(false);
                    Win32Stuff.DrawIconEx(HDC, MousePoint.X, MousePoint.Y, cursorIcon, 0, 0, 0, IntPtr.Zero, 0xB);
                    gidSurface.ReleaseDC();
                }
                Devices.device.ImmediateContext.CopyResource(cursorIconTarget, target);
                return this;
            }

            internal void CopyScreen(Texture2D screen, Texture2D destination)
            {
                if (Overlay.isDisplay)
                {
                    Devices.device.ImmediateContext.CopyResource(screen, destination);
                }
                else
                {
                    var region = new ResourceRegion(Overlay.rectAbs.X, Overlay.rectAbs.Y, 0, Overlay.rectAbs.Right, Overlay.rectAbs.Bottom, 1);
                    Devices.device.ImmediateContext.CopySubresourceRegion(screen, 0, region, destination, 0);
                }
            }

            public void Dispose()
            {
                foreach (var d in _disposables)
                {
                    d.Dispose();
                }
                _disposables.Clear();
            }
        }
    }
}
