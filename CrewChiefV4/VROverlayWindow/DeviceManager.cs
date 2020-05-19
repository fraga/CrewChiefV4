
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
using System.Windows.Forms;

namespace CrewChiefV4
{
    public enum GraphicsDeviceStatus
    {
        /// <summary>
        /// The device is running fine.
        /// </summary>
        Normal,

        /// <summary>
        /// The video card has been physically removed from the system, or a driver upgrade for the video card has occurred. The application should destroy and recreate the device.
        /// </summary>
        Removed,

        /// <summary>
        /// The application's device failed due to badly formed commands sent by the application. This is an design-time issue that should be investigated and fixed.
        /// </summary>
        Hung,

        /// <summary>
        /// The device failed due to a badly formed command. This is a run-time issue; The application should destroy and recreate the device.
        /// </summary>
        Reset,

        /// <summary>
        /// The driver encountered a problem and was put into the device removed state.
        /// </summary>
        InternalError,

        /// <summary>
        /// The application provided invalid parameter data; this must be debugged and fixed before the application is released.
        /// </summary>
        InvalidCall,
    }
    public class DeviceManager : IDisposable
    {
        protected bool disposedValue = false; // To detect redundant calls
        protected Device _device;
        protected SwapChain _swapChain;

        public Device device { get { return _device; }  }
        public DeviceContext1 context { get; private set; }
        //public Factory4 factory { get; private set; }
        public RenderTargetView backBufferView { get; private set; }
        public SwapChain swapChain { get { return _swapChain; } }

        public DeviceManager(Form form = null)
        {

            try
            {
                int adapterIndex = 0;
                OpenVR.System.GetDXGIOutputInfo(ref adapterIndex);
                using (var factory = new Factory4())
                using (var adapter = factory.GetAdapter(adapterIndex))
                {
                    if (form != null)
                    {
                        var swapChainDescription = new SwapChainDescription
                        {
                            BufferCount = 1,
                            Flags = SwapChainFlags.None,
                            IsWindowed = true,
                            ModeDescription = new ModeDescription
                            {
                                Format = Format.B8G8R8A8_UNorm,
                                Width = form.ClientSize.Width,
                                Height = form.ClientSize.Height,
                                RefreshRate = new Rational(144, 1)
                            },
                            OutputHandle = form.Handle,
                            SampleDescription = new SampleDescription(1, 0),
                            SwapEffect = SwapEffect.Discard,
                            Usage = Usage.RenderTargetOutput
                        };
                        // Retrieve the Direct3D 11.1 device and device context
                        Device.CreateWithSwapChain(adapter, DeviceCreationFlags.None, swapChainDescription, out _device, out _swapChain);

                        using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                            backBufferView = new RenderTargetView(device, backBuffer);


                        factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.None);
                    }
                    else
                    {
                        var creationFlags = DeviceCreationFlags.None;
#if DEBUG
                        // Enable D3D device debug layer
                        creationFlags |= DeviceCreationFlags.Debug;
#endif
                        _device = new Device(adapter, creationFlags);
                    }

                    context = _device.ImmediateContext.QueryInterface<DeviceContext1>();
                }
            }
            catch (SharpDXException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        internal bool testDevice()
        {
            return true;
        }
        internal void refreshDevice()
        {
            device.Dispose();
            //device = new Device(SharpDX.Direct3D.DriverType.Hardware);
        }
        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                var result = device.DeviceRemovedReason;
                if (result == SharpDX.DXGI.ResultCode.DeviceRemoved)
                {
                    return GraphicsDeviceStatus.Removed;
                }

                if (result == SharpDX.DXGI.ResultCode.DeviceReset)
                {
                    return GraphicsDeviceStatus.Reset;
                }

                if (result == SharpDX.DXGI.ResultCode.DeviceHung)
                {
                    return GraphicsDeviceStatus.Hung;
                }

                if (result == SharpDX.DXGI.ResultCode.DriverInternalError)
                {
                    return GraphicsDeviceStatus.InternalError;
                }

                if (result == SharpDX.DXGI.ResultCode.InvalidCall)
                {
                    return GraphicsDeviceStatus.InvalidCall;
                }

                if (result.Code < 0)
                {
                    return GraphicsDeviceStatus.Reset;
                }

                return GraphicsDeviceStatus.Normal;
            }
        }
        protected virtual void Dispose(bool disposing)
        {

        }

        ~DeviceManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {              
                device?.Dispose();
                _swapChain?.Dispose();
                _device = null;
                _swapChain = null;
                disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }

    }
}
