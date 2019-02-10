namespace CrewChiefV4
{
    partial class MacroEditor
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
            this.listBoxGames = new System.Windows.Forms.ListBox();
            this.listBoxAvailableActions = new System.Windows.Forms.ListBox();
            this.comboBoxKeySelection = new System.Windows.Forms.ComboBox();
            this.buttonSaveSelectedKey = new System.Windows.Forms.Button();
            this.labelGame = new System.Windows.Forms.Label();
            this.labelAvailableActions = new System.Windows.Forms.Label();
            this.listBoxAvailableMacros = new System.Windows.Forms.ListBox();
            this.labelAvailableMacros = new System.Windows.Forms.Label();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelGlobalMacroDescription = new System.Windows.Forms.Label();
            this.labelActionSequence = new System.Windows.Forms.Label();
            this.textBoxActionSequence = new System.Windows.Forms.TextBox();
            this.textBoxVoiceTriggers = new System.Windows.Forms.TextBox();
            this.textBoxAddNewAction = new System.Windows.Forms.TextBox();
            this.labelNewActionName = new System.Windows.Forms.Label();
            this.textBoxKeyPressTime = new System.Windows.Forms.TextBox();
            this.textBoxWaitBetweenEachCommand = new System.Windows.Forms.TextBox();
            this.textBoxConfirmationMessage = new System.Windows.Forms.TextBox();
            this.labelConfirmationMessage = new System.Windows.Forms.Label();
            this.buttonAddActionToSequence = new System.Windows.Forms.Button();
            this.buttonAddSelectedKeyToSequence = new System.Windows.Forms.Button();
            this.labelKeyPressTime = new System.Windows.Forms.Label();
            this.labelWaitBetweenEachCommand = new System.Windows.Forms.Label();
            this.buttonSelectConfirmationMessage = new System.Windows.Forms.Button();
            this.textBoxAddNewMacro = new System.Windows.Forms.TextBox();
            this.buttonAddNewMacro = new System.Windows.Forms.Button();
            this.labelNewMacroName = new System.Windows.Forms.Label();
            this.buttonAddActionSequence = new System.Windows.Forms.Button();
            this.groupBoxGlobalOptins = new System.Windows.Forms.GroupBox();
            this.groupBoxGlobalMacroVoiceTrigger = new System.Windows.Forms.GroupBox();
            this.radioButtonRegularVoiceTrigger = new System.Windows.Forms.RadioButton();
            this.radioButtonIntegerVoiceTrigger = new System.Windows.Forms.RadioButton();
            this.buttonDeleteSelectedMacro = new System.Windows.Forms.Button();
            this.groupBoxGameSettings = new System.Windows.Forms.GroupBox();
            this.labelGameMacroDescription = new System.Windows.Forms.Label();
            this.textBoxGameMacroDescription = new System.Windows.Forms.TextBox();
            this.buttonSaveMacroSettings = new System.Windows.Forms.Button();
            this.buttonLoadUserMacroSettings = new System.Windows.Forms.Button();
            this.buttonLoadDefaultMacroSettings = new System.Windows.Forms.Button();
            this.groupBoxGlobalOptins.SuspendLayout();
            this.groupBoxGlobalMacroVoiceTrigger.SuspendLayout();
            this.groupBoxGameSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxGames
            // 
            this.listBoxGames.FormattingEnabled = true;
            this.listBoxGames.Location = new System.Drawing.Point(9, 32);
            this.listBoxGames.Name = "listBoxGames";
            this.listBoxGames.Size = new System.Drawing.Size(163, 199);
            this.listBoxGames.TabIndex = 0;
            this.listBoxGames.SelectedIndexChanged += new System.EventHandler(this.listBoxGames_SelectedIndexChanged);
            // 
            // listBoxAvailableActions
            // 
            this.listBoxAvailableActions.FormattingEnabled = true;
            this.listBoxAvailableActions.Location = new System.Drawing.Point(6, 37);
            this.listBoxAvailableActions.Name = "listBoxAvailableActions";
            this.listBoxAvailableActions.Size = new System.Drawing.Size(315, 95);
            this.listBoxAvailableActions.TabIndex = 1;
            this.listBoxAvailableActions.SelectedIndexChanged += new System.EventHandler(this.listBoxAvailableActions_SelectedIndexChanged);
            // 
            // comboBoxKeySelection
            // 
            this.comboBoxKeySelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeySelection.FormattingEnabled = true;
            this.comboBoxKeySelection.Location = new System.Drawing.Point(166, 154);
            this.comboBoxKeySelection.Name = "comboBoxKeySelection";
            this.comboBoxKeySelection.Size = new System.Drawing.Size(155, 21);
            this.comboBoxKeySelection.TabIndex = 2;
            // 
            // buttonSaveSelectedKey
            // 
            this.buttonSaveSelectedKey.Location = new System.Drawing.Point(8, 180);
            this.buttonSaveSelectedKey.Name = "buttonSaveSelectedKey";
            this.buttonSaveSelectedKey.Size = new System.Drawing.Size(313, 26);
            this.buttonSaveSelectedKey.TabIndex = 3;
            this.buttonSaveSelectedKey.Text = "save_action_key";
            this.buttonSaveSelectedKey.UseVisualStyleBackColor = true;
            this.buttonSaveSelectedKey.Click += new System.EventHandler(this.buttonSaveSelectedKey_Click);
            // 
            // labelGame
            // 
            this.labelGame.AutoSize = true;
            this.labelGame.Location = new System.Drawing.Point(6, 16);
            this.labelGame.Name = "labelGame";
            this.labelGame.Size = new System.Drawing.Size(33, 13);
            this.labelGame.TabIndex = 4;
            this.labelGame.Text = "game";
            // 
            // labelAvailableActions
            // 
            this.labelAvailableActions.AutoSize = true;
            this.labelAvailableActions.Location = new System.Drawing.Point(5, 19);
            this.labelAvailableActions.Name = "labelAvailableActions";
            this.labelAvailableActions.Size = new System.Drawing.Size(89, 13);
            this.labelAvailableActions.TabIndex = 5;
            this.labelAvailableActions.Text = "available_actions";
            // 
            // listBoxAvailableMacros
            // 
            this.listBoxAvailableMacros.FormattingEnabled = true;
            this.listBoxAvailableMacros.Location = new System.Drawing.Point(178, 32);
            this.listBoxAvailableMacros.Name = "listBoxAvailableMacros";
            this.listBoxAvailableMacros.Size = new System.Drawing.Size(229, 121);
            this.listBoxAvailableMacros.TabIndex = 6;
            this.listBoxAvailableMacros.SelectedIndexChanged += new System.EventHandler(this.listBoxAvailableMacros_SelectedIndexChanged);
            // 
            // labelAvailableMacros
            // 
            this.labelAvailableMacros.AutoSize = true;
            this.labelAvailableMacros.Location = new System.Drawing.Point(175, 16);
            this.labelAvailableMacros.Name = "labelAvailableMacros";
            this.labelAvailableMacros.Size = new System.Drawing.Size(89, 13);
            this.labelAvailableMacros.TabIndex = 7;
            this.labelAvailableMacros.Text = "available_macros";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(588, 26);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDescription.Size = new System.Drawing.Size(169, 205);
            this.textBoxDescription.TabIndex = 8;
            // 
            // labelGlobalMacroDescription
            // 
            this.labelGlobalMacroDescription.AutoSize = true;
            this.labelGlobalMacroDescription.Location = new System.Drawing.Point(585, 10);
            this.labelGlobalMacroDescription.Name = "labelGlobalMacroDescription";
            this.labelGlobalMacroDescription.Size = new System.Drawing.Size(93, 13);
            this.labelGlobalMacroDescription.TabIndex = 9;
            this.labelGlobalMacroDescription.Text = "macro_description";
            // 
            // labelActionSequence
            // 
            this.labelActionSequence.AutoSize = true;
            this.labelActionSequence.Location = new System.Drawing.Point(324, 21);
            this.labelActionSequence.Name = "labelActionSequence";
            this.labelActionSequence.Size = new System.Drawing.Size(89, 13);
            this.labelActionSequence.TabIndex = 11;
            this.labelActionSequence.Text = "action_sequence";
            // 
            // textBoxActionSequence
            // 
            this.textBoxActionSequence.Location = new System.Drawing.Point(327, 37);
            this.textBoxActionSequence.Multiline = true;
            this.textBoxActionSequence.Name = "textBoxActionSequence";
            this.textBoxActionSequence.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxActionSequence.Size = new System.Drawing.Size(255, 97);
            this.textBoxActionSequence.TabIndex = 12;
            // 
            // textBoxVoiceTriggers
            // 
            this.textBoxVoiceTriggers.Location = new System.Drawing.Point(5, 71);
            this.textBoxVoiceTriggers.Multiline = true;
            this.textBoxVoiceTriggers.Name = "textBoxVoiceTriggers";
            this.textBoxVoiceTriggers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxVoiceTriggers.Size = new System.Drawing.Size(157, 57);
            this.textBoxVoiceTriggers.TabIndex = 13;
            // 
            // textBoxAddNewAction
            // 
            this.textBoxAddNewAction.Location = new System.Drawing.Point(8, 154);
            this.textBoxAddNewAction.Name = "textBoxAddNewAction";
            this.textBoxAddNewAction.Size = new System.Drawing.Size(152, 20);
            this.textBoxAddNewAction.TabIndex = 15;
            // 
            // labelNewActionName
            // 
            this.labelNewActionName.AutoSize = true;
            this.labelNewActionName.Location = new System.Drawing.Point(5, 135);
            this.labelNewActionName.Name = "labelNewActionName";
            this.labelNewActionName.Size = new System.Drawing.Size(62, 13);
            this.labelNewActionName.TabIndex = 16;
            this.labelNewActionName.Text = "new_action";
            // 
            // textBoxKeyPressTime
            // 
            this.textBoxKeyPressTime.Location = new System.Drawing.Point(327, 155);
            this.textBoxKeyPressTime.Name = "textBoxKeyPressTime";
            this.textBoxKeyPressTime.Size = new System.Drawing.Size(124, 20);
            this.textBoxKeyPressTime.TabIndex = 17;
            this.textBoxKeyPressTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyPressTime_KeyPress);
            // 
            // textBoxWaitBetweenEachCommand
            // 
            this.textBoxWaitBetweenEachCommand.Location = new System.Drawing.Point(457, 155);
            this.textBoxWaitBetweenEachCommand.Name = "textBoxWaitBetweenEachCommand";
            this.textBoxWaitBetweenEachCommand.Size = new System.Drawing.Size(125, 20);
            this.textBoxWaitBetweenEachCommand.TabIndex = 18;
            this.textBoxWaitBetweenEachCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxWaitBetweenEachCommand_KeyPress);
            // 
            // textBoxConfirmationMessage
            // 
            this.textBoxConfirmationMessage.Enabled = false;
            this.textBoxConfirmationMessage.Location = new System.Drawing.Point(414, 172);
            this.textBoxConfirmationMessage.Name = "textBoxConfirmationMessage";
            this.textBoxConfirmationMessage.Size = new System.Drawing.Size(169, 20);
            this.textBoxConfirmationMessage.TabIndex = 19;
            // 
            // labelConfirmationMessage
            // 
            this.labelConfirmationMessage.AutoSize = true;
            this.labelConfirmationMessage.Location = new System.Drawing.Point(410, 156);
            this.labelConfirmationMessage.Name = "labelConfirmationMessage";
            this.labelConfirmationMessage.Size = new System.Drawing.Size(112, 13);
            this.labelConfirmationMessage.TabIndex = 20;
            this.labelConfirmationMessage.Text = "confirmation_message";
            // 
            // buttonAddActionToSequence
            // 
            this.buttonAddActionToSequence.Location = new System.Drawing.Point(8, 212);
            this.buttonAddActionToSequence.Name = "buttonAddActionToSequence";
            this.buttonAddActionToSequence.Size = new System.Drawing.Size(152, 30);
            this.buttonAddActionToSequence.TabIndex = 21;
            this.buttonAddActionToSequence.Text = "add_action_to_sequence";
            this.buttonAddActionToSequence.UseVisualStyleBackColor = true;
            this.buttonAddActionToSequence.Click += new System.EventHandler(this.buttonAddActionToSequence_Click);
            // 
            // buttonAddSelectedKeyToSequence
            // 
            this.buttonAddSelectedKeyToSequence.Location = new System.Drawing.Point(166, 212);
            this.buttonAddSelectedKeyToSequence.Name = "buttonAddSelectedKeyToSequence";
            this.buttonAddSelectedKeyToSequence.Size = new System.Drawing.Size(155, 30);
            this.buttonAddSelectedKeyToSequence.TabIndex = 22;
            this.buttonAddSelectedKeyToSequence.Text = "add_key_to_sequence";
            this.buttonAddSelectedKeyToSequence.UseVisualStyleBackColor = true;
            this.buttonAddSelectedKeyToSequence.Click += new System.EventHandler(this.buttonAddSelectedKeyToSequence_Click);
            // 
            // labelKeyPressTime
            // 
            this.labelKeyPressTime.AutoSize = true;
            this.labelKeyPressTime.Location = new System.Drawing.Point(324, 137);
            this.labelKeyPressTime.Name = "labelKeyPressTime";
            this.labelKeyPressTime.Size = new System.Drawing.Size(74, 13);
            this.labelKeyPressTime.TabIndex = 23;
            this.labelKeyPressTime.Text = "keypress_time";
            // 
            // labelWaitBetweenEachCommand
            // 
            this.labelWaitBetweenEachCommand.AutoSize = true;
            this.labelWaitBetweenEachCommand.Location = new System.Drawing.Point(454, 139);
            this.labelWaitBetweenEachCommand.Name = "labelWaitBetweenEachCommand";
            this.labelWaitBetweenEachCommand.Size = new System.Drawing.Size(99, 13);
            this.labelWaitBetweenEachCommand.TabIndex = 24;
            this.labelWaitBetweenEachCommand.Text = "keypress_wait_time";
            // 
            // buttonSelectConfirmationMessage
            // 
            this.buttonSelectConfirmationMessage.Location = new System.Drawing.Point(414, 195);
            this.buttonSelectConfirmationMessage.Name = "buttonSelectConfirmationMessage";
            this.buttonSelectConfirmationMessage.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonSelectConfirmationMessage.Size = new System.Drawing.Size(169, 37);
            this.buttonSelectConfirmationMessage.TabIndex = 25;
            this.buttonSelectConfirmationMessage.Text = "select_confirmation_message";
            this.buttonSelectConfirmationMessage.UseVisualStyleBackColor = true;
            this.buttonSelectConfirmationMessage.Click += new System.EventHandler(this.buttonSelectConfirmationMessage_Click);
            // 
            // textBoxAddNewMacro
            // 
            this.textBoxAddNewMacro.Location = new System.Drawing.Point(178, 172);
            this.textBoxAddNewMacro.Name = "textBoxAddNewMacro";
            this.textBoxAddNewMacro.Size = new System.Drawing.Size(229, 20);
            this.textBoxAddNewMacro.TabIndex = 26;
            // 
            // buttonAddNewMacro
            // 
            this.buttonAddNewMacro.Location = new System.Drawing.Point(178, 195);
            this.buttonAddNewMacro.Name = "buttonAddNewMacro";
            this.buttonAddNewMacro.Size = new System.Drawing.Size(111, 37);
            this.buttonAddNewMacro.TabIndex = 27;
            this.buttonAddNewMacro.Text = "add_or_edit_macro";
            this.buttonAddNewMacro.UseVisualStyleBackColor = true;
            this.buttonAddNewMacro.Click += new System.EventHandler(this.buttonAddNewMacro_Click);
            // 
            // labelNewMacroName
            // 
            this.labelNewMacroName.AutoSize = true;
            this.labelNewMacroName.Location = new System.Drawing.Point(178, 156);
            this.labelNewMacroName.Name = "labelNewMacroName";
            this.labelNewMacroName.Size = new System.Drawing.Size(94, 13);
            this.labelNewMacroName.TabIndex = 28;
            this.labelNewMacroName.Text = "new_macro_name";
            // 
            // buttonAddActionSequence
            // 
            this.buttonAddActionSequence.Location = new System.Drawing.Point(327, 180);
            this.buttonAddActionSequence.Name = "buttonAddActionSequence";
            this.buttonAddActionSequence.Size = new System.Drawing.Size(257, 62);
            this.buttonAddActionSequence.TabIndex = 29;
            this.buttonAddActionSequence.Text = "add_action_sequece";
            this.buttonAddActionSequence.UseVisualStyleBackColor = true;
            this.buttonAddActionSequence.Click += new System.EventHandler(this.buttonAddActionSequence_Click);
            // 
            // groupBoxGlobalOptins
            // 
            this.groupBoxGlobalOptins.Controls.Add(this.groupBoxGlobalMacroVoiceTrigger);
            this.groupBoxGlobalOptins.Controls.Add(this.buttonDeleteSelectedMacro);
            this.groupBoxGlobalOptins.Controls.Add(this.listBoxGames);
            this.groupBoxGlobalOptins.Controls.Add(this.listBoxAvailableMacros);
            this.groupBoxGlobalOptins.Controls.Add(this.textBoxDescription);
            this.groupBoxGlobalOptins.Controls.Add(this.buttonAddNewMacro);
            this.groupBoxGlobalOptins.Controls.Add(this.labelNewMacroName);
            this.groupBoxGlobalOptins.Controls.Add(this.buttonSelectConfirmationMessage);
            this.groupBoxGlobalOptins.Controls.Add(this.textBoxAddNewMacro);
            this.groupBoxGlobalOptins.Controls.Add(this.labelGame);
            this.groupBoxGlobalOptins.Controls.Add(this.labelAvailableMacros);
            this.groupBoxGlobalOptins.Controls.Add(this.labelConfirmationMessage);
            this.groupBoxGlobalOptins.Controls.Add(this.labelGlobalMacroDescription);
            this.groupBoxGlobalOptins.Controls.Add(this.textBoxConfirmationMessage);
            this.groupBoxGlobalOptins.Location = new System.Drawing.Point(9, 11);
            this.groupBoxGlobalOptins.Name = "groupBoxGlobalOptins";
            this.groupBoxGlobalOptins.Size = new System.Drawing.Size(763, 245);
            this.groupBoxGlobalOptins.TabIndex = 30;
            this.groupBoxGlobalOptins.TabStop = false;
            this.groupBoxGlobalOptins.Text = "global_macro_settings";
            // 
            // groupBoxGlobalMacroVoiceTrigger
            // 
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.radioButtonRegularVoiceTrigger);
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.radioButtonIntegerVoiceTrigger);
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.textBoxVoiceTriggers);
            this.groupBoxGlobalMacroVoiceTrigger.Location = new System.Drawing.Point(414, 19);
            this.groupBoxGlobalMacroVoiceTrigger.Name = "groupBoxGlobalMacroVoiceTrigger";
            this.groupBoxGlobalMacroVoiceTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBoxGlobalMacroVoiceTrigger.Size = new System.Drawing.Size(168, 134);
            this.groupBoxGlobalMacroVoiceTrigger.TabIndex = 31;
            this.groupBoxGlobalMacroVoiceTrigger.TabStop = false;
            this.groupBoxGlobalMacroVoiceTrigger.Text = "macro_voice_trigger";
            // 
            // radioButtonRegularVoiceTrigger
            // 
            this.radioButtonRegularVoiceTrigger.AutoSize = true;
            this.radioButtonRegularVoiceTrigger.Location = new System.Drawing.Point(6, 19);
            this.radioButtonRegularVoiceTrigger.Name = "radioButtonRegularVoiceTrigger";
            this.radioButtonRegularVoiceTrigger.Size = new System.Drawing.Size(176, 17);
            this.radioButtonRegularVoiceTrigger.TabIndex = 31;
            this.radioButtonRegularVoiceTrigger.TabStop = true;
            this.radioButtonRegularVoiceTrigger.Text = "regular_macro_voice_command";
            this.radioButtonRegularVoiceTrigger.UseVisualStyleBackColor = true;
            // 
            // radioButtonIntegerVoiceTrigger
            // 
            this.radioButtonIntegerVoiceTrigger.AutoSize = true;
            this.radioButtonIntegerVoiceTrigger.Location = new System.Drawing.Point(6, 42);
            this.radioButtonIntegerVoiceTrigger.Name = "radioButtonIntegerVoiceTrigger";
            this.radioButtonIntegerVoiceTrigger.Size = new System.Drawing.Size(176, 17);
            this.radioButtonIntegerVoiceTrigger.TabIndex = 30;
            this.radioButtonIntegerVoiceTrigger.TabStop = true;
            this.radioButtonIntegerVoiceTrigger.Text = "integer_macro_voice_command";
            this.radioButtonIntegerVoiceTrigger.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteSelectedMacro
            // 
            this.buttonDeleteSelectedMacro.Location = new System.Drawing.Point(294, 195);
            this.buttonDeleteSelectedMacro.Name = "buttonDeleteSelectedMacro";
            this.buttonDeleteSelectedMacro.Size = new System.Drawing.Size(113, 37);
            this.buttonDeleteSelectedMacro.TabIndex = 29;
            this.buttonDeleteSelectedMacro.Text = "delete_selected_macro";
            this.buttonDeleteSelectedMacro.UseVisualStyleBackColor = true;
            this.buttonDeleteSelectedMacro.Click += new System.EventHandler(this.buttonDeleteSelectedMacro_Click);
            // 
            // groupBoxGameSettings
            // 
            this.groupBoxGameSettings.Controls.Add(this.labelGameMacroDescription);
            this.groupBoxGameSettings.Controls.Add(this.textBoxGameMacroDescription);
            this.groupBoxGameSettings.Controls.Add(this.textBoxActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.listBoxAvailableActions);
            this.groupBoxGameSettings.Controls.Add(this.comboBoxKeySelection);
            this.groupBoxGameSettings.Controls.Add(this.buttonAddActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.labelWaitBetweenEachCommand);
            this.groupBoxGameSettings.Controls.Add(this.buttonSaveSelectedKey);
            this.groupBoxGameSettings.Controls.Add(this.labelActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.buttonAddActionToSequence);
            this.groupBoxGameSettings.Controls.Add(this.labelKeyPressTime);
            this.groupBoxGameSettings.Controls.Add(this.labelNewActionName);
            this.groupBoxGameSettings.Controls.Add(this.textBoxKeyPressTime);
            this.groupBoxGameSettings.Controls.Add(this.labelAvailableActions);
            this.groupBoxGameSettings.Controls.Add(this.textBoxWaitBetweenEachCommand);
            this.groupBoxGameSettings.Controls.Add(this.buttonAddSelectedKeyToSequence);
            this.groupBoxGameSettings.Controls.Add(this.textBoxAddNewAction);
            this.groupBoxGameSettings.Location = new System.Drawing.Point(9, 262);
            this.groupBoxGameSettings.Name = "groupBoxGameSettings";
            this.groupBoxGameSettings.Size = new System.Drawing.Size(763, 257);
            this.groupBoxGameSettings.TabIndex = 31;
            this.groupBoxGameSettings.TabStop = false;
            this.groupBoxGameSettings.Text = "game_specific_settings";
            // 
            // labelGameMacroDescription
            // 
            this.labelGameMacroDescription.AutoSize = true;
            this.labelGameMacroDescription.Location = new System.Drawing.Point(586, 21);
            this.labelGameMacroDescription.Name = "labelGameMacroDescription";
            this.labelGameMacroDescription.Size = new System.Drawing.Size(93, 13);
            this.labelGameMacroDescription.TabIndex = 31;
            this.labelGameMacroDescription.Text = "macro_description";
            // 
            // textBoxGameMacroDescription
            // 
            this.textBoxGameMacroDescription.Location = new System.Drawing.Point(589, 37);
            this.textBoxGameMacroDescription.Multiline = true;
            this.textBoxGameMacroDescription.Name = "textBoxGameMacroDescription";
            this.textBoxGameMacroDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxGameMacroDescription.Size = new System.Drawing.Size(168, 205);
            this.textBoxGameMacroDescription.TabIndex = 30;
            // 
            // buttonSaveMacroSettings
            // 
            this.buttonSaveMacroSettings.Location = new System.Drawing.Point(597, 525);
            this.buttonSaveMacroSettings.Name = "buttonSaveMacroSettings";
            this.buttonSaveMacroSettings.Size = new System.Drawing.Size(175, 35);
            this.buttonSaveMacroSettings.TabIndex = 32;
            this.buttonSaveMacroSettings.Text = "save_macro_settings";
            this.buttonSaveMacroSettings.UseVisualStyleBackColor = true;
            this.buttonSaveMacroSettings.Click += new System.EventHandler(this.buttonSaveMacroSettings_Click);
            // 
            // buttonLoadUserMacroSettings
            // 
            this.buttonLoadUserMacroSettings.Location = new System.Drawing.Point(9, 525);
            this.buttonLoadUserMacroSettings.Name = "buttonLoadUserMacroSettings";
            this.buttonLoadUserMacroSettings.Size = new System.Drawing.Size(160, 35);
            this.buttonLoadUserMacroSettings.TabIndex = 33;
            this.buttonLoadUserMacroSettings.Text = "load_user_macro_settings";
            this.buttonLoadUserMacroSettings.UseVisualStyleBackColor = true;
            this.buttonLoadUserMacroSettings.Click += new System.EventHandler(this.buttonLoadUserMacroSettings_Click);
            // 
            // buttonLoadDefaultMacroSettings
            // 
            this.buttonLoadDefaultMacroSettings.Location = new System.Drawing.Point(175, 525);
            this.buttonLoadDefaultMacroSettings.Name = "buttonLoadDefaultMacroSettings";
            this.buttonLoadDefaultMacroSettings.Size = new System.Drawing.Size(155, 35);
            this.buttonLoadDefaultMacroSettings.TabIndex = 34;
            this.buttonLoadDefaultMacroSettings.Text = "load_default_macro_settings";
            this.buttonLoadDefaultMacroSettings.UseVisualStyleBackColor = true;
            this.buttonLoadDefaultMacroSettings.Click += new System.EventHandler(this.buttonLoadDefaultMacroSettings_Click);
            // 
            // MacroEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(784, 572);
            this.Controls.Add(this.buttonLoadDefaultMacroSettings);
            this.Controls.Add(this.buttonLoadUserMacroSettings);
            this.Controls.Add(this.buttonSaveMacroSettings);
            this.Controls.Add(this.groupBoxGameSettings);
            this.Controls.Add(this.groupBoxGlobalOptins);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MacroEditor";
            this.Text = "Command Macro Editor";
            this.groupBoxGlobalOptins.ResumeLayout(false);
            this.groupBoxGlobalOptins.PerformLayout();
            this.groupBoxGlobalMacroVoiceTrigger.ResumeLayout(false);
            this.groupBoxGlobalMacroVoiceTrigger.PerformLayout();
            this.groupBoxGameSettings.ResumeLayout(false);
            this.groupBoxGameSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxGames;
        private System.Windows.Forms.ListBox listBoxAvailableActions;
        private System.Windows.Forms.ComboBox comboBoxKeySelection;
        private System.Windows.Forms.Button buttonSaveSelectedKey;
        private System.Windows.Forms.Label labelGame;
        private System.Windows.Forms.Label labelAvailableActions;
        private System.Windows.Forms.ListBox listBoxAvailableMacros;
        private System.Windows.Forms.Label labelAvailableMacros;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelGlobalMacroDescription;
        private System.Windows.Forms.Label labelActionSequence;
        private System.Windows.Forms.TextBox textBoxActionSequence;
        private System.Windows.Forms.TextBox textBoxVoiceTriggers;
        private System.Windows.Forms.TextBox textBoxAddNewAction;
        private System.Windows.Forms.Label labelNewActionName;
        private System.Windows.Forms.TextBox textBoxKeyPressTime;
        private System.Windows.Forms.TextBox textBoxWaitBetweenEachCommand;
        private System.Windows.Forms.TextBox textBoxConfirmationMessage;
        private System.Windows.Forms.Label labelConfirmationMessage;
        private System.Windows.Forms.Button buttonAddActionToSequence;
        private System.Windows.Forms.Button buttonAddSelectedKeyToSequence;
        private System.Windows.Forms.Label labelKeyPressTime;
        private System.Windows.Forms.Label labelWaitBetweenEachCommand;
        private System.Windows.Forms.Button buttonSelectConfirmationMessage;
        private System.Windows.Forms.TextBox textBoxAddNewMacro;
        private System.Windows.Forms.Button buttonAddNewMacro;
        private System.Windows.Forms.Label labelNewMacroName;
        private System.Windows.Forms.Button buttonAddActionSequence;
        private System.Windows.Forms.GroupBox groupBoxGlobalOptins;
        private System.Windows.Forms.GroupBox groupBoxGameSettings;
        private System.Windows.Forms.Button buttonDeleteSelectedMacro;
        private System.Windows.Forms.Button buttonSaveMacroSettings;
        private System.Windows.Forms.Button buttonLoadUserMacroSettings;
        private System.Windows.Forms.Button buttonLoadDefaultMacroSettings;
        private System.Windows.Forms.GroupBox groupBoxGlobalMacroVoiceTrigger;
        private System.Windows.Forms.RadioButton radioButtonRegularVoiceTrigger;
        private System.Windows.Forms.RadioButton radioButtonIntegerVoiceTrigger;
        private System.Windows.Forms.TextBox textBoxGameMacroDescription;
        private System.Windows.Forms.Label labelGameMacroDescription;
    }
}