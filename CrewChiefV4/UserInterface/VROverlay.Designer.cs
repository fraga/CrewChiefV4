using CrewChiefV4.VirtualReality;
namespace CrewChiefV4
{
    partial class VROverlaySettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                foreach (var window in listBoxWindows.Items)
                {
                    ((VROverlayWindow)window).Dispose();
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer5sec = new System.Windows.Forms.Timer(this.components);
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.trackBarPositionX = new System.Windows.Forms.TrackBar();
            this.trackBarPositionY = new System.Windows.Forms.TrackBar();
            this.trackBarPositionZ = new System.Windows.Forms.TrackBar();
            this.groupBoxPosition = new System.Windows.Forms.GroupBox();
            this.labelPositionZ = new System.Windows.Forms.Label();
            this.labelPositionY = new System.Windows.Forms.Label();
            this.labelPositionX = new System.Windows.Forms.Label();
            this.textBoxDistance = new System.Windows.Forms.TextBox();
            this.textBoxUpDown = new System.Windows.Forms.TextBox();
            this.textBoxLeftRight = new System.Windows.Forms.TextBox();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.listBoxWindows = new System.Windows.Forms.ListBox();
            this.groupBoxRotation = new System.Windows.Forms.GroupBox();
            this.labelRotationZ = new System.Windows.Forms.Label();
            this.trackBarRotationX = new System.Windows.Forms.TrackBar();
            this.labelRotationY = new System.Windows.Forms.Label();
            this.trackBarRotationZ = new System.Windows.Forms.TrackBar();
            this.labelRotationX = new System.Windows.Forms.Label();
            this.textBoxRotationZ = new System.Windows.Forms.TextBox();
            this.trackBarRotationY = new System.Windows.Forms.TrackBar();
            this.textBoxRotationY = new System.Windows.Forms.TextBox();
            this.textBoxRotationX = new System.Windows.Forms.TextBox();
            this.groupBoxScaleTransCurve = new System.Windows.Forms.GroupBox();
            this.labelGazeScale = new System.Windows.Forms.Label();
            this.trackBarGazeScale = new System.Windows.Forms.TrackBar();
            this.labelGazeTransparency = new System.Windows.Forms.Label();
            this.trackBarGazeTransparency = new System.Windows.Forms.TrackBar();
            this.textBoxGazeTransparency = new System.Windows.Forms.TextBox();
            this.textBoxGazeScale = new System.Windows.Forms.TextBox();
            this.checkBoxEnableGazeing = new System.Windows.Forms.CheckBox();
            this.labelCurvature = new System.Windows.Forms.Label();
            this.trackBarScale = new System.Windows.Forms.TrackBar();
            this.labelTransparency = new System.Windows.Forms.Label();
            this.trackBarCurvature = new System.Windows.Forms.TrackBar();
            this.labelSale = new System.Windows.Forms.Label();
            this.textBoxCurvature = new System.Windows.Forms.TextBox();
            this.trackBarTransparency = new System.Windows.Forms.TrackBar();
            this.textBoxTransparency = new System.Windows.Forms.TextBox();
            this.textBoxScale = new System.Windows.Forms.TextBox();
            this.labelAvailableWindows = new System.Windows.Forms.Label();
            this.checkBoxForceTopMostWindow = new System.Windows.Forms.CheckBox();
            this.comboBoxTrackingSpace = new System.Windows.Forms.ComboBox();
            this.labelTrackingSpace = new System.Windows.Forms.Label();
            this.comboBoxToggleVirtualKeys = new System.Windows.Forms.ComboBox();
            this.labelToggleKey = new System.Windows.Forms.Label();
            this.comboBoxModifierKeys = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxSetTrackingSpace = new System.Windows.Forms.ComboBox();
            this.groupBoxTrackingUniverse = new System.Windows.Forms.GroupBox();
            this.buttonReCenter = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionZ)).BeginInit();
            this.groupBoxPosition.SuspendLayout();
            this.groupBoxRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationY)).BeginInit();
            this.groupBoxScaleTransCurve.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGazeScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGazeTransparency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCurvature)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTransparency)).BeginInit();
            this.groupBoxTrackingUniverse.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer5sec
            // 
            this.timer5sec.Enabled = true;
            this.timer5sec.Interval = 5000;
            this.timer5sec.Tick += new System.EventHandler(this.timer5sec_Tick);
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(16, 315);
            this.checkBoxEnabled.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(112, 21);
            this.checkBoxEnabled.TabIndex = 1;
            this.checkBoxEnabled.Text = "enable_in_vr";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            this.checkBoxEnabled.CheckedChanged += new System.EventHandler(this.checkBoxEnabled_CheckedChanged);
            // 
            // trackBarPositionX
            // 
            this.trackBarPositionX.Location = new System.Drawing.Point(71, 39);
            this.trackBarPositionX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarPositionX.Maximum = 1000;
            this.trackBarPositionX.Minimum = -1000;
            this.trackBarPositionX.Name = "trackBarPositionX";
            this.trackBarPositionX.Size = new System.Drawing.Size(297, 56);
            this.trackBarPositionX.TabIndex = 5;
            this.trackBarPositionX.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarPositionX.ValueChanged += new System.EventHandler(this.trackBarPositionX_ValueChanged);
            // 
            // trackBarPositionY
            // 
            this.trackBarPositionY.Location = new System.Drawing.Point(71, 108);
            this.trackBarPositionY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarPositionY.Maximum = 1000;
            this.trackBarPositionY.Minimum = -1000;
            this.trackBarPositionY.Name = "trackBarPositionY";
            this.trackBarPositionY.RightToLeftLayout = true;
            this.trackBarPositionY.Size = new System.Drawing.Size(297, 56);
            this.trackBarPositionY.TabIndex = 10;
            this.trackBarPositionY.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarPositionY.ValueChanged += new System.EventHandler(this.trackBarPositionY_ValueChanged);
            // 
            // trackBarPositionZ
            // 
            this.trackBarPositionZ.Location = new System.Drawing.Point(71, 187);
            this.trackBarPositionZ.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarPositionZ.Maximum = 1000;
            this.trackBarPositionZ.Minimum = -1000;
            this.trackBarPositionZ.Name = "trackBarPositionZ";
            this.trackBarPositionZ.Size = new System.Drawing.Size(297, 56);
            this.trackBarPositionZ.TabIndex = 15;
            this.trackBarPositionZ.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarPositionZ.ValueChanged += new System.EventHandler(this.trackBarPositionZ_ValueChanged);
            // 
            // groupBoxPosition
            // 
            this.groupBoxPosition.Controls.Add(this.labelPositionZ);
            this.groupBoxPosition.Controls.Add(this.labelPositionY);
            this.groupBoxPosition.Controls.Add(this.labelPositionX);
            this.groupBoxPosition.Controls.Add(this.textBoxDistance);
            this.groupBoxPosition.Controls.Add(this.textBoxUpDown);
            this.groupBoxPosition.Controls.Add(this.textBoxLeftRight);
            this.groupBoxPosition.Controls.Add(this.trackBarPositionX);
            this.groupBoxPosition.Controls.Add(this.trackBarPositionZ);
            this.groupBoxPosition.Controls.Add(this.trackBarPositionY);
            this.groupBoxPosition.Location = new System.Drawing.Point(284, 31);
            this.groupBoxPosition.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxPosition.Name = "groupBoxPosition";
            this.groupBoxPosition.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxPosition.Size = new System.Drawing.Size(393, 250);
            this.groupBoxPosition.TabIndex = 3;
            this.groupBoxPosition.TabStop = false;
            this.groupBoxPosition.Text = "vr_overlay_position";
            // 
            // labelPositionZ
            // 
            this.labelPositionZ.AutoSize = true;
            this.labelPositionZ.Location = new System.Drawing.Point(4, 167);
            this.labelPositionZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPositionZ.Name = "labelPositionZ";
            this.labelPositionZ.Size = new System.Drawing.Size(92, 17);
            this.labelPositionZ.TabIndex = 13;
            this.labelPositionZ.Text = "vr_position_z";
            // 
            // labelPositionY
            // 
            this.labelPositionY.AutoSize = true;
            this.labelPositionY.Location = new System.Drawing.Point(4, 89);
            this.labelPositionY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPositionY.Name = "labelPositionY";
            this.labelPositionY.Size = new System.Drawing.Size(92, 17);
            this.labelPositionY.TabIndex = 12;
            this.labelPositionY.Text = "vr_position_y";
            // 
            // labelPositionX
            // 
            this.labelPositionX.AutoSize = true;
            this.labelPositionX.Location = new System.Drawing.Point(8, 20);
            this.labelPositionX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPositionX.Name = "labelPositionX";
            this.labelPositionX.Size = new System.Drawing.Size(91, 17);
            this.labelPositionX.TabIndex = 11;
            this.labelPositionX.Text = "vr_position_x";
            // 
            // textBoxDistance
            // 
            this.textBoxDistance.Location = new System.Drawing.Point(12, 187);
            this.textBoxDistance.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxDistance.MaxLength = 4;
            this.textBoxDistance.Name = "textBoxDistance";
            this.textBoxDistance.ReadOnly = true;
            this.textBoxDistance.Size = new System.Drawing.Size(52, 22);
            this.textBoxDistance.TabIndex = 10;
            this.textBoxDistance.TabStop = false;
            // 
            // textBoxUpDown
            // 
            this.textBoxUpDown.Location = new System.Drawing.Point(9, 108);
            this.textBoxUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxUpDown.MaxLength = 4;
            this.textBoxUpDown.Name = "textBoxUpDown";
            this.textBoxUpDown.ReadOnly = true;
            this.textBoxUpDown.Size = new System.Drawing.Size(52, 22);
            this.textBoxUpDown.TabIndex = 9;
            this.textBoxUpDown.TabStop = false;
            // 
            // textBoxLeftRight
            // 
            this.textBoxLeftRight.Location = new System.Drawing.Point(8, 39);
            this.textBoxLeftRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxLeftRight.MaxLength = 100;
            this.textBoxLeftRight.Name = "textBoxLeftRight";
            this.textBoxLeftRight.ReadOnly = true;
            this.textBoxLeftRight.Size = new System.Drawing.Size(52, 22);
            this.textBoxLeftRight.TabIndex = 8;
            this.textBoxLeftRight.TabStop = false;
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.Location = new System.Drawing.Point(903, 544);
            this.buttonSaveSettings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(181, 28);
            this.buttonSaveSettings.TabIndex = 9;
            this.buttonSaveSettings.Text = "button_save_settings";
            this.buttonSaveSettings.UseVisualStyleBackColor = true;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // listBoxWindows
            // 
            this.listBoxWindows.FormattingEnabled = true;
            this.listBoxWindows.ItemHeight = 16;
            this.listBoxWindows.Location = new System.Drawing.Point(16, 31);
            this.listBoxWindows.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listBoxWindows.Name = "listBoxWindows";
            this.listBoxWindows.Size = new System.Drawing.Size(259, 276);
            this.listBoxWindows.TabIndex = 1;
            this.listBoxWindows.SelectedIndexChanged += new System.EventHandler(this.listBoxWindows_SelectedIndexChanged);
            // 
            // groupBoxRotation
            // 
            this.groupBoxRotation.Controls.Add(this.labelRotationZ);
            this.groupBoxRotation.Controls.Add(this.trackBarRotationX);
            this.groupBoxRotation.Controls.Add(this.labelRotationY);
            this.groupBoxRotation.Controls.Add(this.trackBarRotationZ);
            this.groupBoxRotation.Controls.Add(this.labelRotationX);
            this.groupBoxRotation.Controls.Add(this.textBoxRotationZ);
            this.groupBoxRotation.Controls.Add(this.trackBarRotationY);
            this.groupBoxRotation.Controls.Add(this.textBoxRotationY);
            this.groupBoxRotation.Controls.Add(this.textBoxRotationX);
            this.groupBoxRotation.Location = new System.Drawing.Point(284, 286);
            this.groupBoxRotation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxRotation.Name = "groupBoxRotation";
            this.groupBoxRotation.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxRotation.Size = new System.Drawing.Size(393, 250);
            this.groupBoxRotation.TabIndex = 4;
            this.groupBoxRotation.TabStop = false;
            this.groupBoxRotation.Text = "vr_overlay_rotation";
            // 
            // labelRotationZ
            // 
            this.labelRotationZ.AutoSize = true;
            this.labelRotationZ.Location = new System.Drawing.Point(5, 167);
            this.labelRotationZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRotationZ.Name = "labelRotationZ";
            this.labelRotationZ.Size = new System.Drawing.Size(91, 17);
            this.labelRotationZ.TabIndex = 19;
            this.labelRotationZ.Text = "vr_rotation_z";
            // 
            // trackBarRotationX
            // 
            this.trackBarRotationX.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarRotationX.Location = new System.Drawing.Point(76, 39);
            this.trackBarRotationX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarRotationX.Maximum = 180;
            this.trackBarRotationX.Minimum = -180;
            this.trackBarRotationX.Name = "trackBarRotationX";
            this.trackBarRotationX.Size = new System.Drawing.Size(297, 56);
            this.trackBarRotationX.TabIndex = 1;
            this.trackBarRotationX.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarRotationX.ValueChanged += new System.EventHandler(this.trackBarRotationX_ValueChanged);
            // 
            // labelRotationY
            // 
            this.labelRotationY.AutoSize = true;
            this.labelRotationY.Location = new System.Drawing.Point(5, 89);
            this.labelRotationY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRotationY.Name = "labelRotationY";
            this.labelRotationY.Size = new System.Drawing.Size(91, 17);
            this.labelRotationY.TabIndex = 18;
            this.labelRotationY.Text = "vr_rotation_y";
            // 
            // trackBarRotationZ
            // 
            this.trackBarRotationZ.Location = new System.Drawing.Point(76, 187);
            this.trackBarRotationZ.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarRotationZ.Maximum = 180;
            this.trackBarRotationZ.Minimum = -180;
            this.trackBarRotationZ.Name = "trackBarRotationZ";
            this.trackBarRotationZ.Size = new System.Drawing.Size(297, 56);
            this.trackBarRotationZ.TabIndex = 3;
            this.trackBarRotationZ.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarRotationZ.ValueChanged += new System.EventHandler(this.trackBarRotationZ_ValueChanged);
            // 
            // labelRotationX
            // 
            this.labelRotationX.AutoSize = true;
            this.labelRotationX.Location = new System.Drawing.Point(8, 20);
            this.labelRotationX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRotationX.Name = "labelRotationX";
            this.labelRotationX.Size = new System.Drawing.Size(90, 17);
            this.labelRotationX.TabIndex = 17;
            this.labelRotationX.Text = "vr_rotation_x";
            // 
            // textBoxRotationZ
            // 
            this.textBoxRotationZ.Location = new System.Drawing.Point(9, 187);
            this.textBoxRotationZ.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxRotationZ.MaxLength = 4;
            this.textBoxRotationZ.Name = "textBoxRotationZ";
            this.textBoxRotationZ.ReadOnly = true;
            this.textBoxRotationZ.Size = new System.Drawing.Size(52, 22);
            this.textBoxRotationZ.TabIndex = 16;
            this.textBoxRotationZ.TabStop = false;
            // 
            // trackBarRotationY
            // 
            this.trackBarRotationY.Location = new System.Drawing.Point(76, 108);
            this.trackBarRotationY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarRotationY.Maximum = 180;
            this.trackBarRotationY.Minimum = -180;
            this.trackBarRotationY.Name = "trackBarRotationY";
            this.trackBarRotationY.RightToLeftLayout = true;
            this.trackBarRotationY.Size = new System.Drawing.Size(297, 56);
            this.trackBarRotationY.TabIndex = 2;
            this.trackBarRotationY.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarRotationY.ValueChanged += new System.EventHandler(this.trackBarRotationY_ValueChanged);
            // 
            // textBoxRotationY
            // 
            this.textBoxRotationY.Location = new System.Drawing.Point(9, 108);
            this.textBoxRotationY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxRotationY.MaxLength = 4;
            this.textBoxRotationY.Name = "textBoxRotationY";
            this.textBoxRotationY.ReadOnly = true;
            this.textBoxRotationY.Size = new System.Drawing.Size(52, 22);
            this.textBoxRotationY.TabIndex = 15;
            this.textBoxRotationY.TabStop = false;
            // 
            // textBoxRotationX
            // 
            this.textBoxRotationX.Location = new System.Drawing.Point(9, 39);
            this.textBoxRotationX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxRotationX.MaxLength = 100;
            this.textBoxRotationX.Name = "textBoxRotationX";
            this.textBoxRotationX.ReadOnly = true;
            this.textBoxRotationX.Size = new System.Drawing.Size(52, 22);
            this.textBoxRotationX.TabIndex = 14;
            this.textBoxRotationX.TabStop = false;
            // 
            // groupBoxScaleTransCurve
            // 
            this.groupBoxScaleTransCurve.Controls.Add(this.labelGazeScale);
            this.groupBoxScaleTransCurve.Controls.Add(this.trackBarGazeScale);
            this.groupBoxScaleTransCurve.Controls.Add(this.labelGazeTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.trackBarGazeTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.textBoxGazeTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.textBoxGazeScale);
            this.groupBoxScaleTransCurve.Controls.Add(this.checkBoxEnableGazeing);
            this.groupBoxScaleTransCurve.Controls.Add(this.labelCurvature);
            this.groupBoxScaleTransCurve.Controls.Add(this.trackBarScale);
            this.groupBoxScaleTransCurve.Controls.Add(this.labelTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.trackBarCurvature);
            this.groupBoxScaleTransCurve.Controls.Add(this.labelSale);
            this.groupBoxScaleTransCurve.Controls.Add(this.textBoxCurvature);
            this.groupBoxScaleTransCurve.Controls.Add(this.trackBarTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.textBoxTransparency);
            this.groupBoxScaleTransCurve.Controls.Add(this.textBoxScale);
            this.groupBoxScaleTransCurve.Location = new System.Drawing.Point(685, 31);
            this.groupBoxScaleTransCurve.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxScaleTransCurve.Name = "groupBoxScaleTransCurve";
            this.groupBoxScaleTransCurve.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxScaleTransCurve.Size = new System.Drawing.Size(393, 438);
            this.groupBoxScaleTransCurve.TabIndex = 5;
            this.groupBoxScaleTransCurve.TabStop = false;
            this.groupBoxScaleTransCurve.Text = "vr_overlay_scale_trans_curve";
            // 
            // labelGazeScale
            // 
            this.labelGazeScale.AutoSize = true;
            this.labelGazeScale.Location = new System.Drawing.Point(8, 274);
            this.labelGazeScale.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGazeScale.Name = "labelGazeScale";
            this.labelGazeScale.Size = new System.Drawing.Size(100, 17);
            this.labelGazeScale.TabIndex = 26;
            this.labelGazeScale.Text = "vr_gaze_scale";
            // 
            // trackBarGazeScale
            // 
            this.trackBarGazeScale.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarGazeScale.Location = new System.Drawing.Point(76, 294);
            this.trackBarGazeScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarGazeScale.Maximum = 100;
            this.trackBarGazeScale.Name = "trackBarGazeScale";
            this.trackBarGazeScale.Size = new System.Drawing.Size(297, 56);
            this.trackBarGazeScale.TabIndex = 21;
            this.trackBarGazeScale.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarGazeScale.Value = 10;
            this.trackBarGazeScale.ValueChanged += new System.EventHandler(this.trackBarGazeScale_ValueChanged);
            // 
            // labelGazeTransparency
            // 
            this.labelGazeTransparency.AutoSize = true;
            this.labelGazeTransparency.Location = new System.Drawing.Point(5, 353);
            this.labelGazeTransparency.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGazeTransparency.Name = "labelGazeTransparency";
            this.labelGazeTransparency.Size = new System.Drawing.Size(150, 17);
            this.labelGazeTransparency.TabIndex = 25;
            this.labelGazeTransparency.Text = "vr_gaze_transparency";
            // 
            // trackBarGazeTransparency
            // 
            this.trackBarGazeTransparency.Location = new System.Drawing.Point(76, 373);
            this.trackBarGazeTransparency.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarGazeTransparency.Maximum = 100;
            this.trackBarGazeTransparency.Name = "trackBarGazeTransparency";
            this.trackBarGazeTransparency.RightToLeftLayout = true;
            this.trackBarGazeTransparency.Size = new System.Drawing.Size(297, 56);
            this.trackBarGazeTransparency.TabIndex = 22;
            this.trackBarGazeTransparency.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarGazeTransparency.Value = 100;
            this.trackBarGazeTransparency.ValueChanged += new System.EventHandler(this.trackBarGazeTransparency_ValueChanged);
            // 
            // textBoxGazeTransparency
            // 
            this.textBoxGazeTransparency.Location = new System.Drawing.Point(9, 373);
            this.textBoxGazeTransparency.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxGazeTransparency.MaxLength = 4;
            this.textBoxGazeTransparency.Name = "textBoxGazeTransparency";
            this.textBoxGazeTransparency.ReadOnly = true;
            this.textBoxGazeTransparency.Size = new System.Drawing.Size(52, 22);
            this.textBoxGazeTransparency.TabIndex = 24;
            this.textBoxGazeTransparency.TabStop = false;
            // 
            // textBoxGazeScale
            // 
            this.textBoxGazeScale.Location = new System.Drawing.Point(9, 304);
            this.textBoxGazeScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxGazeScale.MaxLength = 100;
            this.textBoxGazeScale.Name = "textBoxGazeScale";
            this.textBoxGazeScale.ReadOnly = true;
            this.textBoxGazeScale.Size = new System.Drawing.Size(52, 22);
            this.textBoxGazeScale.TabIndex = 23;
            this.textBoxGazeScale.TabStop = false;
            // 
            // checkBoxEnableGazeing
            // 
            this.checkBoxEnableGazeing.AutoSize = true;
            this.checkBoxEnableGazeing.Location = new System.Drawing.Point(8, 255);
            this.checkBoxEnableGazeing.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxEnableGazeing.Name = "checkBoxEnableGazeing";
            this.checkBoxEnableGazeing.Size = new System.Drawing.Size(143, 21);
            this.checkBoxEnableGazeing.TabIndex = 20;
            this.checkBoxEnableGazeing.Text = "vr_enable_gazing";
            this.checkBoxEnableGazeing.UseVisualStyleBackColor = true;
            this.checkBoxEnableGazeing.CheckedChanged += new System.EventHandler(this.checkBoxEnableGazeing_CheckedChanged);
            // 
            // labelCurvature
            // 
            this.labelCurvature.AutoSize = true;
            this.labelCurvature.Location = new System.Drawing.Point(5, 167);
            this.labelCurvature.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCurvature.Name = "labelCurvature";
            this.labelCurvature.Size = new System.Drawing.Size(88, 17);
            this.labelCurvature.TabIndex = 19;
            this.labelCurvature.Text = "vr_curvature";
            // 
            // trackBarScale
            // 
            this.trackBarScale.BackColor = System.Drawing.SystemColors.Control;
            this.trackBarScale.Location = new System.Drawing.Point(76, 39);
            this.trackBarScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarScale.Maximum = 1000;
            this.trackBarScale.Name = "trackBarScale";
            this.trackBarScale.Size = new System.Drawing.Size(297, 56);
            this.trackBarScale.TabIndex = 1;
            this.trackBarScale.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarScale.UseWaitCursor = true;
            this.trackBarScale.Value = 10;
            this.trackBarScale.ValueChanged += new System.EventHandler(this.trackBarScale_ValueChanged);
            // 
            // labelTransparency
            // 
            this.labelTransparency.AutoSize = true;
            this.labelTransparency.Location = new System.Drawing.Point(5, 98);
            this.labelTransparency.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTransparency.Name = "labelTransparency";
            this.labelTransparency.Size = new System.Drawing.Size(111, 17);
            this.labelTransparency.TabIndex = 18;
            this.labelTransparency.Text = "vr_transparency";
            // 
            // trackBarCurvature
            // 
            this.trackBarCurvature.Location = new System.Drawing.Point(76, 187);
            this.trackBarCurvature.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarCurvature.Maximum = 100;
            this.trackBarCurvature.Name = "trackBarCurvature";
            this.trackBarCurvature.Size = new System.Drawing.Size(297, 56);
            this.trackBarCurvature.TabIndex = 3;
            this.trackBarCurvature.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarCurvature.ValueChanged += new System.EventHandler(this.trackBarCurvature_ValueChanged);
            // 
            // labelSale
            // 
            this.labelSale.AutoSize = true;
            this.labelSale.Location = new System.Drawing.Point(8, 20);
            this.labelSale.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSale.Name = "labelSale";
            this.labelSale.Size = new System.Drawing.Size(61, 17);
            this.labelSale.TabIndex = 17;
            this.labelSale.Text = "vr_scale";
            // 
            // textBoxCurvature
            // 
            this.textBoxCurvature.Location = new System.Drawing.Point(9, 187);
            this.textBoxCurvature.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxCurvature.MaxLength = 4;
            this.textBoxCurvature.Name = "textBoxCurvature";
            this.textBoxCurvature.ReadOnly = true;
            this.textBoxCurvature.Size = new System.Drawing.Size(52, 22);
            this.textBoxCurvature.TabIndex = 16;
            this.textBoxCurvature.TabStop = false;
            // 
            // trackBarTransparency
            // 
            this.trackBarTransparency.Location = new System.Drawing.Point(76, 118);
            this.trackBarTransparency.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBarTransparency.Maximum = 100;
            this.trackBarTransparency.Name = "trackBarTransparency";
            this.trackBarTransparency.RightToLeftLayout = true;
            this.trackBarTransparency.Size = new System.Drawing.Size(297, 56);
            this.trackBarTransparency.TabIndex = 2;
            this.trackBarTransparency.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarTransparency.Value = 100;
            this.trackBarTransparency.ValueChanged += new System.EventHandler(this.trackBarTransparency_ValueChanged);
            // 
            // textBoxTransparency
            // 
            this.textBoxTransparency.Location = new System.Drawing.Point(9, 118);
            this.textBoxTransparency.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxTransparency.MaxLength = 4;
            this.textBoxTransparency.Name = "textBoxTransparency";
            this.textBoxTransparency.ReadOnly = true;
            this.textBoxTransparency.Size = new System.Drawing.Size(52, 22);
            this.textBoxTransparency.TabIndex = 15;
            this.textBoxTransparency.TabStop = false;
            // 
            // textBoxScale
            // 
            this.textBoxScale.Location = new System.Drawing.Point(9, 49);
            this.textBoxScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxScale.MaxLength = 100;
            this.textBoxScale.Name = "textBoxScale";
            this.textBoxScale.ReadOnly = true;
            this.textBoxScale.Size = new System.Drawing.Size(52, 22);
            this.textBoxScale.TabIndex = 14;
            this.textBoxScale.TabStop = false;
            // 
            // labelAvailableWindows
            // 
            this.labelAvailableWindows.AutoSize = true;
            this.labelAvailableWindows.Location = new System.Drawing.Point(16, 9);
            this.labelAvailableWindows.Name = "labelAvailableWindows";
            this.labelAvailableWindows.Size = new System.Drawing.Size(124, 17);
            this.labelAvailableWindows.TabIndex = 10;
            this.labelAvailableWindows.Text = "available_windows";
            // 
            // checkBoxForceTopMostWindow
            // 
            this.checkBoxForceTopMostWindow.AutoSize = true;
            this.checkBoxForceTopMostWindow.Location = new System.Drawing.Point(16, 343);
            this.checkBoxForceTopMostWindow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBoxForceTopMostWindow.Name = "checkBoxForceTopMostWindow";
            this.checkBoxForceTopMostWindow.Size = new System.Drawing.Size(193, 21);
            this.checkBoxForceTopMostWindow.TabIndex = 11;
            this.checkBoxForceTopMostWindow.Text = "vr_force_topmost_window";
            this.checkBoxForceTopMostWindow.UseVisualStyleBackColor = true;
            this.checkBoxForceTopMostWindow.CheckedChanged += new System.EventHandler(this.checkBoxForceTopMostWindow_CheckedChanged);
            // 
            // comboBoxTrackingSpace
            // 
            this.comboBoxTrackingSpace.FormattingEnabled = true;
            this.comboBoxTrackingSpace.Location = new System.Drawing.Point(16, 391);
            this.comboBoxTrackingSpace.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxTrackingSpace.Name = "comboBoxTrackingSpace";
            this.comboBoxTrackingSpace.Size = new System.Drawing.Size(160, 24);
            this.comboBoxTrackingSpace.TabIndex = 12;
            this.comboBoxTrackingSpace.SelectedIndexChanged += new System.EventHandler(this.comboBoxTrackingSpace_SelectedIndexChanged);
            // 
            // labelTrackingSpace
            // 
            this.labelTrackingSpace.AutoSize = true;
            this.labelTrackingSpace.Location = new System.Drawing.Point(16, 372);
            this.labelTrackingSpace.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTrackingSpace.Name = "labelTrackingSpace";
            this.labelTrackingSpace.Size = new System.Drawing.Size(124, 17);
            this.labelTrackingSpace.TabIndex = 13;
            this.labelTrackingSpace.Text = "vr_tracking_space";
            // 
            // comboBoxToggleVirtualKeys
            // 
            this.comboBoxToggleVirtualKeys.FormattingEnabled = true;
            this.comboBoxToggleVirtualKeys.Location = new System.Drawing.Point(16, 449);
            this.comboBoxToggleVirtualKeys.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxToggleVirtualKeys.Name = "comboBoxToggleVirtualKeys";
            this.comboBoxToggleVirtualKeys.Size = new System.Drawing.Size(160, 24);
            this.comboBoxToggleVirtualKeys.TabIndex = 14;
            this.comboBoxToggleVirtualKeys.SelectedIndexChanged += new System.EventHandler(this.comboBoxVirtualKeys_SelectedIndexChanged);
            // 
            // labelToggleKey
            // 
            this.labelToggleKey.AutoSize = true;
            this.labelToggleKey.Location = new System.Drawing.Point(16, 430);
            this.labelToggleKey.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelToggleKey.Name = "labelToggleKey";
            this.labelToggleKey.Size = new System.Drawing.Size(97, 17);
            this.labelToggleKey.TabIndex = 15;
            this.labelToggleKey.Text = "vr_toggle_key";
            // 
            // comboBoxModifierKeys
            // 
            this.comboBoxModifierKeys.FormattingEnabled = true;
            this.comboBoxModifierKeys.Location = new System.Drawing.Point(16, 502);
            this.comboBoxModifierKeys.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxModifierKeys.Name = "comboBoxModifierKeys";
            this.comboBoxModifierKeys.Size = new System.Drawing.Size(160, 24);
            this.comboBoxModifierKeys.TabIndex = 16;
            this.comboBoxModifierKeys.SelectedIndexChanged += new System.EventHandler(this.comboBoxModifierKeys_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(83, 481);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 17);
            this.label2.TabIndex = 20;
            this.label2.Text = "+";
            // 
            // comboBoxSetTrackingSpace
            // 
            this.comboBoxSetTrackingSpace.FormattingEnabled = true;
            this.comboBoxSetTrackingSpace.Location = new System.Drawing.Point(6, 24);
            this.comboBoxSetTrackingSpace.Name = "comboBoxSetTrackingSpace";
            this.comboBoxSetTrackingSpace.Size = new System.Drawing.Size(164, 24);
            this.comboBoxSetTrackingSpace.TabIndex = 21;
            this.comboBoxSetTrackingSpace.SelectedIndexChanged += new System.EventHandler(this.comboBoxSetTrackingSpace_SelectedIndexChanged);
            // 
            // groupBoxTrackingUniverse
            // 
            this.groupBoxTrackingUniverse.Controls.Add(this.buttonReCenter);
            this.groupBoxTrackingUniverse.Controls.Add(this.comboBoxSetTrackingSpace);
            this.groupBoxTrackingUniverse.Location = new System.Drawing.Point(685, 481);
            this.groupBoxTrackingUniverse.Name = "groupBoxTrackingUniverse";
            this.groupBoxTrackingUniverse.Size = new System.Drawing.Size(393, 55);
            this.groupBoxTrackingUniverse.TabIndex = 24;
            this.groupBoxTrackingUniverse.TabStop = false;
            this.groupBoxTrackingUniverse.Text = "vr_tracking_universe_overwrite";
            // 
            // buttonReCenter
            // 
            this.buttonReCenter.Location = new System.Drawing.Point(202, 21);
            this.buttonReCenter.Name = "buttonReCenter";
            this.buttonReCenter.Size = new System.Drawing.Size(185, 29);
            this.buttonReCenter.TabIndex = 22;
            this.buttonReCenter.Text = "vr_recenter_pose";
            this.buttonReCenter.UseVisualStyleBackColor = true;
            this.buttonReCenter.Click += new System.EventHandler(this.buttonReCenter_Click);
            // 
            // VROverlaySettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 585);
            this.Controls.Add(this.groupBoxTrackingUniverse);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxModifierKeys);
            this.Controls.Add(this.labelToggleKey);
            this.Controls.Add(this.comboBoxToggleVirtualKeys);
            this.Controls.Add(this.labelTrackingSpace);
            this.Controls.Add(this.comboBoxTrackingSpace);
            this.Controls.Add(this.checkBoxForceTopMostWindow);
            this.Controls.Add(this.labelAvailableWindows);
            this.Controls.Add(this.groupBoxScaleTransCurve);
            this.Controls.Add(this.groupBoxRotation);
            this.Controls.Add(this.listBoxWindows);
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.groupBoxPosition);
            this.Controls.Add(this.checkBoxEnabled);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "VROverlaySettings";
            this.Text = "SteamVR Overlay Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VROverlayController_FormClosing);
            this.Load += new System.EventHandler(this.VROverlay_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPositionZ)).EndInit();
            this.groupBoxPosition.ResumeLayout(false);
            this.groupBoxPosition.PerformLayout();
            this.groupBoxRotation.ResumeLayout(false);
            this.groupBoxRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRotationY)).EndInit();
            this.groupBoxScaleTransCurve.ResumeLayout(false);
            this.groupBoxScaleTransCurve.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGazeScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGazeTransparency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCurvature)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTransparency)).EndInit();
            this.groupBoxTrackingUniverse.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer5sec;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.TrackBar trackBarPositionX;
        private System.Windows.Forms.TrackBar trackBarPositionY;
        private System.Windows.Forms.TrackBar trackBarPositionZ;
        private System.Windows.Forms.GroupBox groupBoxPosition;
        private System.Windows.Forms.Button buttonSaveSettings;
        public System.Windows.Forms.ListBox listBoxWindows;
        private System.Windows.Forms.GroupBox groupBoxRotation;
        private System.Windows.Forms.TrackBar trackBarRotationX;
        private System.Windows.Forms.TrackBar trackBarRotationZ;
        private System.Windows.Forms.TrackBar trackBarRotationY;
        private System.Windows.Forms.TextBox textBoxLeftRight;
        private System.Windows.Forms.TextBox textBoxDistance;
        private System.Windows.Forms.TextBox textBoxUpDown;
        private System.Windows.Forms.Label labelPositionZ;
        private System.Windows.Forms.Label labelPositionY;
        private System.Windows.Forms.Label labelPositionX;
        private System.Windows.Forms.Label labelRotationZ;
        private System.Windows.Forms.Label labelRotationY;
        private System.Windows.Forms.Label labelRotationX;
        private System.Windows.Forms.TextBox textBoxRotationZ;
        private System.Windows.Forms.TextBox textBoxRotationY;
        private System.Windows.Forms.TextBox textBoxRotationX;
        private System.Windows.Forms.GroupBox groupBoxScaleTransCurve;
        private System.Windows.Forms.Label labelCurvature;
        private System.Windows.Forms.TrackBar trackBarScale;
        private System.Windows.Forms.Label labelTransparency;
        private System.Windows.Forms.TrackBar trackBarCurvature;
        private System.Windows.Forms.Label labelSale;
        private System.Windows.Forms.TextBox textBoxCurvature;
        private System.Windows.Forms.TrackBar trackBarTransparency;
        private System.Windows.Forms.TextBox textBoxTransparency;
        private System.Windows.Forms.TextBox textBoxScale;
        private System.Windows.Forms.Label labelAvailableWindows;
        private System.Windows.Forms.CheckBox checkBoxEnableGazeing;
        private System.Windows.Forms.Label labelGazeScale;
        private System.Windows.Forms.TrackBar trackBarGazeScale;
        private System.Windows.Forms.Label labelGazeTransparency;
        private System.Windows.Forms.TrackBar trackBarGazeTransparency;
        private System.Windows.Forms.TextBox textBoxGazeTransparency;
        private System.Windows.Forms.TextBox textBoxGazeScale;
        private System.Windows.Forms.CheckBox checkBoxForceTopMostWindow;
        private System.Windows.Forms.ComboBox comboBoxTrackingSpace;
        private System.Windows.Forms.Label labelTrackingSpace;
        private System.Windows.Forms.ComboBox comboBoxToggleVirtualKeys;
        private System.Windows.Forms.Label labelToggleKey;
        private System.Windows.Forms.ComboBox comboBoxModifierKeys;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxSetTrackingSpace;
        private System.Windows.Forms.GroupBox groupBoxTrackingUniverse;
        private System.Windows.Forms.Button buttonReCenter;
    }
}