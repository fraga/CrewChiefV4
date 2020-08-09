
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Graphics = System.Drawing.Graphics;
using Device = SharpDX.Direct3D11.Device;
using SharpDX;
using Valve.VR;
using SharpDX.Mathematics.Interop;
using CrewChiefV4;
using System.Collections.Generic;
using CrewChiefV4.Events;
using CrewChiefV4.VirtualReality;
namespace CrewChiefV4.ScreenCapture
{

    public class Direct3D11CaptureSource : IDisposable
    {
        private OutputDuplicationSource[] outputDuplicationSource;
        private Device device;
        private DeviceManager deviceManager;
        IntPtr cursor = Win32Stuff.LoadCursor(IntPtr.Zero, (int)Win32Stuff.IDC_STANDARD_CURSORS.IDC_ARROW);

        public Direct3D11CaptureSource(DeviceManager deviceManager)
        {
            device = deviceManager.device;
            this.deviceManager = deviceManager;
            Initialize();
        }
        private void Initialize()
        {
            int adapterIndex = 0;
            OpenVR.System.GetDXGIOutputInfo(ref adapterIndex);

            using (var factory = new Factory4())
            using (var adapter = factory.GetAdapter(adapterIndex))
            {
                int DisplaysCount = adapter.GetOutputCount();
                outputDuplicationSource = new OutputDuplicationSource[DisplaysCount];
                for (int i = 0; i < DisplaysCount; i++)
                {
                    using (var output = new Output1(adapter.Outputs[i].NativePointer))
                    {
                        var outputDuplication = output.DuplicateOutput(device);
                        outputDuplicationSource[i] = new OutputDuplicationSource(outputDuplication, output.Description.DesktopBounds, output.Description.DeviceName);
                    }
                }
            }
        }
        private void OnDeviceAccessLost()
        {
            Dispose();
            Initialize();
        }
        public void Capture(ref List<VROverlayWindow> windows)
        {
            if (device == null)
            {
                return;
            }

            foreach (var dub in outputDuplicationSource)
            {
                bool captureDone = false;
                List<VROverlayWindow> currentBatch = new List<VROverlayWindow>();
                for (int i = 0; i < windows.Count; i++ /* var w in windows*/)
                {
                    var w = windows[i];
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
                            Win32Stuff.SetWindowPos(w.hWnd, (IntPtr)Win32Stuff.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0, Win32Stuff.SetWindowPosFlags.SWP_NOMOVE | Win32Stuff.SetWindowPosFlags.SWP_NOSIZE);
                    }
                    else if (w.Name != dub.deviceName)
                        continue;



                    SharpDX.Rectangle rect = w.isDisplay ? new SharpDX.Rectangle(0, 0, dub.width, dub.height) : dub.convertToAbsScreenRect(info);
                    w.rectScreen = w.isDisplay ? new SharpDX.Rectangle(dub.rectangle.Left, dub.rectangle.Top, dub.rectangle.Width, dub.rectangle.Height) : dub.convertToScreenRect(info); ;
                    if ((rect.Width != w.rectAbs.Width || rect.Height != w.rectAbs.Height) && (rect.Width > 0 && rect.Height > 0))
                    {
                        w.rectAbs = rect;
                        try
                        {
                            w.copiedScreenTexture?.Dispose();
                            w.copiedScreenTexture = null;
                            w.copiedScreenTexture = new Texture2D(device, new Texture2DDescription
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
                            });
                        }
                        catch (SharpDXException e)
                        {
                            Console.WriteLine("CaptureScreen.Capture: Screen capturing failed = " + e.Message);
                        }
                    }

                    w.rectAbs = rect;
                    w.aspect = Math.Abs(((float)w.rectAbs.Height / (float)w.rectAbs.Width));
                    currentBatch.Add(w);
                }

                if (currentBatch.Count < 1)
                    continue;

                const int MAX_CAPTURE_RETRY_COUNT = 5;
                int captureRetry = 0;
                for (; !captureDone && captureRetry < MAX_CAPTURE_RETRY_COUNT; captureRetry++)
                {
                    if (!VROverlayController.vrUpdateThreadRunning
                        || VROverlayController.vrOverlayRenderThreadSuspended)
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
                                foreach (var w1 in currentBatch)
                                {
                                    if (!w1.isDisplay)
                                    {
                                        ResourceRegion region = new ResourceRegion(w1.rectAbs.X, w1.rectAbs.Y, 0, w1.rectAbs.Right, w1.rectAbs.Bottom, 1);
                                        //Console.WriteLine("Capture size, Width " + (region.Right - region.Left) + " Height " + (region.Bottom - region.Top));
                                        var pt = CursorInteraction.GetCursorPosRelativeWindow(w1.rectScreen);
                                        Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
                                        ci.cbSize = Marshal.SizeOf(ci);
                                        var gotCursor = Win32Stuff.GetCursorInfo(out ci);
                                        if ((pt.X >= 0 && pt.X <= w1.rectAbs.Width) && (pt.Y >= 0 && pt.Y <= w1.rectAbs.Height) && gotCursor && ci.flags == Win32Stuff.CURSOR_SHOWING)
                                        {
                                            var gdiTexture = new Texture2D(device, new Texture2DDescription
                                            {
                                                CpuAccessFlags = CpuAccessFlags.None,
                                                BindFlags = BindFlags.RenderTarget,
                                                Format = dub.outputDuplication.Description.ModeDescription.Format,
                                                Width = w1.rectAbs.Width,
                                                Height = w1.rectAbs.Height,
                                                OptionFlags = ResourceOptionFlags.GdiCompatible,
                                                MipLevels = 1,
                                                ArraySize = 1,
                                                SampleDescription = { Count = 1, Quality = 0 },
                                                Usage = ResourceUsage.Default
                                            });
                                            device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, region, gdiTexture, 0);
                                            using (var gidSurface = gdiTexture.QueryInterface<Surface1>())
                                            {
                                                var HDC = gidSurface.GetDC(false);
                                                Win32Stuff.DrawIconEx(HDC, pt.X, pt.Y, cursor, 0, 0, 0, IntPtr.Zero, 0xB);
                                                gidSurface.ReleaseDC();
                                            }
                                            device.ImmediateContext.CopyResource(gdiTexture, w1.copiedScreenTexture);
                                            gdiTexture.Dispose();
                                            gdiTexture = null;
                                        }
                                        else
                                        {
                                            device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, region, w1.copiedScreenTexture, 0);
                                        }
                                    }
                                    else
                                    {
                                        Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
                                        ci.cbSize = Marshal.SizeOf(ci);
                                        if (Win32Stuff.GetCursorInfo(out ci) && ci.flags == Win32Stuff.CURSOR_SHOWING)
                                        {
                                            var gdiTexture = new Texture2D(device, new Texture2DDescription
                                            {
                                                CpuAccessFlags = CpuAccessFlags.None,
                                                BindFlags = BindFlags.RenderTarget,
                                                Format = dub.outputDuplication.Description.ModeDescription.Format,
                                                Width = dub.width,
                                                Height = dub.height,
                                                OptionFlags = ResourceOptionFlags.GdiCompatible,
                                                MipLevels = 1,
                                                ArraySize = 1,
                                                SampleDescription = { Count = 1, Quality = 0 },
                                                Usage = ResourceUsage.Default
                                            });
                                            device.ImmediateContext.CopyResource(screenTexture, gdiTexture);
                                            using (var gidSurface = gdiTexture.QueryInterface<Surface1>())
                                            {
                                                var HDC = gidSurface.GetDC(false);
                                                Win32Stuff.DrawIconEx(HDC, ci.ptScreenPos.x, ci.ptScreenPos.y, cursor, 0, 0, 0, IntPtr.Zero, 0xB);
                                                gidSurface.ReleaseDC();

                                            }
                                            device.ImmediateContext.CopyResource(gdiTexture, w1.copiedScreenTexture);
                                            gdiTexture.Dispose();
                                            gdiTexture = null;
                                        }
                                        else
                                        {
                                            device.ImmediateContext.CopyResource(screenTexture, w1.copiedScreenTexture);
                                        }
                                    }
                                    w1.shouldDraw = true;
                                }
                            }
                            captureDone = true;
                            screenResource?.Dispose();
                            screenResource = null;
                            dub.outputDuplication.ReleaseFrame();
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
            try
            {
                if (outputDuplicationSource != null)
                {
                    foreach (var dub in outputDuplicationSource)
                    {
                        if (dub.outputDuplication != null
                            && dub.outputDuplication.NativePointer != IntPtr.Zero
                            && !dub.outputDuplication.IsDisposed)
                        {
                            dub.outputDuplication.Dispose();
                            dub.outputDuplication = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // This still crashes, wtf?
                Utilities.ReportException(ex, "Direct3D11CaptureSource.Dispose crashed.", needReport: false);
            }
            //copiedScreenTexture.Dispose();
            //copiedScreenTexture = null;
        }
        internal class OutputDuplicationSource
        {
            public OutputDuplication outputDuplication { get; set; }
            public System.Drawing.Rectangle rectangle;
            public string deviceName;
            public int width;
            public int height;
            public OutputDuplicationSource(OutputDuplication outputDuplication, RawRectangle rectangle, string deviceName)
            {
                this.outputDuplication = outputDuplication;
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
            public SharpDX.Rectangle convertToAbsScreenRect(Win32Stuff.WINDOWINFO info)
            {
                return new SharpDX.Rectangle(Math.Abs(rectangle.Left - info.rcWindow.Left) + (int)info.cxWindowBorders, Math.Abs(rectangle.Top - info.rcWindow.Top), info.rcWindow.Width - (int)info.cxWindowBorders * 2, info.rcWindow.Height - (int)info.cyWindowBorders);
            }
            public SharpDX.Rectangle convertToScreenRect(Win32Stuff.WINDOWINFO info)
            {
                return new SharpDX.Rectangle(info.rcWindow.Left + (int)info.cxWindowBorders, info.rcWindow.Top , info.rcWindow.Width - (int)info.cxWindowBorders * 2, info.rcWindow.Height - (int)info.cyWindowBorders);
            }
        }

    }
}
