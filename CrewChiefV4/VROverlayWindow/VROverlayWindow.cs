﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using SharpDX;
using SharpDX.Direct3D11;
using Valve.VR;
/// <summary>
/// This class stores settings for each Target Application
/// </summary>
/// 
namespace CrewChiefV4.VirtualReality
{
    public enum ClickAPI
    {
        None = 0,
        SendInput = 1,
        SendMessage = 2,
        SendNotifyMessage = 3,
    }
    public enum CaptureMode
    {
        GdiDirect = 0,
        GdiIndirect = 1,
        ReplicationApi = 2,
    }

    public enum MouseInteractionMode
    {
        DirectInteraction = 0, // Keep Window on top, Move Cursor
        WindowTop = 1, // Keep Window on top only, Send Mouse Clicks Only (No Move)
        SendClicksOnly = 2, // Only Send Mouse Clicks
        Disabled = 3
    }
    [Serializable]
    public class VROverlayWindow 
    {
        public string Name { get; set; }
        public string Text { get; set; }
        [JsonIgnore]
        public IntPtr hWnd { get; set; }
        public bool enabled { get; set; }
        public bool wasEnabled { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public float positionZ { get; set; }
        public float rotationX { get; set; }
        public float rotationY { get; set; }
        public float rotationZ { get; set; }
        public float scale { get; set; }
        public float gazeScale { get; set; }
        public float transparency { get; set; }
        public float gazeTransparency { get; set; }
        public float curvature { get; set; }
        public bool gazeEnabled { get; set; }
        public bool forceTopMost { get; set; }
        public ETrackingUniverseOrigin trackingUniverse { get; set; }
        public bool isDisplay { get; set; }
        [JsonIgnore]
        public Texture2D copiedScreenTexture { get; set; }       
        [JsonIgnore]
        public SharpDX.Rectangle rectAbs;
        [JsonIgnore]
        public SharpDX.Rectangle rectScreen;
        [JsonIgnore]
        public ulong vrOverlayHandle;
        [JsonIgnore]
        public ulong vrOverlayCursorHandle;
        [JsonIgnore]
        public bool shouldDraw { get; set; }
        [JsonIgnore]
        public float aspect { get; set; }
        [JsonIgnore]
        public Matrix hmdMatrix { get; set; }
        public VROverlayWindow()
        {
            positionZ = -1;
        }
        //[JsonConstructor]
        public VROverlayWindow(string Text, IntPtr hWnd, ulong vrOverlayCursorHandle = 0, string Name = "", bool enabled = false, bool wasEnabled = false, float positionX = 0, float positionY = 0, float positionZ = -1,
            float rotationX = 0, float rotationY = 0, float rotationZ = 0, float scale = 1, float transparency = 1, float curvature = 0, ETrackingUniverseOrigin trackingUniverse = ETrackingUniverseOrigin.TrackingUniverseSeated, bool isDisplay = false, bool gazeEnabled = false, float gazeScale = 1f, float gazeTransparency = 1f, bool forceTopMost = false)
        {
            this.Text = Text;
            if(string.IsNullOrWhiteSpace(Name))
                this.Name = Text;
            else
                this.Name = Name;
            this.enabled = enabled;
            this.hWnd = hWnd;
            this.positionX = positionX;
            this.positionY = positionY;
            this.positionZ = positionZ; // place initial overlay 1 meter infront of the user
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationY = rotationZ;
            this.scale = scale;
            this.gazeScale = gazeScale;
            this.transparency = transparency;
            this.gazeTransparency = gazeTransparency;
            this.trackingUniverse = trackingUniverse;
            this.curvature = curvature;
            this.isDisplay = isDisplay;
            this.vrOverlayCursorHandle = vrOverlayCursorHandle;
            this.gazeEnabled = gazeEnabled;
            this.wasEnabled = wasEnabled;
            this.rectAbs = new Rectangle();
            this.rectScreen = new Rectangle();
            this.forceTopMost = forceTopMost;
            shouldDraw = false;
            if (enabled)
            {
                CreateOverlay();
                SetOverlayCurvature();
                SetOverlayTransparency();
                SetOverlayEnabled(true);
            }
        }
        public VROverlayWindow(IntPtr hWnd, VROverlayWindow other, ulong vrOverlayCursorHandle = 0)
        {
            this.Text = other.Text;
            this.Name = other.Name;
            this.enabled = other.enabled;            
            this.hWnd = hWnd;
            this.positionX = other.positionX;
            this.positionY = other.positionY;
            this.positionZ = other.positionZ; // place initial overlay 1 meter infront of the user
            this.rotationX = other.rotationX;
            this.rotationY = other.rotationY;
            this.rotationZ = other.rotationZ;
            this.scale = other.scale;
            this.gazeScale = other.gazeScale;
            this.transparency = other.transparency;
            this.gazeTransparency = other.gazeTransparency;
            this.trackingUniverse = other.trackingUniverse;
            this.curvature = other.curvature;
            this.isDisplay = other.isDisplay;
            this.gazeEnabled = other.gazeEnabled;
            this.wasEnabled = other.wasEnabled;
            this.vrOverlayCursorHandle = vrOverlayCursorHandle;
            this.rectAbs = new Rectangle();
            this.rectScreen = new Rectangle();
            this.forceTopMost = other.forceTopMost;

            shouldDraw = false;
            if(enabled)
            {
                CreateOverlay();
                SetOverlayCurvature();
                SetOverlayTransparency();
                SetOverlayEnabled(true);
            }
        }
        public void CreateOverlay(bool setFlags = true)
        {
            if (string.IsNullOrWhiteSpace(Name))
                Name = Guid.NewGuid().ToString();
            vrOverlayHandle = 0;
            
            var error = SteamVR.instance.overlay.CreateOverlay(Name, Name, ref vrOverlayHandle);
            if(error == EVROverlayError.None)
            {
                if(setFlags)
                {
                    SteamVR.instance.overlay.SetOverlayFlag(vrOverlayHandle, VROverlayFlags.SortWithNonSceneOverlays, true);
                    // disable mouse input for not as it ned a propper handler
                    //SteamVR.instance.overlay.SetOverlayFlag(vrOverlayHandle, VROverlayFlags.MakeOverlaysInteractiveIfVisible, true);
                    //SteamVR.instance.overlay.SetOverlayFlag(vrOverlayHandle, VROverlayFlags.HideLaserIntersection, true);

                    //SteamVR.instance.overlay.SetOverlayInputMethod(vrOverlayHandle, VROverlayInputMethod.Mouse);

                    
                }
                Console.WriteLine($"Created SVR overlay handle for: {Name} : {vrOverlayHandle}");
            }
            else
            {
                Console.WriteLine($"Failed to create SVR overlay handle for: {Name} error: {error}");
            }

        }

        public bool SubmitOverlay()
        {
            var tex = new Texture_t
            {
                eType = ETextureType.DirectX,
                eColorSpace = EColorSpace.Auto,
                handle = copiedScreenTexture.NativePointer,
            };
            var vecMouseScale = new HmdVector2_t
            {
                v0 = 1f,
                v1 = aspect
            };
            SteamVR.instance.overlay.SetOverlayMouseScale(vrOverlayHandle, ref vecMouseScale);
            return SteamVR.instance.overlay.SetOverlayTexture(vrOverlayHandle, ref tex) == EVROverlayError.None;
        }

        public void SetOverlayCursors(ulong cursorHandle)
        {
            SteamVR.instance.overlay.SetOverlayCursor(vrOverlayHandle, cursorHandle);
        }

        public void SetOverlayEnabled(bool enabled)
        {
            if (enabled)
                SteamVR.instance.overlay.ShowOverlay(vrOverlayHandle);
            else
                SteamVR.instance.overlay.HideOverlay(vrOverlayHandle);
        }

        public void SetOverlayCurvature()
        {
            SteamVR.instance.overlay.SetOverlayCurvature(vrOverlayHandle, curvature);
        }

        public void SetOverlayTransparency()
        {
            SteamVR.instance.overlay.SetOverlayAlpha(vrOverlayHandle, transparency);
        }

        public void SetOverlayParams(bool followsHead, float scale = 1.0f)
        {
            SteamVR.instance.overlay.SetOverlayWidthInMeters(vrOverlayHandle, scale);

            Matrix rotCenter = Matrix.Translation(0, 0, 0);
            rotCenter *= Matrix.RotationX(rotationX);
            rotCenter *= Matrix.RotationY(rotationY);
            rotCenter *= Matrix.RotationZ(rotationZ);
            var transform = Matrix.Scaling(this.scale) * rotCenter *  Matrix.Translation(positionX, positionY, positionZ);
            transform.Transpose();

            if(gazeEnabled)
            {
                if(SetOverlayGazeing(transform))
                {
                    transform = Matrix.Scaling(this.gazeScale) * rotCenter * Matrix.Translation(positionX, positionY, positionZ);
                    transform.Transpose();
                    SteamVR.instance.overlay.SetOverlayAlpha(vrOverlayHandle, gazeTransparency);
                }
                else
                {
                    SteamVR.instance.overlay.SetOverlayAlpha(vrOverlayHandle, transparency);
                }
            }

            if (followsHead)
            {
                HmdMatrix34_t pose = transform.ToHmdMatrix34();
                SteamVR.instance.overlay.SetOverlayTransformTrackedDeviceRelative(vrOverlayHandle, 0, ref pose);
            }
            else
            {
                HmdMatrix34_t pose = transform.ToHmdMatrix34();
                SteamVR.instance.overlay.SetOverlayTransformAbsolute(vrOverlayHandle, trackingUniverse, ref pose);
            }
            shouldDraw = false;
        }

        private bool SetOverlayGazeing(Matrix transform)
        {
            float angle = Matrix.Invert(hmdMatrix).Forward.Angle(transform.Forward);
            if (angle <= 90)
            {
                IntersectionResults results = new IntersectionResults();
                if (ComputeIntersection(hmdMatrix.TranslationVector, hmdMatrix.Forward, ref results))
                {
                    return true;
                }
            }
            return false;
        }

        public void Draw()
        {
            SetOverlayEnabled(shouldDraw && enabled);
            if (shouldDraw)
            {                
                SubmitOverlay();                
                SetOverlayParams(false);
            }
        }

        public struct IntersectionResults
        {
            public Vector3 point;
            public Vector3 normal;
            public Vector2 UVs;
            public float distance;
        }

        public bool ComputeIntersection(Vector3 source, Vector3 direction, ref IntersectionResults results)
        {
            var overlay = SteamVR.instance.overlay;
            if (overlay == null)
                return false;

            var input = new VROverlayIntersectionParams_t();
            input.eOrigin = trackingUniverse;
            input.vSource.v0 = source.X;
            input.vSource.v1 = source.Y;
            input.vSource.v2 = source.Z;
            input.vDirection.v0 = direction.X;
            input.vDirection.v1 = direction.Y;
            input.vDirection.v2 = direction.Z;

            var output = new VROverlayIntersectionResults_t();
            if (!overlay.ComputeOverlayIntersection(vrOverlayHandle, ref input, ref output))
                return false;

            results.point = new Vector3(output.vPoint.v0, output.vPoint.v1, output.vPoint.v2);
            results.normal = new Vector3(output.vNormal.v0, output.vNormal.v1, output.vNormal.v2);
            results.UVs = new Vector2(output.vUVs.v0, output.vUVs.v1);
            results.distance = output.fDistance;
            return true;
        }

        public override string ToString()
        {
            return this.Text;
        }
        // load save settings
        public static T loadOverlaySetttings<T>(string overlayFileName) where T : class
        {
            String path = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "CrewChiefV4", overlayFileName);
            if (!File.Exists(path))
            {
                saveOverlaySetttings(overlayFileName, (T)Activator.CreateInstance(typeof(T)));
            }
            if (path != null)
            {
                try
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        T data = JsonConvert.DeserializeObject<T>(json);
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error pasing " + path + ": " + e.Message);
                }
            }
            return (T)Activator.CreateInstance(typeof(T));
        }
        public static void saveOverlaySetttings<T>(string overlayFileName, T settings)
        {
            String path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating " + path + ": " + e.Message);
                }
            }
            if (overlayFileName != null)
            {
                try
                {
                    using (StreamWriter file = File.CreateText(System.IO.Path.Combine(path, overlayFileName)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(file, settings);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + overlayFileName + ": " + e.Message);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    copiedScreenTexture?.Dispose();
                    if(OpenVR.Overlay != null)
                        OpenVR.Overlay.DestroyOverlay(vrOverlayHandle);
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VROverlayWindow() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
