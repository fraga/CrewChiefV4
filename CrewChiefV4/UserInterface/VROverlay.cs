using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using Valve.VR;
using CrewChiefV4.VirtualReality;
using System.Threading;
using System.Diagnostics;
using static CrewChiefV4.commands.KeyPresser;

namespace CrewChiefV4
{
    public partial class VROverlaySettings : Form
    {

        

        private class VirtualKeyMap
        {
            public Keys keyCode;
            public VirtualKeyMap(Keys keyCode)
            {
                this.keyCode = keyCode;
            }

            public override string ToString()
            {
                return keyCode.ToString();
            }
        }

        List<VROverlayWindow> settings = VROverlayWindow.loadOverlaySetttings<List<VROverlayWindow>>("vr_overlay_windows.json");

        public static object instanceLock = new object();

        private bool loadingSettings = true;
        public VROverlaySettings()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.SuspendLayout();
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();

            labelAvailableWindows.Text = Configuration.getUIString("available_windows_label");
            buttonSaveSettings.Text = Configuration.getUIString("save_changes");
            buttonSaveSettings.Enabled = false;
            checkBoxEnabled.Text = Configuration.getUIString("enable_in_vr");
            groupBoxPosition.Text = Configuration.getUIString("vr_overlay_position");
            labelPositionX.Text = Configuration.getUIString("vr_position_x");
            labelPositionY.Text = Configuration.getUIString("vr_position_y");
            labelPositionZ.Text = Configuration.getUIString("vr_position_z");

            groupBoxRotation.Text = Configuration.getUIString("vr_overlay_rotation");
            labelRotationX.Text = Configuration.getUIString("vr_rotation_x");
            labelRotationY.Text = Configuration.getUIString("vr_rotation_y");
            labelRotationZ.Text = Configuration.getUIString("vr_rotation_z");

            groupBoxScaleTransCurve.Text = Configuration.getUIString("vr_overlay_scale_trans_curve");
            labelSale.Text = Configuration.getUIString("vr_scale");
            labelTransparency.Text = Configuration.getUIString("vr_transparency");
            labelCurvature.Text = Configuration.getUIString("vr_curvature");

            checkBoxEnableGazeing.Text = Configuration.getUIString("vr_enable_gazing");
            labelGazeScale.Text = Configuration.getUIString("vr_gaze_scale");
            labelGazeTransparency.Text = Configuration.getUIString("vr_gaze_transparency");

            checkBoxForceTopMostWindow.Text = Configuration.getUIString("vr_force_topmost_window");

            labelTrackingSpace.Text = Configuration.getUIString("vr_tracking_space");
            comboBoxTrackingSpace.Items.Add(Configuration.getUIString("vr_tracking_space_seated"));
            comboBoxTrackingSpace.Items.Add(Configuration.getUIString("vr_tracking_space_standing"));
            comboBoxTrackingSpace.Items.Add(Configuration.getUIString("vr_tracking_space_followhead"));

            labelToggleKey.Text = Configuration.getUIString("vr_toggle_key");

            var keys = Enum.GetValues(typeof(Keys));
            foreach(var key in keys)
            {
                comboBoxToggleVirtualKeys.Items.Add(new VirtualKeyMap((Keys)key));
            }
            foreach (var key in keys)
            {
                comboBoxModifierKeys.Items.Add(new VirtualKeyMap((Keys)key));
            }
            updateWindowList();

            this.KeyPreview = true;
            this.KeyDown += VROverlaySettings_KeyDown;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void VROverlaySettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        void updateWindowList()
        {
            List<IntPtr> windows = new List<IntPtr>();
            // hack! screens dont contain a hwnd so make one up and hope it dont collide with an existing hwnd
            IntPtr scrVirtWnd = IntPtr.Zero;            
            foreach (var screen in Screen.AllScreens)
            {                
                windows.Add(scrVirtWnd);
                scrVirtWnd = IntPtr.Subtract(scrVirtWnd, 1);
            }
            windows.AddRange(Win32Stuff.FindWindows());
            VROverlayWindow []currentItems = new VROverlayWindow[listBoxWindows.Items.Count];
            listBoxWindows.Items.CopyTo(currentItems, 0);
            var newWindows = windows.Where(wnd => !currentItems.Any(cu => cu.hWnd == wnd));
            var removedWindows = currentItems.Where(ws => !windows.Any(wnd => wnd == ws.hWnd));
            var showSettingsAsOverlay = UserSettings.GetUserSettings().getBoolean("show_vr_settings_as_overlay");
            foreach (var wnd in newWindows)
            {
                bool added = false;
                bool isDisplay = false;
                string windowName = Win32Stuff.GetWindowText(wnd);

                if (wnd == IntPtr.Subtract(IntPtr.Zero, 1) || wnd == IntPtr.Zero)
                {
                    windowName = Screen.AllScreens[Math.Abs((int)wnd)].DeviceName;
                    isDisplay = true;
                }
                foreach (var s in settings)
                {
                    if(s.Name == windowName)
                    {
                        this.loadingSettings = true;
                        // always enable this window in VR, unless opted out.
                        VROverlayWindow savedWindow = null;
                        if (s.Text == this.Text)
                        {
                            s.enabled = showSettingsAsOverlay;
                        }

                        savedWindow = new VROverlayWindow(wnd, s);
                        savedWindow.isDisplay = isDisplay;
                        listBoxWindows.Items.Add(savedWindow);
                        
                        //if (savedWindow.enabled)
                        //    savedWindow.SetOverlayCursors(cursorOverlay.vrOverlayHandle);
                        added = true;
                        this.loadingSettings = false;
                        if(s.Text == this.Text)
                        {
                            // move the cursor over this form so its easy to find in VR
                            MoveCursor();
                        }
                        break;
                    }
                }
                if (!added)
                {
                    if (!string.IsNullOrWhiteSpace(windowName))
                    {
                        var scr = new VROverlayWindow(windowName, wnd, isDisplay: isDisplay, enabled: windowName == this.Text && showSettingsAsOverlay, wasEnabled: windowName == this.Text && showSettingsAsOverlay);
                        listBoxWindows.Items.Add(scr);
                        if (windowName == this.Text)
                        {
                            // move the cursor over this form so its easy to find in VR
                            MoveCursor();
                        }
                    }
                }                
            }
            foreach(var rm in removedWindows)
            {
                if(rm == listBoxWindows.SelectedItem)
                {
                    if(listBoxWindows.Items.Count == 1)
                    {
                        listBoxWindows.SelectedIndex = -1;
                    }
                    else
                    {
                        listBoxWindows.SelectedIndex = 0;
                    }
                }
                rm.Dispose();
                listBoxWindows.Items.Remove(rm);
            }
        }

        private void VROverlay_Load(object sender, EventArgs e)
        {
            this.loadingSettings = true;
            // updated a bit faster if form is showing
            timer5sec.Interval = 1000;
            if (listBoxWindows.Items.Count > 0)
                listBoxWindows.SelectedIndex = 0;

            this.loadingSettings = false;
        }
        // call this in a seperate thread to ensure this window gets removed from the list, this is needed as listBoxWindows hold a handle to this window for 5 sec after we close it.
        // resulting in overlay creation error if user opens this window again within the 5 sec of closing it.
        private void PostCloseUpdateWindows()
        {
            Thread.Sleep(200);
            lock (instanceLock)
            {
                updateWindowList();
            }
        }

        private void VROverlayController_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            //Cursor.Clip = default(System.Drawing.Rectangle);
           
            new Thread(PostCloseUpdateWindows).Start();
            // limit update rate to 5 sec interval
            timer5sec.Interval = 5000;
        }

        private void timer5sec_Tick(object sender, EventArgs e)
        {
            lock(instanceLock)
            {
                updateWindowList();
            }
        }

        private void listBoxWindows_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    // Let child controls know that this is not value set via UI interaction.
                    this.loadingSettings = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);

                    checkBoxEnabled.Checked = window.enabled;
                    checkBoxEnableGazeing.Checked = window.gazeEnabled;

                    checkBoxForceTopMostWindow.Checked = window.forceTopMost;

                    trackBarPositionX.Value = (int)(window.positionX * 100);
                    trackBarPositionY.Value = (int)(window.positionY * 100);
                    trackBarPositionZ.Value = (int)(window.positionZ * 100);

                    textBoxLeftRight.Text = window.positionX.ToString("0.000");
                    textBoxUpDown.Text = window.positionY.ToString("0.000");
                    textBoxDistance.Text = window.positionZ.ToString("0.000");

                    trackBarRotationX.Value = (int)(window.rotationX * MathUtil.Rad2Deg);
                    trackBarRotationY.Value = (int)(window.rotationY * MathUtil.Rad2Deg);
                    trackBarRotationZ.Value = (int)(window.rotationZ * MathUtil.Rad2Deg);

                    textBoxRotationX.Text = trackBarRotationX.Value.ToString("0.0");
                    textBoxRotationY.Text = trackBarRotationY.Value.ToString("0.0");
                    textBoxRotationZ.Text = trackBarRotationZ.Value.ToString("0.0");

                    trackBarScale.Value = (int)(window.scale * 100);
                    trackBarTransparency.Value = (int)(window.transparency * 100);
                    trackBarCurvature.Value = (int)(window.curvature * 100);

                    textBoxScale.Text = window.scale.ToString("0.00");
                    textBoxTransparency.Text = window.transparency.ToString("0.00");
                    textBoxCurvature.Text = window.curvature.ToString("0.00");

                    trackBarGazeScale.Value = (int)(window.gazeScale * 10);
                    trackBarGazeTransparency.Value = (int)(window.gazeTransparency * 100);

                    textBoxGazeScale.Text = window.gazeScale.ToString("0.0");
                    textBoxGazeTransparency.Text = window.gazeTransparency.ToString("0.00");

                    comboBoxTrackingSpace.SelectedIndex = (int)window.trackingSpace;

                    if(window.toggleVKeyCode == -1)
                    {
                        comboBoxToggleVirtualKeys.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBoxToggleVirtualKeys.SelectedIndex = comboBoxToggleVirtualKeys.FindString(((Keys)window.toggleVKeyCode).ToString());
                    }
                    if (window.modifierVKeyCode == -1)
                    {
                        comboBoxModifierKeys.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBoxModifierKeys.SelectedIndex = comboBoxModifierKeys.FindString(((Keys)window.modifierVKeyCode).ToString());
                    }
                    this.loadingSettings = false;
                }
            }
        }

        private void trackBarPositionX_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.positionX = (trackBarPositionX.Value * 0.01f);
                    textBoxLeftRight.Text = window.positionX.ToString("0.000");
                }
            }
        }

        private void trackBarPositionY_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;
                    
                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.positionY = (trackBarPositionY.Value * 0.01f);
                    textBoxUpDown.Text = window.positionY.ToString("0.000");
                }
            }
        }

        private void trackBarPositionZ_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.positionZ = (trackBarPositionZ.Value * 0.01f);
                    textBoxDistance.Text = window.positionZ.ToString("0.000");
                }
            }
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                this.buttonSaveSettings.Enabled = false;

                VROverlayWindow[] currentItems = new VROverlayWindow[listBoxWindows.Items.Count];
                listBoxWindows.Items.CopyTo(currentItems, 0);

                var currWasEnabled = currentItems.Where(s => s.wasEnabled).ToList();

                // Merge with the existing settings.
                foreach (var currWnd in currWasEnabled)
                {
                    bool newWindow = true;
                    foreach (var s in settings)
                    {
                        if (s.Text == currWnd.Text)
                        {
                            newWindow = false;
                            Debug.Assert(s.Name == currWnd.Name);
                            s.enabled = currWnd.enabled;
                            s.wasEnabled = currWnd.wasEnabled;
                            s.positionX = currWnd.positionX;
                            s.positionY = currWnd.positionY;
                            s.positionZ = currWnd.positionZ;
                            s.rotationX = currWnd.rotationX;
                            s.rotationY = currWnd.rotationY;
                            s.rotationZ = currWnd.rotationZ;
                            s.scale = currWnd.scale;
                            s.gazeScale = currWnd.gazeScale;
                            s.transparency = currWnd.transparency;
                            s.gazeTransparency = currWnd.gazeTransparency;
                            s.curvature = currWnd.curvature;
                            s.gazeEnabled = currWnd.gazeEnabled;
                            s.forceTopMost = currWnd.forceTopMost;
                            s.trackingSpace = currWnd.trackingSpace;
                            s.isDisplay = currWnd.isDisplay;
                            s.toggleVKeyCode = currWnd.toggleVKeyCode;
                            s.modifierVKeyCode = currWnd.modifierVKeyCode;
                        }
                    }

                    // Add new Window into settings.
                    if (newWindow)
                    {
                        settings.Add(currWnd);
                    }
                }

                VROverlayWindow.saveOverlaySetttings("vr_overlay_windows.json", settings);
            }
        }

        private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.enabled = checkBoxEnabled.Checked;
                    if (window.enabled && window.vrOverlayHandle == 0)
                        window.CreateOverlay();
                    if (!window.enabled)
                        window.shouldDraw = false;

                    window.SetOverlayEnabled(window.enabled);
                    window.SetOverlayCurvature();
                    window.SetOverlayTransparency();

                    // If window was ever enabled, keep it in the settings.
                    window.wasEnabled = window.wasEnabled || window.enabled;
                }
            }
        }

        private void trackBarRotationX_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.rotationX = (trackBarRotationX.Value * MathUtil.Deg2Rad);
                    textBoxRotationX.Text = trackBarRotationX.Value.ToString("0.0");
                }
            }
        }

        private void trackBarRotationY_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.rotationY = (trackBarRotationY.Value * MathUtil.Deg2Rad);
                    textBoxRotationY.Text = trackBarRotationY.Value.ToString("0.0");
                }
            }
        }

        private void trackBarRotationZ_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.rotationZ = (trackBarRotationZ.Value * MathUtil.Deg2Rad);
                    textBoxRotationZ.Text = trackBarRotationZ.Value.ToString("0.0");
                }
            }
        }

        private void trackBarScale_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.scale = (trackBarScale.Value * 0.01f);
                    textBoxScale.Text = window.scale.ToString("0.00");
                }
            }
        }

        private void trackBarTransparency_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.transparency = (trackBarTransparency.Value * 0.01f);
                    textBoxTransparency.Text = window.transparency.ToString("0.00");
                    window.SetOverlayTransparency();
                }
            }
        }

        private void trackBarCurvature_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.curvature = (trackBarCurvature.Value * 0.01f);
                    textBoxCurvature.Text = window.curvature.ToString("0.00");
                    window.SetOverlayCurvature();
                }
            }
        }
        private void MoveCursor()
        {
            this.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new System.Drawing.Point(this.Location.X + 50, this.Location.Y + 50);
            //Cursor.Clip = new System.Drawing.Rectangle(this.Location, this.Size);
        }

        private void checkBoxEnableGazeing_CheckedChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.gazeEnabled = checkBoxEnableGazeing.Checked;
                }
            }
        }

        private void trackBarGazeScale_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.gazeScale = (trackBarGazeScale.Value * 0.1f);
                    textBoxGazeScale.Text = window.gazeScale.ToString("0.00");
                }
            }
        }

        private void trackBarGazeTransparency_ValueChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.gazeTransparency = (trackBarGazeTransparency.Value * 0.01f);
                    textBoxGazeTransparency.Text = window.gazeTransparency.ToString("0.00");
                }
            }
        }

        private void checkBoxForceTopMostWindow_CheckedChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.forceTopMost = checkBoxForceTopMostWindow.Checked;
                }
            }
        }

        private void comboBoxTrackingSpace_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.trackingSpace = comboBoxTrackingSpace.SelectedIndex != -1 ? (TrackingSpace)comboBoxTrackingSpace.SelectedIndex : TrackingSpace.Seated;
                    ///window.forceTopMost = checkBoxForceTopMostWindow.Checked;
                }
            }
        }

        private void comboBoxVirtualKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                        window.toggleVKeyCode =  comboBoxToggleVirtualKeys.SelectedIndex != -1 || comboBoxToggleVirtualKeys.SelectedIndex == 0 ? (int)((VirtualKeyMap)comboBoxToggleVirtualKeys.SelectedItem).keyCode : -1;
                    ///window.forceTopMost = checkBoxForceTopMostWindow.Checked;
                }
            }
        }

        private void comboBoxModifierKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (instanceLock)
            {
                if (listBoxWindows.SelectedIndex != -1)
                {
                    if (!this.loadingSettings)
                        this.buttonSaveSettings.Enabled = true;

                    var window = ((VROverlayWindow)listBoxWindows.SelectedItem);
                    window.modifierVKeyCode = comboBoxModifierKeys.SelectedIndex != -1 || comboBoxModifierKeys.SelectedIndex == 0 ? (int)((VirtualKeyMap)comboBoxModifierKeys.SelectedItem).keyCode : -1;
                    ///window.forceTopMost = checkBoxForceTopMostWindow.Checked;
                }
            }
        }
    }
}
