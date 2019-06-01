﻿using System.Windows.Forms;
namespace CrewChiefV4
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.consoleTextBox = new System.Windows.Forms.RichTextBox();
            this.consoleTextBoxBackgroundPanel = new System.Windows.Forms.Panel();
            this.startApplicationButton = new System.Windows.Forms.Button();
            this.forceVersionCheckButton = new System.Windows.Forms.Button();
            this.buttonActionSelect = new System.Windows.Forms.ListBox();
            this.controllersList = new System.Windows.Forms.ListBox();
            this.assignButtonToAction = new System.Windows.Forms.Button();
            this.deleteAssigmentButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.propertiesButton = new System.Windows.Forms.Button();
            this.aboutButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.voiceDisableButton = new System.Windows.Forms.RadioButton();
            this.holdButton = new System.Windows.Forms.RadioButton();
            this.toggleButton = new System.Windows.Forms.RadioButton();
            this.alwaysOnButton = new System.Windows.Forms.RadioButton();
            this.triggerWordButton = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.messagesVolumeSlider = new System.Windows.Forms.TrackBar();
            this.messagesAudioDeviceBox = new System.Windows.Forms.ComboBox();
            this.speechRecognitionDeviceBox = new System.Windows.Forms.ComboBox();
            this.backgroundAudioDeviceBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.backgroundVolumeSlider = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.gameDefinitionList = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.personalisationLabel = new System.Windows.Forms.Label();
            this.filenameTextbox = new System.Windows.Forms.TextBox();
            this.filenameLabel = new System.Windows.Forms.Label();
            this.recordSession = new System.Windows.Forms.CheckBox();
            this.playbackInterval = new System.Windows.Forms.TextBox();
            this.app_version = new System.Windows.Forms.Label();
            this.soundPackProgressBar = new System.Windows.Forms.ProgressBar();
            this.downloadSoundPackButton = new System.Windows.Forms.Button();
            this.downloadDriverNamesButton = new System.Windows.Forms.Button();
            this.downloadPersonalisationsButton = new System.Windows.Forms.Button();
            this.driverNamesProgressBar = new System.Windows.Forms.ProgressBar();
            this.personalisationsProgressBar = new System.Windows.Forms.ProgressBar();
            this.personalisationBox = new System.Windows.Forms.ComboBox();
            this.spotterNameLabel = new System.Windows.Forms.Label();
            this.messagesAudioDeviceLabel = new System.Windows.Forms.Label();
            this.speechRecognitionDeviceLabel = new System.Windows.Forms.Label();
            this.backgroundAudioDeviceLabel = new System.Windows.Forms.Label();
            this.spotterNameBox = new System.Windows.Forms.ComboBox();
            this.donateLink = new System.Windows.Forms.LinkLabel();
            this.smokeTestTextBox = new System.Windows.Forms.TextBox();
            this.buttonSmokeTest = new System.Windows.Forms.Button();
            this.chiefNameLabel = new System.Windows.Forms.Label();
            this.chiefNameBox = new System.Windows.Forms.ComboBox();
            this.myNameBoxTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.chiefNameBoxTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.spotterNameBoxTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.scanControllers = new System.Windows.Forms.Button();
            this.buttonEditCommandMacros = new System.Windows.Forms.Button();
            this.voiceRecognitionToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.voiceRecognitionAlwaysOnToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.voiceRecognitionToggleButtonToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.voiceRecognitionHoldButtonToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.voiceRecognitionDisabledToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.voiceRecognitionTriggerWordToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.AddRemoveActions = new System.Windows.Forms.Button();
            this.consoleTextBoxBackgroundPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.messagesVolumeSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundVolumeSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.consoleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleTextBox.Location = new System.Drawing.Point(0, 0);
            this.consoleTextBox.MaxLength = 99999999;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.Size = new System.Drawing.Size(1091, 283);
            this.consoleTextBox.TabIndex = 200;
            this.consoleTextBox.Text = "";
            // 
            // consoleTextBoxBackgroundPanel
            // 
            this.consoleTextBoxBackgroundPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.consoleTextBoxBackgroundPanel.Controls.Add(this.consoleTextBox);
            this.consoleTextBoxBackgroundPanel.Location = new System.Drawing.Point(41, 215);
            this.consoleTextBoxBackgroundPanel.Name = "consoleTextBoxBackgroundPanel";
            this.consoleTextBoxBackgroundPanel.Size = new System.Drawing.Size(1093, 285);
            this.consoleTextBoxBackgroundPanel.TabIndex = 291;
            // 
            // startApplicationButton
            // 
            this.startApplicationButton.Location = new System.Drawing.Point(41, 28);
            this.startApplicationButton.Name = "startApplicationButton";
            this.startApplicationButton.Size = new System.Drawing.Size(137, 38);
            this.startApplicationButton.TabIndex = 40;
            this.startApplicationButton.Text = "start_application";
            this.startApplicationButton.UseVisualStyleBackColor = true;
            this.startApplicationButton.Click += new System.EventHandler(this.startApplicationButton_Click);
            // 
            // forceVersionCheckButton
            // 
            this.forceVersionCheckButton.AutoSize = true;
            this.forceVersionCheckButton.Location = new System.Drawing.Point(1030, 665);
            this.forceVersionCheckButton.Name = "forceVersionCheckButton";
            this.forceVersionCheckButton.Size = new System.Drawing.Size(109, 23);
            this.forceVersionCheckButton.TabIndex = 290;
            this.forceVersionCheckButton.Text = "check_for_updates";
            this.forceVersionCheckButton.UseVisualStyleBackColor = true;
            this.forceVersionCheckButton.Click += new System.EventHandler(this.forceVersionCheckButtonClicked);
            // 
            // buttonActionSelect
            // 
            this.buttonActionSelect.FormattingEnabled = true;
            this.buttonActionSelect.Location = new System.Drawing.Point(295, 520);
            this.buttonActionSelect.Name = "buttonActionSelect";
            this.buttonActionSelect.Size = new System.Drawing.Size(528, 108);
            this.buttonActionSelect.TabIndex = 230;
            this.buttonActionSelect.SelectedIndexChanged += new System.EventHandler(this.buttonActionSelect_SelectedIndexChanged);
            // 
            // controllersList
            // 
            this.controllersList.FormattingEnabled = true;
            this.controllersList.Location = new System.Drawing.Point(41, 520);
            this.controllersList.Name = "controllersList";
            this.controllersList.Size = new System.Drawing.Size(248, 108);
            this.controllersList.TabIndex = 210;
            this.controllersList.SelectedIndexChanged += new System.EventHandler(this.controllersList_SelectedIndexChanged);
            // 
            // assignButtonToAction
            // 
            this.assignButtonToAction.Location = new System.Drawing.Point(830, 520);
            this.assignButtonToAction.Name = "assignButtonToAction";
            this.assignButtonToAction.Size = new System.Drawing.Size(130, 39);
            this.assignButtonToAction.TabIndex = 240;
            this.assignButtonToAction.Text = "assign_control";
            this.assignButtonToAction.UseVisualStyleBackColor = true;
            this.assignButtonToAction.Click += new System.EventHandler(this.assignButtonToActionClick);
            // 
            // deleteAssigmentButton
            // 
            this.deleteAssigmentButton.Location = new System.Drawing.Point(830, 564);
            this.deleteAssigmentButton.Name = "deleteAssigmentButton";
            this.deleteAssigmentButton.Size = new System.Drawing.Size(130, 39);
            this.deleteAssigmentButton.TabIndex = 250;
            this.deleteAssigmentButton.Text = "delete_assignment";
            this.deleteAssigmentButton.UseVisualStyleBackColor = true;
            this.deleteAssigmentButton.Click += new System.EventHandler(this.deleteAssignmentButtonClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(38, 500);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 17);
            this.label1.TabIndex = 209;
            this.label1.Text = "available_controllers";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(292, 500);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 17);
            this.label2.TabIndex = 229;
            this.label2.Text = "available_actions";
            // 
            // propertiesButton
            // 
            this.propertiesButton.Location = new System.Drawing.Point(961, 105);
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new System.Drawing.Size(173, 31);
            this.propertiesButton.TabIndex = 110;
            this.propertiesButton.Text = "properties";
            this.propertiesButton.UseVisualStyleBackColor = true;
            this.propertiesButton.Click += new System.EventHandler(this.editPropertiesButtonClicked);
            // 
            // aboutButton
            // 
            this.aboutButton.Location = new System.Drawing.Point(961, 169);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(173, 31);
            this.aboutButton.TabIndex = 130;
            this.aboutButton.Text = "about";
            this.aboutButton.UseVisualStyleBackColor = true;
            this.aboutButton.Click += new System.EventHandler(this.aboutButtonClicked);
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(961, 137);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(173, 31);
            this.helpButton.TabIndex = 120;
            this.helpButton.Text = "help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButtonClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.voiceDisableButton);
            this.groupBox1.Controls.Add(this.holdButton);
            this.groupBox1.Controls.Add(this.toggleButton);
            this.groupBox1.Controls.Add(this.alwaysOnButton);
            this.groupBox1.Controls.Add(this.triggerWordButton);
            this.groupBox1.Location = new System.Drawing.Point(970, 519);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(164, 124);
            this.groupBox1.TabIndex = 260;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "voice_recognition_mode";
            this.voiceRecognitionToolTip.SetToolTip(this.groupBox1, "voice_recognition_mode_help");
            // 
            // voiceDisableButton
            // 
            this.voiceDisableButton.AutoSize = true;
            this.voiceDisableButton.Location = new System.Drawing.Point(7, 16);
            this.voiceDisableButton.Name = "voiceDisableButton";
            this.voiceDisableButton.Size = new System.Drawing.Size(64, 17);
            this.voiceDisableButton.TabIndex = 0;
            this.voiceDisableButton.TabStop = true;
            this.voiceDisableButton.Text = "disabled";
            this.voiceRecognitionDisabledToolTip.SetToolTip(this.voiceDisableButton, "voice_recognition_disabled_help");
            this.voiceDisableButton.UseVisualStyleBackColor = true;
            this.voiceDisableButton.CheckedChanged += new System.EventHandler(this.voiceDisableButton_CheckedChanged);
            // 
            // holdButton
            // 
            this.holdButton.AutoSize = true;
            this.holdButton.Location = new System.Drawing.Point(7, 39);
            this.holdButton.Name = "holdButton";
            this.holdButton.Size = new System.Drawing.Size(81, 17);
            this.holdButton.TabIndex = 1;
            this.holdButton.TabStop = true;
            this.holdButton.Text = "hold_button";
            this.voiceRecognitionHoldButtonToolTip.SetToolTip(this.holdButton, "voice_recognition_hold_button_help");
            this.holdButton.UseVisualStyleBackColor = true;
            this.holdButton.CheckedChanged += new System.EventHandler(this.holdButton_CheckedChanged);
            // 
            // toggleButton
            // 
            this.toggleButton.AutoSize = true;
            this.toggleButton.Location = new System.Drawing.Point(7, 60);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(90, 17);
            this.toggleButton.TabIndex = 2;
            this.toggleButton.TabStop = true;
            this.toggleButton.Text = "toggle_button";
            this.voiceRecognitionToggleButtonToolTip.SetToolTip(this.toggleButton, "voice_recognition_toggle_button_help");
            this.toggleButton.UseVisualStyleBackColor = true;
            this.toggleButton.CheckedChanged += new System.EventHandler(this.toggleButton_CheckedChanged);
            // 
            // alwaysOnButton
            // 
            this.alwaysOnButton.AutoSize = true;
            this.alwaysOnButton.Location = new System.Drawing.Point(7, 82);
            this.alwaysOnButton.Name = "alwaysOnButton";
            this.alwaysOnButton.Size = new System.Drawing.Size(75, 17);
            this.alwaysOnButton.TabIndex = 3;
            this.alwaysOnButton.TabStop = true;
            this.alwaysOnButton.Text = "always_on";
            this.voiceRecognitionAlwaysOnToolTip.SetToolTip(this.alwaysOnButton, "voice_recognition_always_on_help");
            this.alwaysOnButton.UseVisualStyleBackColor = true;
            this.alwaysOnButton.CheckedChanged += new System.EventHandler(this.alwaysOnButton_CheckedChanged);
            // 
            // triggerWordButton
            // 
            this.triggerWordButton.AutoSize = true;
            this.triggerWordButton.Location = new System.Drawing.Point(7, 104);
            this.triggerWordButton.Name = "triggerWordButton";
            this.triggerWordButton.Size = new System.Drawing.Size(254, 17);
            this.triggerWordButton.TabIndex = 4;
            this.triggerWordButton.TabStop = true;
            this.triggerWordButton.Text = "trigger_word (\"trigger_word_for_always_on_sre\")";
            this.voiceRecognitionTriggerWordToolTip.SetToolTip(this.triggerWordButton, "voice_recognition_trigger_word_help");
            this.triggerWordButton.UseVisualStyleBackColor = true;
            this.triggerWordButton.CheckedChanged += new System.EventHandler(this.triggerWordButton_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(184, 28);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 38);
            this.button2.TabIndex = 50;
            this.button2.Text = "clear_console";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.clearConsole);
            // 
            // messagesVolumeSlider
            // 
            this.messagesVolumeSlider.Location = new System.Drawing.Point(327, 28);
            this.messagesVolumeSlider.Maximum = 100;
            this.messagesVolumeSlider.Name = "messagesVolumeSlider";
            this.messagesVolumeSlider.Size = new System.Drawing.Size(176, 45);
            this.messagesVolumeSlider.TabIndex = 60;
            this.messagesVolumeSlider.TickFrequency = 10;
            this.messagesVolumeSlider.Scroll += new System.EventHandler(this.messagesVolumeSlider_Scroll);
            // 
            // messagesAudioDeviceBox
            // 
            this.messagesAudioDeviceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.messagesAudioDeviceBox.Enabled = false;
            this.messagesAudioDeviceBox.IntegralHeight = false;
            this.messagesAudioDeviceBox.Location = new System.Drawing.Point(330, 90);
            this.messagesAudioDeviceBox.MaxDropDownItems = 5;
            this.messagesAudioDeviceBox.Name = "messagesAudioDeviceBox";
            this.messagesAudioDeviceBox.Size = new System.Drawing.Size(190, 21);
            this.messagesAudioDeviceBox.TabIndex = 150;
            this.messagesAudioDeviceBox.Visible = false;
            // 
            // speechRecognitionDeviceBox
            // 
            this.speechRecognitionDeviceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.speechRecognitionDeviceBox.Enabled = false;
            this.speechRecognitionDeviceBox.IntegralHeight = false;
            this.speechRecognitionDeviceBox.Location = new System.Drawing.Point(115, 90);
            this.speechRecognitionDeviceBox.MaxDropDownItems = 5;
            this.speechRecognitionDeviceBox.Name = "speechRecognitionDeviceBox";
            this.speechRecognitionDeviceBox.Size = new System.Drawing.Size(190, 21);
            this.speechRecognitionDeviceBox.TabIndex = 140;
            this.speechRecognitionDeviceBox.Visible = false;
            // 
            // backgroundAudioDeviceBox
            // 
            this.backgroundAudioDeviceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backgroundAudioDeviceBox.Enabled = false;
            this.backgroundAudioDeviceBox.IntegralHeight = false;
            this.backgroundAudioDeviceBox.Location = new System.Drawing.Point(550, 90);
            this.backgroundAudioDeviceBox.MaxDropDownItems = 5;
            this.backgroundAudioDeviceBox.Name = "backgroundAudioDeviceBox";
            this.backgroundAudioDeviceBox.Size = new System.Drawing.Size(190, 21);
            this.backgroundAudioDeviceBox.TabIndex = 160;
            this.backgroundAudioDeviceBox.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(367, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 59;
            this.label3.Text = "messages_volume";
            // 
            // backgroundVolumeSlider
            // 
            this.backgroundVolumeSlider.Location = new System.Drawing.Point(558, 28);
            this.backgroundVolumeSlider.Maximum = 100;
            this.backgroundVolumeSlider.Name = "backgroundVolumeSlider";
            this.backgroundVolumeSlider.Size = new System.Drawing.Size(184, 45);
            this.backgroundVolumeSlider.TabIndex = 70;
            this.backgroundVolumeSlider.TickFrequency = 10;
            this.backgroundVolumeSlider.Scroll += new System.EventHandler(this.backgroundVolumeSlider_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(567, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 69;
            this.label4.Text = "background_volume";
            // 
            // gameDefinitionList
            // 
            this.gameDefinitionList.AllowDrop = true;
            this.gameDefinitionList.FormattingEnabled = true;
            this.gameDefinitionList.Items.AddRange(new object[] {
            "assetto_32_bit",
            "assetto_64_bit",
            "automobilista",
            "f1_2018",
            "ftruck",
            "gamestockcar",
            "iracing",
            "marcas",
            "pcars_2",
            "pcars_32_bit",
            "pcars_64_bit",
            "pcars_udp",
            "race_room",
            "rfactor1",
            "rfactor2_64_bit"});
            this.gameDefinitionList.Location = new System.Drawing.Point(782, 28);
            this.gameDefinitionList.MaximumSize = new System.Drawing.Size(170, 173);
            this.gameDefinitionList.MinimumSize = new System.Drawing.Size(170, 173);
            this.gameDefinitionList.Name = "gameDefinitionList";
            this.gameDefinitionList.Size = new System.Drawing.Size(170, 173);
            this.gameDefinitionList.TabIndex = 80;
            this.gameDefinitionList.SelectedValueChanged += new System.EventHandler(this.updateSelectedGameDefinition);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(779, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 79;
            this.label5.Text = "game";
            // 
            // personalisationLabel
            // 
            this.personalisationLabel.AutoSize = true;
            this.personalisationLabel.Location = new System.Drawing.Point(961, 11);
            this.personalisationLabel.Name = "personalisationLabel";
            this.personalisationLabel.Size = new System.Drawing.Size(105, 13);
            this.personalisationLabel.TabIndex = 89;
            this.personalisationLabel.Text = "personalisation_label";
            this.myNameBoxTooltip.SetToolTip(this.personalisationLabel, "personalisation_tooltip");
            // 
            // filenameTextbox
            // 
            this.filenameTextbox.Location = new System.Drawing.Point(150, 2);
            this.filenameTextbox.Name = "filenameTextbox";
            this.filenameTextbox.Size = new System.Drawing.Size(108, 20);
            this.filenameTextbox.TabIndex = 20;
            // 
            // filenameLabel
            // 
            this.filenameLabel.AutoSize = true;
            this.filenameLabel.Location = new System.Drawing.Point(68, 5);
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(82, 13);
            this.filenameLabel.TabIndex = 19;
            this.filenameLabel.Text = "File &name to run";
            // 
            // recordSession
            // 
            this.recordSession.AutoSize = true;
            this.recordSession.Location = new System.Drawing.Point(7, 4);
            this.recordSession.Name = "recordSession";
            this.recordSession.Size = new System.Drawing.Size(61, 17);
            this.recordSession.TabIndex = 10;
            this.recordSession.Text = "&Record";
            this.recordSession.UseVisualStyleBackColor = true;
            // 
            // playbackInterval
            // 
            this.playbackInterval.Location = new System.Drawing.Point(261, 2);
            this.playbackInterval.Name = "playbackInterval";
            this.playbackInterval.Size = new System.Drawing.Size(100, 20);
            this.playbackInterval.TabIndex = 30;
            this.playbackInterval.TextChanged += new System.EventHandler(this.playbackIntervalChanged);
            // 
            // app_version
            // 
            this.app_version.AutoSize = true;
            this.app_version.Location = new System.Drawing.Point(1045, 650);
            this.app_version.Name = "app_version";
            this.app_version.Size = new System.Drawing.Size(65, 13);
            this.app_version.TabIndex = 193;
            this.app_version.Text = "app_version";
            // 
            // soundPackProgressBar
            // 
            this.soundPackProgressBar.Location = new System.Drawing.Point(39, 176);
            this.soundPackProgressBar.Name = "soundPackProgressBar";
            this.soundPackProgressBar.Size = new System.Drawing.Size(220, 23);
            this.soundPackProgressBar.TabIndex = 191;
            // 
            // downloadSoundPackButton
            // 
            this.downloadSoundPackButton.Enabled = false;
            this.downloadSoundPackButton.Location = new System.Drawing.Point(39, 123);
            this.downloadSoundPackButton.Name = "downloadSoundPackButton";
            this.downloadSoundPackButton.Size = new System.Drawing.Size(220, 37);
            this.downloadSoundPackButton.TabIndex = 170;
            this.downloadSoundPackButton.Text = "sound_pack_is_up_to_date";
            this.downloadSoundPackButton.UseVisualStyleBackColor = true;
            this.downloadSoundPackButton.Click += new System.EventHandler(this.downloadSoundPackButtonPress);
            // 
            // downloadDriverNamesButton
            // 
            this.downloadDriverNamesButton.Enabled = false;
            this.downloadDriverNamesButton.Location = new System.Drawing.Point(295, 123);
            this.downloadDriverNamesButton.Name = "downloadDriverNamesButton";
            this.downloadDriverNamesButton.Size = new System.Drawing.Size(220, 37);
            this.downloadDriverNamesButton.TabIndex = 180;
            this.downloadDriverNamesButton.Text = "driver_names_are_up_to_date";
            this.downloadDriverNamesButton.UseVisualStyleBackColor = true;
            this.downloadDriverNamesButton.Click += new System.EventHandler(this.downloadDriverNamesButtonPress);
            // 
            // downloadPersonalisationsButton
            // 
            this.downloadPersonalisationsButton.Enabled = false;
            this.downloadPersonalisationsButton.Location = new System.Drawing.Point(550, 123);
            this.downloadPersonalisationsButton.Name = "downloadPersonalisationsButton";
            this.downloadPersonalisationsButton.Size = new System.Drawing.Size(220, 37);
            this.downloadPersonalisationsButton.TabIndex = 190;
            this.downloadPersonalisationsButton.Text = "personalisations_are_up_to_date";
            this.downloadPersonalisationsButton.UseVisualStyleBackColor = true;
            this.downloadPersonalisationsButton.Click += new System.EventHandler(this.downloadPersonalisationsButtonPress);
            // 
            // driverNamesProgressBar
            // 
            this.driverNamesProgressBar.Location = new System.Drawing.Point(295, 176);
            this.driverNamesProgressBar.Name = "driverNamesProgressBar";
            this.driverNamesProgressBar.Size = new System.Drawing.Size(220, 23);
            this.driverNamesProgressBar.TabIndex = 0;
            // 
            // personalisationsProgressBar
            // 
            this.personalisationsProgressBar.Location = new System.Drawing.Point(550, 176);
            this.personalisationsProgressBar.Name = "personalisationsProgressBar";
            this.personalisationsProgressBar.Size = new System.Drawing.Size(220, 23);
            this.personalisationsProgressBar.TabIndex = 192;
            // 
            // personalisationBox
            // 
            this.personalisationBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.personalisationBox.IntegralHeight = false;
            this.personalisationBox.Location = new System.Drawing.Point(1027, 9);
            this.personalisationBox.MaxDropDownItems = 5;
            this.personalisationBox.Name = "personalisationBox";
            this.personalisationBox.Size = new System.Drawing.Size(106, 21);
            this.personalisationBox.TabIndex = 90;
            this.myNameBoxTooltip.SetToolTip(this.personalisationBox, "personalisation_tooltip");
            // 
            // spotterNameLabel
            // 
            this.spotterNameLabel.AutoSize = true;
            this.spotterNameLabel.Location = new System.Drawing.Point(961, 75);
            this.spotterNameLabel.Name = "spotterNameLabel";
            this.spotterNameLabel.Size = new System.Drawing.Size(99, 13);
            this.spotterNameLabel.TabIndex = 99;
            this.spotterNameLabel.Text = "spotter_name_label";
            this.spotterNameBoxTooltip.SetToolTip(this.spotterNameLabel, "spotter_name_tooltip");
            // 
            // messagesAudioDeviceLabel
            // 
            this.messagesAudioDeviceLabel.AutoSize = true;
            this.messagesAudioDeviceLabel.Location = new System.Drawing.Point(330, 70);
            this.messagesAudioDeviceLabel.Name = "messagesAudioDeviceLabel";
            this.messagesAudioDeviceLabel.Size = new System.Drawing.Size(152, 13);
            this.messagesAudioDeviceLabel.TabIndex = 149;
            this.messagesAudioDeviceLabel.Text = "messages_audio_device_label";
            this.messagesAudioDeviceLabel.Visible = false;
            // 
            // speechRecognitionDeviceLabel
            // 
            this.speechRecognitionDeviceLabel.AutoSize = true;
            this.speechRecognitionDeviceLabel.Location = new System.Drawing.Point(115, 70);
            this.speechRecognitionDeviceLabel.Name = "speechRecognitionDeviceLabel";
            this.speechRecognitionDeviceLabel.Size = new System.Drawing.Size(166, 13);
            this.speechRecognitionDeviceLabel.TabIndex = 139;
            this.speechRecognitionDeviceLabel.Text = "speech_recognition_device_label";
            this.speechRecognitionDeviceLabel.Visible = false;
            // 
            // backgroundAudioDeviceLabel
            // 
            this.backgroundAudioDeviceLabel.AutoSize = true;
            this.backgroundAudioDeviceLabel.Location = new System.Drawing.Point(550, 70);
            this.backgroundAudioDeviceLabel.Name = "backgroundAudioDeviceLabel";
            this.backgroundAudioDeviceLabel.Size = new System.Drawing.Size(162, 13);
            this.backgroundAudioDeviceLabel.TabIndex = 159;
            this.backgroundAudioDeviceLabel.Text = "background_audio_device_label";
            this.backgroundAudioDeviceLabel.Visible = false;
            // 
            // spotterNameBox
            // 
            this.spotterNameBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spotterNameBox.IntegralHeight = false;
            this.spotterNameBox.Location = new System.Drawing.Point(1027, 73);
            this.spotterNameBox.MaxDropDownItems = 5;
            this.spotterNameBox.Name = "spotterNameBox";
            this.spotterNameBox.Size = new System.Drawing.Size(106, 21);
            this.spotterNameBox.TabIndex = 100;
            this.spotterNameBoxTooltip.SetToolTip(this.spotterNameBox, "spotter_name_tooltip");
            // 
            // donateLink
            // 
            this.donateLink.Location = new System.Drawing.Point(35, 660);
            this.donateLink.Name = "donateLink";
            this.donateLink.Size = new System.Drawing.Size(250, 15);
            this.donateLink.TabIndex = 270;
            this.donateLink.TabStop = true;
            this.donateLink.Text = "donate_link_text";
            this.donateLink.Click += new System.EventHandler(this.internetPanHandler);
            // 
            // smokeTestTextBox
            // 
            this.smokeTestTextBox.Location = new System.Drawing.Point(847, 215);
            this.smokeTestTextBox.MaxLength = 99999999;
            this.smokeTestTextBox.Multiline = true;
            this.smokeTestTextBox.Name = "smokeTestTextBox";
            this.smokeTestTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.smokeTestTextBox.Size = new System.Drawing.Size(283, 270);
            this.smokeTestTextBox.TabIndex = 502;
            this.smokeTestTextBox.Visible = false;
            // 
            // buttonSmokeTest
            // 
            this.buttonSmokeTest.Location = new System.Drawing.Point(847, 485);
            this.buttonSmokeTest.Name = "buttonSmokeTest";
            this.buttonSmokeTest.Size = new System.Drawing.Size(283, 23);
            this.buttonSmokeTest.TabIndex = 501;
            this.buttonSmokeTest.Text = "Test Sounds";
            this.buttonSmokeTest.UseVisualStyleBackColor = true;
            this.buttonSmokeTest.Visible = false;
            this.buttonSmokeTest.Click += new System.EventHandler(this.playSmokeTestSounds);
            // 
            // chiefNameLabel
            // 
            this.chiefNameLabel.AutoSize = true;
            this.chiefNameLabel.Location = new System.Drawing.Point(961, 43);
            this.chiefNameLabel.Name = "chiefNameLabel";
            this.chiefNameLabel.Size = new System.Drawing.Size(90, 13);
            this.chiefNameLabel.TabIndex = 94;
            this.chiefNameLabel.Text = "chief_name_label";
            this.chiefNameBoxTooltip.SetToolTip(this.chiefNameLabel, "chief_name_tooltip");
            // 
            // chiefNameBox
            // 
            this.chiefNameBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.chiefNameBox.IntegralHeight = false;
            this.chiefNameBox.Location = new System.Drawing.Point(1027, 41);
            this.chiefNameBox.MaxDropDownItems = 5;
            this.chiefNameBox.Name = "chiefNameBox";
            this.chiefNameBox.Size = new System.Drawing.Size(106, 21);
            this.chiefNameBox.TabIndex = 95;
            this.chiefNameBoxTooltip.SetToolTip(this.chiefNameBox, "chief_name_tooltip");
            // 
            // scanControllers
            // 
            this.scanControllers.Location = new System.Drawing.Point(41, 630);
            this.scanControllers.Name = "scanControllers";
            this.scanControllers.Size = new System.Drawing.Size(248, 20);
            this.scanControllers.TabIndex = 215;
            this.scanControllers.Text = "scan_for_controllers";
            this.scanControllers.Click += new System.EventHandler(this.ScanControllers_Click);
            // 
            // buttonEditCommandMacros
            // 
            this.buttonEditCommandMacros.Location = new System.Drawing.Point(830, 609);
            this.buttonEditCommandMacros.Name = "buttonEditCommandMacros";
            this.buttonEditCommandMacros.Size = new System.Drawing.Size(130, 39);
            this.buttonEditCommandMacros.TabIndex = 252;
            this.buttonEditCommandMacros.Text = "edit_macro_commands";
            this.buttonEditCommandMacros.UseVisualStyleBackColor = true;
            this.buttonEditCommandMacros.Click += new System.EventHandler(this.editCommandMacroButtonClicked);
            // 
            // AddRemoveActions
            // 
            this.AddRemoveActions.Location = new System.Drawing.Point(295, 630);
            this.AddRemoveActions.Name = "AddRemoveActions";
            this.AddRemoveActions.Size = new System.Drawing.Size(529, 20);
            this.AddRemoveActions.TabIndex = 233;
            this.AddRemoveActions.Text = "add_remove_actions";
            this.AddRemoveActions.UseVisualStyleBackColor = true;
            this.AddRemoveActions.Click += new System.EventHandler(this.AddRemoveActions_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 692);
            this.Controls.Add(this.AddRemoveActions);
            this.Controls.Add(this.driverNamesProgressBar);
            this.Controls.Add(this.downloadDriverNamesButton);
            this.Controls.Add(this.downloadSoundPackButton);
            this.Controls.Add(this.downloadPersonalisationsButton);
            this.Controls.Add(this.soundPackProgressBar);
            this.Controls.Add(this.personalisationsProgressBar);
            this.Controls.Add(this.app_version);
            this.Controls.Add(this.playbackInterval);
            this.Controls.Add(this.recordSession);
            this.Controls.Add(this.filenameLabel);
            this.Controls.Add(this.filenameTextbox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.gameDefinitionList);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.backgroundVolumeSlider);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.messagesVolumeSlider);
            this.Controls.Add(this.messagesAudioDeviceBox);
            this.Controls.Add(this.speechRecognitionDeviceBox);
            this.Controls.Add(this.backgroundAudioDeviceBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.propertiesButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.deleteAssigmentButton);
            this.Controls.Add(this.assignButtonToAction);
            this.Controls.Add(this.controllersList);
            this.Controls.Add(this.buttonActionSelect);
            this.Controls.Add(this.startApplicationButton);
            this.Controls.Add(this.forceVersionCheckButton);
            this.Controls.Add(this.consoleTextBoxBackgroundPanel);
            this.Controls.Add(this.personalisationBox);
            this.Controls.Add(this.spotterNameBox);
            this.Controls.Add(this.personalisationLabel);
            this.Controls.Add(this.spotterNameLabel);
            this.Controls.Add(this.messagesAudioDeviceLabel);
            this.Controls.Add(this.speechRecognitionDeviceLabel);
            this.Controls.Add(this.backgroundAudioDeviceLabel);
            this.Controls.Add(this.donateLink);
            this.Controls.Add(this.buttonSmokeTest);
            this.Controls.Add(this.smokeTestTextBox);
            this.Controls.Add(this.chiefNameLabel);
            this.Controls.Add(this.chiefNameBox);
            this.Controls.Add(this.scanControllers);
            this.Controls.Add(this.buttonEditCommandMacros);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Crew Chief V4";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.stopApp);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.consoleTextBoxBackgroundPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.messagesVolumeSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundVolumeSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RichTextBox consoleTextBox;
        private Panel consoleTextBoxBackgroundPanel;
        public System.Windows.Forms.Button startApplicationButton;
        public System.Windows.Forms.CheckBox recordSession;
        private System.Windows.Forms.Button forceVersionCheckButton;
        private System.Windows.Forms.ListBox buttonActionSelect;
        private System.Windows.Forms.ListBox controllersList;
        private System.Windows.Forms.Button assignButtonToAction;
        private System.Windows.Forms.Button deleteAssigmentButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button propertiesButton;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton alwaysOnButton;
        private System.Windows.Forms.RadioButton toggleButton;
        private System.Windows.Forms.RadioButton holdButton;
        private System.Windows.Forms.RadioButton voiceDisableButton;
        private System.Windows.Forms.RadioButton triggerWordButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TrackBar messagesVolumeSlider;
        private System.Windows.Forms.ComboBox speechRecognitionDeviceBox;
        private System.Windows.Forms.ComboBox messagesAudioDeviceBox;
        private System.Windows.Forms.ComboBox backgroundAudioDeviceBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar backgroundVolumeSlider;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ListBox gameDefinitionList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label personalisationLabel;
        public System.Windows.Forms.TextBox filenameTextbox;
        private System.Windows.Forms.Label filenameLabel;
        private System.Windows.Forms.TextBox playbackInterval;
        private System.Windows.Forms.Label app_version;
        private System.Windows.Forms.ProgressBar soundPackProgressBar;
        private System.Windows.Forms.Button downloadSoundPackButton;
        private System.Windows.Forms.Button downloadDriverNamesButton;
        private System.Windows.Forms.Button downloadPersonalisationsButton;
        private System.Windows.Forms.ProgressBar driverNamesProgressBar;
        private System.Windows.Forms.ProgressBar personalisationsProgressBar;
        private System.Windows.Forms.ComboBox personalisationBox;
        private System.Windows.Forms.Label spotterNameLabel;
        private System.Windows.Forms.Label messagesAudioDeviceLabel;
        private System.Windows.Forms.Label speechRecognitionDeviceLabel;
        private System.Windows.Forms.Label backgroundAudioDeviceLabel;
        private System.Windows.Forms.ComboBox spotterNameBox;
        private System.Windows.Forms.LinkLabel donateLink;
        private System.Windows.Forms.TextBox smokeTestTextBox;
        private System.Windows.Forms.Button buttonSmokeTest;
        private System.Windows.Forms.Label chiefNameLabel;
        private System.Windows.Forms.ComboBox chiefNameBox;
        private System.Windows.Forms.ToolTip myNameBoxTooltip;
        private System.Windows.Forms.ToolTip chiefNameBoxTooltip;
        private System.Windows.Forms.ToolTip spotterNameBoxTooltip;
        private System.Windows.Forms.Button scanControllers;
        private System.Windows.Forms.Button buttonEditCommandMacros;
        private ToolTip voiceRecognitionDisabledToolTip;
        private ToolTip voiceRecognitionHoldButtonToolTip;
        private ToolTip voiceRecognitionToggleButtonToolTip;
        private ToolTip voiceRecognitionAlwaysOnToolTip;
        private ToolTip voiceRecognitionTriggerWordToolTip;
        private ToolTip voiceRecognitionToolTip;
        private Button AddRemoveActions;
    }
}
