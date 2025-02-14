﻿namespace CrewChiefV4
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
            this.comboBoxKeySelection = new System.Windows.Forms.ComboBox();
            this.labelGame = new System.Windows.Forms.Label();
            this.listBoxAvailableMacros = new System.Windows.Forms.ListBox();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelGlobalMacroDescription = new System.Windows.Forms.Label();
            this.labelActionSequence = new System.Windows.Forms.Label();
            this.textBoxActionSequence = new System.Windows.Forms.TextBox();
            this.textBoxVoiceTriggers = new System.Windows.Forms.TextBox();
            this.textBoxKeyPressTime = new System.Windows.Forms.TextBox();
            this.textBoxWaitBetweenEachCommand = new System.Windows.Forms.TextBox();
            this.textBoxConfirmationMessage = new System.Windows.Forms.TextBox();
            this.labelConfirmationMessage = new System.Windows.Forms.Label();
            this.buttonAddSelectedKeyToSequence = new System.Windows.Forms.Button();
            this.labelKeyPressTime = new System.Windows.Forms.Label();
            this.labelWaitBetweenEachCommand = new System.Windows.Forms.Label();
            this.buttonSelectConfirmationMessage = new System.Windows.Forms.Button();
            this.textBoxAddNewMacro = new System.Windows.Forms.TextBox();
            this.buttonAddNewMacro = new System.Windows.Forms.Button();
            this.buttonAddActionSequence = new System.Windows.Forms.Button();
            this.groupBoxGlobalOptins = new System.Windows.Forms.GroupBox();
            this.deleteAssignmentButton = new System.Windows.Forms.Button();
            this.addAssignmentButton = new System.Windows.Forms.Button();
            this.currentAssignmentLabel = new System.Windows.Forms.Label();
            this.groupBoxAvailableMacros = new System.Windows.Forms.GroupBox();
            this.labelMacroEditMode = new System.Windows.Forms.Label();
            this.buttonDeleteSelectedMacro = new System.Windows.Forms.Button();
            this.radioButtonAddNewMacro = new System.Windows.Forms.RadioButton();
            this.radioButtonEditSelectedMacro = new System.Windows.Forms.RadioButton();
            this.radioButtonViewOnly = new System.Windows.Forms.RadioButton();
            this.groupBoxGlobalMacroVoiceTrigger = new System.Windows.Forms.GroupBox();
            this.radioButtonRegularVoiceTrigger = new System.Windows.Forms.RadioButton();
            this.radioButtonIntegerVoiceTrigger = new System.Windows.Forms.RadioButton();
            this.controllerListLabel = new System.Windows.Forms.Label();
            this.controllersList = new System.Windows.Forms.ListBox();
            this.groupBoxGameSettings = new System.Windows.Forms.GroupBox();
            this.groupAvailableActions = new System.Windows.Forms.GroupBox();
            this.autoExecuteEndMacro = new System.Windows.Forms.CheckBox();
            this.autoExecuteStartMacro = new System.Windows.Forms.CheckBox();
            this.lableModifierKeys = new System.Windows.Forms.Label();
            this.comboBoxModifierKeySelection = new System.Windows.Forms.ComboBox();
            this.radioButtonModifierAndKey = new System.Windows.Forms.RadioButton();
            this.textBoxSpecialActionParameter = new System.Windows.Forms.TextBox();
            this.buttonUndoLastAction = new System.Windows.Forms.Button();
            this.labelActionKeys = new System.Windows.Forms.Label();
            this.radioButtonAdvancedEditAction = new System.Windows.Forms.RadioButton();
            this.labelSpecialActionParameter = new System.Windows.Forms.Label();
            this.radioButtonMultipleFuelAction = new System.Windows.Forms.RadioButton();
            this.radioButtonWaitAction = new System.Windows.Forms.RadioButton();
            this.radioButtonMultipleVoiceTrigger = new System.Windows.Forms.RadioButton();
            this.radioButtonFreeTextAction = new System.Windows.Forms.RadioButton();
            this.radioButtonMultipleKeyAction = new System.Windows.Forms.RadioButton();
            this.radioButtonRegularKeyAction = new System.Windows.Forms.RadioButton();
            this.labelGameMacroDescription = new System.Windows.Forms.Label();
            this.textBoxGameMacroDescription = new System.Windows.Forms.TextBox();
            this.buttonLoadUserMacroSettings = new System.Windows.Forms.Button();
            this.buttonLoadDefaultMacroSettings = new System.Windows.Forms.Button();
            this.groupBoxGlobalOptins.SuspendLayout();
            this.groupBoxAvailableMacros.SuspendLayout();
            this.groupBoxGlobalMacroVoiceTrigger.SuspendLayout();
            this.groupBoxGameSettings.SuspendLayout();
            this.groupAvailableActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxGames
            // 
            this.listBoxGames.FormattingEnabled = true;
            this.listBoxGames.ItemHeight = 16;
            this.listBoxGames.Location = new System.Drawing.Point(12, 39);
            this.listBoxGames.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxGames.Name = "listBoxGames";
            this.listBoxGames.Size = new System.Drawing.Size(216, 244);
            this.listBoxGames.TabIndex = 0;
            this.listBoxGames.SelectedIndexChanged += new System.EventHandler(this.listBoxGames_SelectedIndexChanged);
            // 
            // comboBoxKeySelection
            // 
            this.comboBoxKeySelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeySelection.FormattingEnabled = true;
            this.comboBoxKeySelection.Location = new System.Drawing.Point(198, 47);
            this.comboBoxKeySelection.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxKeySelection.Name = "comboBoxKeySelection";
            this.comboBoxKeySelection.Size = new System.Drawing.Size(213, 24);
            this.comboBoxKeySelection.TabIndex = 7;
            // 
            // labelGame
            // 
            this.labelGame.AutoSize = true;
            this.labelGame.Location = new System.Drawing.Point(8, 20);
            this.labelGame.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGame.Name = "labelGame";
            this.labelGame.Size = new System.Drawing.Size(43, 17);
            this.labelGame.TabIndex = 4;
            this.labelGame.Text = "game";
            // 
            // listBoxAvailableMacros
            // 
            this.listBoxAvailableMacros.FormattingEnabled = true;
            this.listBoxAvailableMacros.ItemHeight = 16;
            this.listBoxAvailableMacros.Location = new System.Drawing.Point(8, 16);
            this.listBoxAvailableMacros.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxAvailableMacros.Name = "listBoxAvailableMacros";
            this.listBoxAvailableMacros.Size = new System.Drawing.Size(288, 116);
            this.listBoxAvailableMacros.TabIndex = 1;
            this.listBoxAvailableMacros.SelectedIndexChanged += new System.EventHandler(this.listBoxAvailableMacros_SelectedIndexChanged);
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(784, 39);
            this.textBoxDescription.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDescription.Size = new System.Drawing.Size(224, 244);
            this.textBoxDescription.TabIndex = 5;
            this.textBoxDescription.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxDescription_KeyPress);
            // 
            // labelGlobalMacroDescription
            // 
            this.labelGlobalMacroDescription.AutoSize = true;
            this.labelGlobalMacroDescription.Location = new System.Drawing.Point(780, 20);
            this.labelGlobalMacroDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGlobalMacroDescription.Name = "labelGlobalMacroDescription";
            this.labelGlobalMacroDescription.Size = new System.Drawing.Size(124, 17);
            this.labelGlobalMacroDescription.TabIndex = 9;
            this.labelGlobalMacroDescription.Text = "macro_description";
            // 
            // labelActionSequence
            // 
            this.labelActionSequence.AutoSize = true;
            this.labelActionSequence.Location = new System.Drawing.Point(432, 20);
            this.labelActionSequence.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelActionSequence.Name = "labelActionSequence";
            this.labelActionSequence.Size = new System.Drawing.Size(116, 17);
            this.labelActionSequence.TabIndex = 11;
            this.labelActionSequence.Text = "action_sequence";
            // 
            // textBoxActionSequence
            // 
            this.textBoxActionSequence.Location = new System.Drawing.Point(436, 39);
            this.textBoxActionSequence.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxActionSequence.Multiline = true;
            this.textBoxActionSequence.Name = "textBoxActionSequence";
            this.textBoxActionSequence.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxActionSequence.Size = new System.Drawing.Size(339, 201);
            this.textBoxActionSequence.TabIndex = 7;
            this.textBoxActionSequence.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxActionSequence_KeyPress);
            // 
            // textBoxVoiceTriggers
            // 
            this.textBoxVoiceTriggers.Location = new System.Drawing.Point(7, 87);
            this.textBoxVoiceTriggers.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxVoiceTriggers.Multiline = true;
            this.textBoxVoiceTriggers.Name = "textBoxVoiceTriggers";
            this.textBoxVoiceTriggers.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxVoiceTriggers.Size = new System.Drawing.Size(208, 69);
            this.textBoxVoiceTriggers.TabIndex = 10;
            // 
            // textBoxKeyPressTime
            // 
            this.textBoxKeyPressTime.Location = new System.Drawing.Point(436, 268);
            this.textBoxKeyPressTime.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxKeyPressTime.Name = "textBoxKeyPressTime";
            this.textBoxKeyPressTime.Size = new System.Drawing.Size(152, 22);
            this.textBoxKeyPressTime.TabIndex = 8;
            this.textBoxKeyPressTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyPressTime_KeyPress);
            // 
            // textBoxWaitBetweenEachCommand
            // 
            this.textBoxWaitBetweenEachCommand.Location = new System.Drawing.Point(611, 268);
            this.textBoxWaitBetweenEachCommand.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWaitBetweenEachCommand.Name = "textBoxWaitBetweenEachCommand";
            this.textBoxWaitBetweenEachCommand.Size = new System.Drawing.Size(164, 22);
            this.textBoxWaitBetweenEachCommand.TabIndex = 9;
            this.textBoxWaitBetweenEachCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxWaitBetweenEachCommand_KeyPress);
            // 
            // textBoxConfirmationMessage
            // 
            this.textBoxConfirmationMessage.Enabled = false;
            this.textBoxConfirmationMessage.Location = new System.Drawing.Point(552, 212);
            this.textBoxConfirmationMessage.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxConfirmationMessage.Name = "textBoxConfirmationMessage";
            this.textBoxConfirmationMessage.Size = new System.Drawing.Size(215, 22);
            this.textBoxConfirmationMessage.TabIndex = 19;
            this.textBoxConfirmationMessage.TabStop = false;
            // 
            // labelConfirmationMessage
            // 
            this.labelConfirmationMessage.AutoSize = true;
            this.labelConfirmationMessage.Location = new System.Drawing.Point(547, 192);
            this.labelConfirmationMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelConfirmationMessage.Name = "labelConfirmationMessage";
            this.labelConfirmationMessage.Size = new System.Drawing.Size(150, 17);
            this.labelConfirmationMessage.TabIndex = 20;
            this.labelConfirmationMessage.Text = "confirmation_message";
            // 
            // buttonAddSelectedKeyToSequence
            // 
            this.buttonAddSelectedKeyToSequence.Location = new System.Drawing.Point(196, 176);
            this.buttonAddSelectedKeyToSequence.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddSelectedKeyToSequence.Name = "buttonAddSelectedKeyToSequence";
            this.buttonAddSelectedKeyToSequence.Size = new System.Drawing.Size(215, 41);
            this.buttonAddSelectedKeyToSequence.TabIndex = 9;
            this.buttonAddSelectedKeyToSequence.Text = "add_key_to_sequence";
            this.buttonAddSelectedKeyToSequence.UseVisualStyleBackColor = true;
            this.buttonAddSelectedKeyToSequence.Click += new System.EventHandler(this.buttonAddSelectedActionToSequence_Click);
            // 
            // labelKeyPressTime
            // 
            this.labelKeyPressTime.AutoSize = true;
            this.labelKeyPressTime.Location = new System.Drawing.Point(432, 248);
            this.labelKeyPressTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelKeyPressTime.Name = "labelKeyPressTime";
            this.labelKeyPressTime.Size = new System.Drawing.Size(99, 17);
            this.labelKeyPressTime.TabIndex = 23;
            this.labelKeyPressTime.Text = "keypress_time";
            // 
            // labelWaitBetweenEachCommand
            // 
            this.labelWaitBetweenEachCommand.AutoSize = true;
            this.labelWaitBetweenEachCommand.Location = new System.Drawing.Point(607, 248);
            this.labelWaitBetweenEachCommand.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelWaitBetweenEachCommand.Name = "labelWaitBetweenEachCommand";
            this.labelWaitBetweenEachCommand.Size = new System.Drawing.Size(131, 17);
            this.labelWaitBetweenEachCommand.TabIndex = 24;
            this.labelWaitBetweenEachCommand.Text = "keypress_wait_time";
            // 
            // buttonSelectConfirmationMessage
            // 
            this.buttonSelectConfirmationMessage.Location = new System.Drawing.Point(552, 240);
            this.buttonSelectConfirmationMessage.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelectConfirmationMessage.Name = "buttonSelectConfirmationMessage";
            this.buttonSelectConfirmationMessage.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonSelectConfirmationMessage.Size = new System.Drawing.Size(216, 46);
            this.buttonSelectConfirmationMessage.TabIndex = 4;
            this.buttonSelectConfirmationMessage.Text = "select_confirmation_message";
            this.buttonSelectConfirmationMessage.UseVisualStyleBackColor = true;
            this.buttonSelectConfirmationMessage.Click += new System.EventHandler(this.buttonSelectConfirmationMessage_Click);
            // 
            // textBoxAddNewMacro
            // 
            this.textBoxAddNewMacro.Location = new System.Drawing.Point(8, 188);
            this.textBoxAddNewMacro.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxAddNewMacro.Name = "textBoxAddNewMacro";
            this.textBoxAddNewMacro.Size = new System.Drawing.Size(288, 22);
            this.textBoxAddNewMacro.TabIndex = 5;
            // 
            // buttonAddNewMacro
            // 
            this.buttonAddNewMacro.Location = new System.Drawing.Point(8, 217);
            this.buttonAddNewMacro.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddNewMacro.Name = "buttonAddNewMacro";
            this.buttonAddNewMacro.Size = new System.Drawing.Size(147, 38);
            this.buttonAddNewMacro.TabIndex = 6;
            this.buttonAddNewMacro.Text = "add_or_edit_macro";
            this.buttonAddNewMacro.UseVisualStyleBackColor = true;
            this.buttonAddNewMacro.Click += new System.EventHandler(this.buttonAddNewMacro_Click);
            // 
            // buttonAddActionSequence
            // 
            this.buttonAddActionSequence.Location = new System.Drawing.Point(432, 300);
            this.buttonAddActionSequence.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddActionSequence.Name = "buttonAddActionSequence";
            this.buttonAddActionSequence.Size = new System.Drawing.Size(345, 42);
            this.buttonAddActionSequence.TabIndex = 10;
            this.buttonAddActionSequence.Text = "add_action_sequece";
            this.buttonAddActionSequence.UseVisualStyleBackColor = true;
            this.buttonAddActionSequence.Click += new System.EventHandler(this.buttonAddActionSequence_Click);
            // 
            // groupBoxGlobalOptins
            // 
            this.groupBoxGlobalOptins.Controls.Add(this.deleteAssignmentButton);
            this.groupBoxGlobalOptins.Controls.Add(this.addAssignmentButton);
            this.groupBoxGlobalOptins.Controls.Add(this.currentAssignmentLabel);
            this.groupBoxGlobalOptins.Controls.Add(this.groupBoxAvailableMacros);
            this.groupBoxGlobalOptins.Controls.Add(this.groupBoxGlobalMacroVoiceTrigger);
            this.groupBoxGlobalOptins.Controls.Add(this.controllerListLabel);
            this.groupBoxGlobalOptins.Controls.Add(this.listBoxGames);
            this.groupBoxGlobalOptins.Controls.Add(this.textBoxDescription);
            this.groupBoxGlobalOptins.Controls.Add(this.buttonSelectConfirmationMessage);
            this.groupBoxGlobalOptins.Controls.Add(this.labelGame);
            this.groupBoxGlobalOptins.Controls.Add(this.labelConfirmationMessage);
            this.groupBoxGlobalOptins.Controls.Add(this.labelGlobalMacroDescription);
            this.groupBoxGlobalOptins.Controls.Add(this.controllersList);
            this.groupBoxGlobalOptins.Controls.Add(this.textBoxConfirmationMessage);
            this.groupBoxGlobalOptins.Location = new System.Drawing.Point(12, 14);
            this.groupBoxGlobalOptins.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxGlobalOptins.Name = "groupBoxGlobalOptins";
            this.groupBoxGlobalOptins.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxGlobalOptins.Size = new System.Drawing.Size(1363, 302);
            this.groupBoxGlobalOptins.TabIndex = 30;
            this.groupBoxGlobalOptins.TabStop = false;
            this.groupBoxGlobalOptins.Text = "global_macro_settings";
            // 
            // deleteAssignmentButton
            // 
            this.deleteAssignmentButton.Location = new System.Drawing.Point(1025, 245);
            this.deleteAssignmentButton.Margin = new System.Windows.Forms.Padding(4);
            this.deleteAssignmentButton.Name = "deleteAssignmentButton";
            this.deleteAssignmentButton.Size = new System.Drawing.Size(232, 41);
            this.deleteAssignmentButton.TabIndex = 213;
            this.deleteAssignmentButton.Text = "delete_assignment";
            this.deleteAssignmentButton.UseVisualStyleBackColor = true;
            this.deleteAssignmentButton.Click += new System.EventHandler(this.deleteAssignment_Click);
            // 
            // addAssignmentButton
            // 
            this.addAssignmentButton.Location = new System.Drawing.Point(1025, 192);
            this.addAssignmentButton.Margin = new System.Windows.Forms.Padding(4);
            this.addAssignmentButton.Name = "addAssignmentButton";
            this.addAssignmentButton.Size = new System.Drawing.Size(232, 39);
            this.addAssignmentButton.TabIndex = 215;
            this.addAssignmentButton.Text = "add_assignment";
            this.addAssignmentButton.UseVisualStyleBackColor = true;
            this.addAssignmentButton.Click += new System.EventHandler(this.addAssignment_Click);
            // 
            // currentAssignmentLabel
            // 
            this.currentAssignmentLabel.AutoSize = true;
            this.currentAssignmentLabel.Location = new System.Drawing.Point(1021, 143);
            this.currentAssignmentLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.currentAssignmentLabel.MaximumSize = new System.Drawing.Size(321, 37);
            this.currentAssignmentLabel.Name = "currentAssignmentLabel";
            this.currentAssignmentLabel.Size = new System.Drawing.Size(171, 17);
            this.currentAssignmentLabel.TabIndex = 216;
            this.currentAssignmentLabel.Text = "current_assignment_label";
            // 
            // groupBoxAvailableMacros
            // 
            this.groupBoxAvailableMacros.Controls.Add(this.labelMacroEditMode);
            this.groupBoxAvailableMacros.Controls.Add(this.buttonDeleteSelectedMacro);
            this.groupBoxAvailableMacros.Controls.Add(this.radioButtonAddNewMacro);
            this.groupBoxAvailableMacros.Controls.Add(this.radioButtonEditSelectedMacro);
            this.groupBoxAvailableMacros.Controls.Add(this.radioButtonViewOnly);
            this.groupBoxAvailableMacros.Controls.Add(this.listBoxAvailableMacros);
            this.groupBoxAvailableMacros.Controls.Add(this.textBoxAddNewMacro);
            this.groupBoxAvailableMacros.Controls.Add(this.buttonAddNewMacro);
            this.groupBoxAvailableMacros.Location = new System.Drawing.Point(237, 23);
            this.groupBoxAvailableMacros.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxAvailableMacros.Name = "groupBoxAvailableMacros";
            this.groupBoxAvailableMacros.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxAvailableMacros.Size = new System.Drawing.Size(305, 262);
            this.groupBoxAvailableMacros.TabIndex = 2;
            this.groupBoxAvailableMacros.TabStop = false;
            this.groupBoxAvailableMacros.Text = "available_macros";
            // 
            // labelMacroEditMode
            // 
            this.labelMacroEditMode.AutoSize = true;
            this.labelMacroEditMode.Location = new System.Drawing.Point(9, 142);
            this.labelMacroEditMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMacroEditMode.Name = "labelMacroEditMode";
            this.labelMacroEditMode.Size = new System.Drawing.Size(121, 17);
            this.labelMacroEditMode.TabIndex = 34;
            this.labelMacroEditMode.Text = "macro_edit_mode";
            // 
            // buttonDeleteSelectedMacro
            // 
            this.buttonDeleteSelectedMacro.Location = new System.Drawing.Point(163, 217);
            this.buttonDeleteSelectedMacro.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDeleteSelectedMacro.Name = "buttonDeleteSelectedMacro";
            this.buttonDeleteSelectedMacro.Size = new System.Drawing.Size(135, 38);
            this.buttonDeleteSelectedMacro.TabIndex = 7;
            this.buttonDeleteSelectedMacro.Text = "delete_macro";
            this.buttonDeleteSelectedMacro.UseVisualStyleBackColor = true;
            this.buttonDeleteSelectedMacro.Click += new System.EventHandler(this.buttonDeleteSelectedMacro_Click);
            // 
            // radioButtonAddNewMacro
            // 
            this.radioButtonAddNewMacro.AutoSize = true;
            this.radioButtonAddNewMacro.Location = new System.Drawing.Point(199, 160);
            this.radioButtonAddNewMacro.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonAddNewMacro.Name = "radioButtonAddNewMacro";
            this.radioButtonAddNewMacro.Size = new System.Drawing.Size(149, 21);
            this.radioButtonAddNewMacro.TabIndex = 4;
            this.radioButtonAddNewMacro.TabStop = true;
            this.radioButtonAddNewMacro.Text = "create_new_macro";
            this.radioButtonAddNewMacro.UseVisualStyleBackColor = true;
            this.radioButtonAddNewMacro.CheckedChanged += new System.EventHandler(this.radioButtonAvailableMacros_CheckedChanged);
            // 
            // radioButtonEditSelectedMacro
            // 
            this.radioButtonEditSelectedMacro.AutoSize = true;
            this.radioButtonEditSelectedMacro.Location = new System.Drawing.Point(105, 160);
            this.radioButtonEditSelectedMacro.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonEditSelectedMacro.Name = "radioButtonEditSelectedMacro";
            this.radioButtonEditSelectedMacro.Size = new System.Drawing.Size(160, 21);
            this.radioButtonEditSelectedMacro.TabIndex = 3;
            this.radioButtonEditSelectedMacro.TabStop = true;
            this.radioButtonEditSelectedMacro.Text = "edit_selected_macro";
            this.radioButtonEditSelectedMacro.UseVisualStyleBackColor = true;
            this.radioButtonEditSelectedMacro.CheckedChanged += new System.EventHandler(this.radioButtonAvailableMacros_CheckedChanged);
            // 
            // radioButtonViewOnly
            // 
            this.radioButtonViewOnly.AutoSize = true;
            this.radioButtonViewOnly.Location = new System.Drawing.Point(8, 160);
            this.radioButtonViewOnly.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonViewOnly.Name = "radioButtonViewOnly";
            this.radioButtonViewOnly.Size = new System.Drawing.Size(137, 21);
            this.radioButtonViewOnly.TabIndex = 2;
            this.radioButtonViewOnly.TabStop = true;
            this.radioButtonViewOnly.Text = "macro_view_only";
            this.radioButtonViewOnly.UseVisualStyleBackColor = true;
            this.radioButtonViewOnly.CheckedChanged += new System.EventHandler(this.radioButtonAvailableMacros_CheckedChanged);
            // 
            // groupBoxGlobalMacroVoiceTrigger
            // 
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.radioButtonRegularVoiceTrigger);
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.radioButtonIntegerVoiceTrigger);
            this.groupBoxGlobalMacroVoiceTrigger.Controls.Add(this.textBoxVoiceTriggers);
            this.groupBoxGlobalMacroVoiceTrigger.Location = new System.Drawing.Point(552, 23);
            this.groupBoxGlobalMacroVoiceTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxGlobalMacroVoiceTrigger.Name = "groupBoxGlobalMacroVoiceTrigger";
            this.groupBoxGlobalMacroVoiceTrigger.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxGlobalMacroVoiceTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBoxGlobalMacroVoiceTrigger.Size = new System.Drawing.Size(224, 165);
            this.groupBoxGlobalMacroVoiceTrigger.TabIndex = 3;
            this.groupBoxGlobalMacroVoiceTrigger.TabStop = false;
            this.groupBoxGlobalMacroVoiceTrigger.Text = "macro_voice_trigger";
            // 
            // radioButtonRegularVoiceTrigger
            // 
            this.radioButtonRegularVoiceTrigger.AutoSize = true;
            this.radioButtonRegularVoiceTrigger.Location = new System.Drawing.Point(8, 23);
            this.radioButtonRegularVoiceTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonRegularVoiceTrigger.Name = "radioButtonRegularVoiceTrigger";
            this.radioButtonRegularVoiceTrigger.Size = new System.Drawing.Size(231, 21);
            this.radioButtonRegularVoiceTrigger.TabIndex = 0;
            this.radioButtonRegularVoiceTrigger.TabStop = true;
            this.radioButtonRegularVoiceTrigger.Text = "regular_macro_voice_command";
            this.radioButtonRegularVoiceTrigger.UseVisualStyleBackColor = true;
            // 
            // radioButtonIntegerVoiceTrigger
            // 
            this.radioButtonIntegerVoiceTrigger.AutoSize = true;
            this.radioButtonIntegerVoiceTrigger.Location = new System.Drawing.Point(8, 52);
            this.radioButtonIntegerVoiceTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonIntegerVoiceTrigger.Name = "radioButtonIntegerVoiceTrigger";
            this.radioButtonIntegerVoiceTrigger.Size = new System.Drawing.Size(230, 21);
            this.radioButtonIntegerVoiceTrigger.TabIndex = 9;
            this.radioButtonIntegerVoiceTrigger.TabStop = true;
            this.radioButtonIntegerVoiceTrigger.Text = "integer_macro_voice_command";
            this.radioButtonIntegerVoiceTrigger.UseVisualStyleBackColor = true;
            // 
            // controllerListLabel
            // 
            this.controllerListLabel.AutoSize = true;
            this.controllerListLabel.Location = new System.Drawing.Point(1021, 20);
            this.controllerListLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.controllerListLabel.Name = "controllerListLabel";
            this.controllerListLabel.Size = new System.Drawing.Size(74, 17);
            this.controllerListLabel.TabIndex = 214;
            this.controllerListLabel.Text = "controllers";
            // 
            // controllersList
            // 
            this.controllersList.FormattingEnabled = true;
            this.controllersList.ItemHeight = 16;
            this.controllersList.Location = new System.Drawing.Point(1025, 39);
            this.controllersList.Margin = new System.Windows.Forms.Padding(4);
            this.controllersList.Name = "controllersList";
            this.controllersList.Size = new System.Drawing.Size(320, 100);
            this.controllersList.TabIndex = 210;
            this.controllersList.SelectedIndexChanged += new System.EventHandler(this.controllersList_SelectedIndexChanged);
            // 
            // groupBoxGameSettings
            // 
            this.groupBoxGameSettings.Controls.Add(this.groupAvailableActions);
            this.groupBoxGameSettings.Controls.Add(this.labelGameMacroDescription);
            this.groupBoxGameSettings.Controls.Add(this.textBoxGameMacroDescription);
            this.groupBoxGameSettings.Controls.Add(this.textBoxActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.buttonAddActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.labelActionSequence);
            this.groupBoxGameSettings.Controls.Add(this.labelWaitBetweenEachCommand);
            this.groupBoxGameSettings.Controls.Add(this.textBoxWaitBetweenEachCommand);
            this.groupBoxGameSettings.Controls.Add(this.labelKeyPressTime);
            this.groupBoxGameSettings.Controls.Add(this.textBoxKeyPressTime);
            this.groupBoxGameSettings.Location = new System.Drawing.Point(12, 322);
            this.groupBoxGameSettings.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxGameSettings.Name = "groupBoxGameSettings";
            this.groupBoxGameSettings.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxGameSettings.Size = new System.Drawing.Size(1017, 359);
            this.groupBoxGameSettings.TabIndex = 31;
            this.groupBoxGameSettings.TabStop = false;
            this.groupBoxGameSettings.Text = "game_specific_settings";
            // 
            // groupAvailableActions
            // 
            this.groupAvailableActions.Controls.Add(this.autoExecuteEndMacro);
            this.groupAvailableActions.Controls.Add(this.lableModifierKeys);
            this.groupAvailableActions.Controls.Add(this.comboBoxModifierKeySelection);
            this.groupAvailableActions.Controls.Add(this.autoExecuteStartMacro);
            this.groupAvailableActions.Controls.Add(this.radioButtonModifierAndKey);
            this.groupAvailableActions.Controls.Add(this.textBoxSpecialActionParameter);
            this.groupAvailableActions.Controls.Add(this.buttonUndoLastAction);
            this.groupAvailableActions.Controls.Add(this.labelActionKeys);
            this.groupAvailableActions.Controls.Add(this.radioButtonFreeTextAction);
            this.groupAvailableActions.Controls.Add(this.radioButtonAdvancedEditAction);
            this.groupAvailableActions.Controls.Add(this.labelSpecialActionParameter);
            this.groupAvailableActions.Controls.Add(this.radioButtonMultipleFuelAction);
            this.groupAvailableActions.Controls.Add(this.radioButtonWaitAction);
            this.groupAvailableActions.Controls.Add(this.radioButtonMultipleVoiceTrigger);
            this.groupAvailableActions.Controls.Add(this.buttonAddSelectedKeyToSequence);
            this.groupAvailableActions.Controls.Add(this.radioButtonMultipleKeyAction);
            this.groupAvailableActions.Controls.Add(this.radioButtonRegularKeyAction);
            this.groupAvailableActions.Controls.Add(this.comboBoxKeySelection);
            this.groupAvailableActions.Location = new System.Drawing.Point(5, 23);
            this.groupAvailableActions.Margin = new System.Windows.Forms.Padding(4);
            this.groupAvailableActions.Name = "groupAvailableActions";
            this.groupAvailableActions.Padding = new System.Windows.Forms.Padding(4);
            this.groupAvailableActions.Size = new System.Drawing.Size(419, 328);
            this.groupAvailableActions.TabIndex = 6;
            this.groupAvailableActions.TabStop = false;
            this.groupAvailableActions.Text = "available_actions";
            // 
            // autoExecuteEndMacro
            // 
            this.autoExecuteEndMacro.AutoSize = true;
            this.autoExecuteEndMacro.Location = new System.Drawing.Point(8, 290);
            this.autoExecuteEndMacro.Name = "autoExecuteEndMacro";
            this.autoExecuteEndMacro.Size = new System.Drawing.Size(182, 21);
            this.autoExecuteEndMacro.TabIndex = 32;
            this.autoExecuteEndMacro.Text = "auto_execute_end_chat";
            this.autoExecuteEndMacro.UseVisualStyleBackColor = true;
            // 
            // autoExecuteStartMacro
            // 
            this.autoExecuteStartMacro.AutoSize = true;
            this.autoExecuteStartMacro.Location = new System.Drawing.Point(8, 263);
            this.autoExecuteStartMacro.Name = "autoExecuteStartMacro";
            this.autoExecuteStartMacro.Size = new System.Drawing.Size(186, 21);
            this.autoExecuteStartMacro.TabIndex = 29;
            this.autoExecuteStartMacro.Text = "auto_execute_start_chat";
            this.autoExecuteStartMacro.UseVisualStyleBackColor = true;
            // 
            // lableModifierKeys
            // 
            this.lableModifierKeys.AutoSize = true;
            this.lableModifierKeys.Location = new System.Drawing.Point(195, 75);
            this.lableModifierKeys.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lableModifierKeys.Name = "lableModifierKeys";
            this.lableModifierKeys.Size = new System.Drawing.Size(95, 17);
            this.lableModifierKeys.TabIndex = 28;
            this.lableModifierKeys.Text = "modifier_keys";
            this.lableModifierKeys.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // comboBoxModifierKeySelection
            // 
            this.comboBoxModifierKeySelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxModifierKeySelection.FormattingEnabled = true;
            this.comboBoxModifierKeySelection.Location = new System.Drawing.Point(198, 96);
            this.comboBoxModifierKeySelection.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxModifierKeySelection.Name = "comboBoxModifierKeySelection";
            this.comboBoxModifierKeySelection.Size = new System.Drawing.Size(213, 24);
            this.comboBoxModifierKeySelection.TabIndex = 27;
            // 
            // radioButtonModifierAndKey
            // 
            this.radioButtonModifierAndKey.AutoSize = true;
            this.radioButtonModifierAndKey.Location = new System.Drawing.Point(9, 166);
            this.radioButtonModifierAndKey.Name = "radioButtonModifierAndKey";
            this.radioButtonModifierAndKey.Size = new System.Drawing.Size(141, 21);
            this.radioButtonModifierAndKey.TabIndex = 26;
            this.radioButtonModifierAndKey.TabStop = true;
            this.radioButtonModifierAndKey.Text = "modifier_and_key";
            this.radioButtonModifierAndKey.UseVisualStyleBackColor = true;
            // 
            // textBoxSpecialActionParameter
            // 
            this.textBoxSpecialActionParameter.Location = new System.Drawing.Point(197, 144);
            this.textBoxSpecialActionParameter.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSpecialActionParameter.Name = "textBoxSpecialActionParameter";
            this.textBoxSpecialActionParameter.Size = new System.Drawing.Size(213, 22);
            this.textBoxSpecialActionParameter.TabIndex = 8;
            this.textBoxSpecialActionParameter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxSpecialActionParameter_KeyPress);
            // 
            // buttonUndoLastAction
            // 
            this.buttonUndoLastAction.Location = new System.Drawing.Point(196, 225);
            this.buttonUndoLastAction.Margin = new System.Windows.Forms.Padding(4);
            this.buttonUndoLastAction.Name = "buttonUndoLastAction";
            this.buttonUndoLastAction.Size = new System.Drawing.Size(215, 41);
            this.buttonUndoLastAction.TabIndex = 10;
            this.buttonUndoLastAction.Text = "undo_last_action";
            this.buttonUndoLastAction.UseVisualStyleBackColor = true;
            this.buttonUndoLastAction.Click += new System.EventHandler(this.buttonUndoLastAction_Click);
            // 
            // labelActionKeys
            // 
            this.labelActionKeys.AutoSize = true;
            this.labelActionKeys.Location = new System.Drawing.Point(195, 27);
            this.labelActionKeys.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelActionKeys.Name = "labelActionKeys";
            this.labelActionKeys.Size = new System.Drawing.Size(83, 17);
            this.labelActionKeys.TabIndex = 25;
            this.labelActionKeys.Text = "action_keys";
            // 
            // radioButtonAdvancedEditAction
            // 
            this.radioButtonAdvancedEditAction.ForeColor = System.Drawing.SystemColors.ControlText;
            this.radioButtonAdvancedEditAction.Location = new System.Drawing.Point(9, 192);
            this.radioButtonAdvancedEditAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonAdvancedEditAction.Name = "radioButtonAdvancedEditAction";
            this.radioButtonAdvancedEditAction.Size = new System.Drawing.Size(157, 42);
            this.radioButtonAdvancedEditAction.TabIndex = 6;
            this.radioButtonAdvancedEditAction.TabStop = true;
            this.radioButtonAdvancedEditAction.Text = "advanced_edit_action";
            this.radioButtonAdvancedEditAction.UseVisualStyleBackColor = true;
            // 
            // labelSpecialActionParameter
            // 
            this.labelSpecialActionParameter.AutoSize = true;
            this.labelSpecialActionParameter.Location = new System.Drawing.Point(195, 124);
            this.labelSpecialActionParameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSpecialActionParameter.Name = "labelSpecialActionParameter";
            this.labelSpecialActionParameter.Size = new System.Drawing.Size(171, 17);
            this.labelSpecialActionParameter.TabIndex = 23;
            this.labelSpecialActionParameter.Text = "special_action_parameter";
            // 
            // radioButtonMultipleFuelAction
            // 
            this.radioButtonMultipleFuelAction.AutoSize = true;
            this.radioButtonMultipleFuelAction.Location = new System.Drawing.Point(9, 110);
            this.radioButtonMultipleFuelAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonMultipleFuelAction.Name = "radioButtonMultipleFuelAction";
            this.radioButtonMultipleFuelAction.Size = new System.Drawing.Size(154, 21);
            this.radioButtonMultipleFuelAction.TabIndex = 3;
            this.radioButtonMultipleFuelAction.TabStop = true;
            this.radioButtonMultipleFuelAction.Text = "multiple_fuel_action";
            this.radioButtonMultipleFuelAction.UseVisualStyleBackColor = true;
            this.radioButtonMultipleFuelAction.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // radioButtonWaitAction
            // 
            this.radioButtonWaitAction.AutoSize = true;
            this.radioButtonWaitAction.Location = new System.Drawing.Point(9, 138);
            this.radioButtonWaitAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonWaitAction.Name = "radioButtonWaitAction";
            this.radioButtonWaitAction.Size = new System.Drawing.Size(99, 21);
            this.radioButtonWaitAction.TabIndex = 4;
            this.radioButtonWaitAction.TabStop = true;
            this.radioButtonWaitAction.Text = "wait_action";
            this.radioButtonWaitAction.UseVisualStyleBackColor = true;
            this.radioButtonWaitAction.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // radioButtonMultipleVoiceTrigger
            // 
            this.radioButtonMultipleVoiceTrigger.AutoSize = true;
            this.radioButtonMultipleVoiceTrigger.Location = new System.Drawing.Point(9, 81);
            this.radioButtonMultipleVoiceTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonMultipleVoiceTrigger.Name = "radioButtonMultipleVoiceTrigger";
            this.radioButtonMultipleVoiceTrigger.Size = new System.Drawing.Size(157, 21);
            this.radioButtonMultipleVoiceTrigger.TabIndex = 2;
            this.radioButtonMultipleVoiceTrigger.TabStop = true;
            this.radioButtonMultipleVoiceTrigger.Text = "voice_trigger_action";
            this.radioButtonMultipleVoiceTrigger.UseVisualStyleBackColor = true;
            this.radioButtonMultipleVoiceTrigger.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // radioButtonFreeTextAction
            // 
            this.radioButtonFreeTextAction.AutoSize = true;
            this.radioButtonFreeTextAction.Location = new System.Drawing.Point(8, 235);
            this.radioButtonFreeTextAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonFreeTextAction.Name = "radioButtonFreeTextAction";
            this.radioButtonFreeTextAction.Size = new System.Drawing.Size(130, 21);
            this.radioButtonFreeTextAction.TabIndex = 5;
            this.radioButtonFreeTextAction.TabStop = true;
            this.radioButtonFreeTextAction.Text = "free_text_action";
            this.radioButtonFreeTextAction.UseVisualStyleBackColor = true;
            this.radioButtonFreeTextAction.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // radioButtonMultipleKeyAction
            // 
            this.radioButtonMultipleKeyAction.AutoSize = true;
            this.radioButtonMultipleKeyAction.Location = new System.Drawing.Point(9, 53);
            this.radioButtonMultipleKeyAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonMultipleKeyAction.Name = "radioButtonMultipleKeyAction";
            this.radioButtonMultipleKeyAction.Size = new System.Drawing.Size(153, 21);
            this.radioButtonMultipleKeyAction.TabIndex = 1;
            this.radioButtonMultipleKeyAction.TabStop = true;
            this.radioButtonMultipleKeyAction.Text = "multiple_key_action";
            this.radioButtonMultipleKeyAction.UseVisualStyleBackColor = true;
            this.radioButtonMultipleKeyAction.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // radioButtonRegularKeyAction
            // 
            this.radioButtonRegularKeyAction.AutoSize = true;
            this.radioButtonRegularKeyAction.Location = new System.Drawing.Point(9, 25);
            this.radioButtonRegularKeyAction.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonRegularKeyAction.Name = "radioButtonRegularKeyAction";
            this.radioButtonRegularKeyAction.Size = new System.Drawing.Size(150, 21);
            this.radioButtonRegularKeyAction.TabIndex = 0;
            this.radioButtonRegularKeyAction.TabStop = true;
            this.radioButtonRegularKeyAction.Text = "regular_key_action";
            this.radioButtonRegularKeyAction.UseVisualStyleBackColor = true;
            this.radioButtonRegularKeyAction.CheckedChanged += new System.EventHandler(this.radioButtonAvailableActions_CheckedChanged);
            // 
            // labelGameMacroDescription
            // 
            this.labelGameMacroDescription.AutoSize = true;
            this.labelGameMacroDescription.Location = new System.Drawing.Point(781, 20);
            this.labelGameMacroDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelGameMacroDescription.Name = "labelGameMacroDescription";
            this.labelGameMacroDescription.Size = new System.Drawing.Size(124, 17);
            this.labelGameMacroDescription.TabIndex = 31;
            this.labelGameMacroDescription.Text = "macro_description";
            // 
            // textBoxGameMacroDescription
            // 
            this.textBoxGameMacroDescription.Location = new System.Drawing.Point(784, 39);
            this.textBoxGameMacroDescription.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxGameMacroDescription.Multiline = true;
            this.textBoxGameMacroDescription.Name = "textBoxGameMacroDescription";
            this.textBoxGameMacroDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxGameMacroDescription.Size = new System.Drawing.Size(223, 312);
            this.textBoxGameMacroDescription.TabIndex = 11;
            // 
            // buttonLoadUserMacroSettings
            // 
            this.buttonLoadUserMacroSettings.Location = new System.Drawing.Point(12, 689);
            this.buttonLoadUserMacroSettings.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLoadUserMacroSettings.Name = "buttonLoadUserMacroSettings";
            this.buttonLoadUserMacroSettings.Size = new System.Drawing.Size(213, 43);
            this.buttonLoadUserMacroSettings.TabIndex = 12;
            this.buttonLoadUserMacroSettings.Text = "load_user_macro_settings";
            this.buttonLoadUserMacroSettings.UseVisualStyleBackColor = true;
            this.buttonLoadUserMacroSettings.Click += new System.EventHandler(this.buttonLoadUserMacroSettings_Click);
            // 
            // buttonLoadDefaultMacroSettings
            // 
            this.buttonLoadDefaultMacroSettings.Location = new System.Drawing.Point(233, 689);
            this.buttonLoadDefaultMacroSettings.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLoadDefaultMacroSettings.Name = "buttonLoadDefaultMacroSettings";
            this.buttonLoadDefaultMacroSettings.Size = new System.Drawing.Size(207, 43);
            this.buttonLoadDefaultMacroSettings.TabIndex = 13;
            this.buttonLoadDefaultMacroSettings.Text = "load_default_macro_settings";
            this.buttonLoadDefaultMacroSettings.UseVisualStyleBackColor = true;
            this.buttonLoadDefaultMacroSettings.Click += new System.EventHandler(this.buttonLoadDefaultMacroSettings_Click);
            // 
            // MacroEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1388, 745);
            this.Controls.Add(this.buttonLoadDefaultMacroSettings);
            this.Controls.Add(this.buttonLoadUserMacroSettings);
            this.Controls.Add(this.groupBoxGameSettings);
            this.Controls.Add(this.groupBoxGlobalOptins);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MacroEditor";
            this.Text = "Command Macro Editor";
            this.Load += new System.EventHandler(this.MacroEditor_Load);
            this.groupBoxGlobalOptins.ResumeLayout(false);
            this.groupBoxGlobalOptins.PerformLayout();
            this.groupBoxAvailableMacros.ResumeLayout(false);
            this.groupBoxAvailableMacros.PerformLayout();
            this.groupBoxGlobalMacroVoiceTrigger.ResumeLayout(false);
            this.groupBoxGlobalMacroVoiceTrigger.PerformLayout();
            this.groupBoxGameSettings.ResumeLayout(false);
            this.groupBoxGameSettings.PerformLayout();
            this.groupAvailableActions.ResumeLayout(false);
            this.groupAvailableActions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxGames;
        private System.Windows.Forms.ComboBox comboBoxKeySelection;
        private System.Windows.Forms.Label labelGame;
        private System.Windows.Forms.ListBox listBoxAvailableMacros;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelGlobalMacroDescription;
        private System.Windows.Forms.Label labelActionSequence;
        private System.Windows.Forms.TextBox textBoxActionSequence;
        private System.Windows.Forms.TextBox textBoxVoiceTriggers;
        private System.Windows.Forms.TextBox textBoxKeyPressTime;
        private System.Windows.Forms.TextBox textBoxWaitBetweenEachCommand;
        private System.Windows.Forms.TextBox textBoxConfirmationMessage;
        private System.Windows.Forms.Label labelConfirmationMessage;
        private System.Windows.Forms.Button buttonAddSelectedKeyToSequence;
        private System.Windows.Forms.Label labelKeyPressTime;
        private System.Windows.Forms.Label labelWaitBetweenEachCommand;
        private System.Windows.Forms.Button buttonSelectConfirmationMessage;
        private System.Windows.Forms.TextBox textBoxAddNewMacro;
        private System.Windows.Forms.Button buttonAddNewMacro;
        private System.Windows.Forms.Button buttonAddActionSequence;
        private System.Windows.Forms.GroupBox groupBoxGlobalOptins;
        private System.Windows.Forms.GroupBox groupBoxGameSettings;
        private System.Windows.Forms.Button buttonLoadUserMacroSettings;
        private System.Windows.Forms.Button buttonLoadDefaultMacroSettings;
        private System.Windows.Forms.GroupBox groupBoxGlobalMacroVoiceTrigger;
        private System.Windows.Forms.RadioButton radioButtonRegularVoiceTrigger;
        private System.Windows.Forms.RadioButton radioButtonIntegerVoiceTrigger;
        private System.Windows.Forms.TextBox textBoxGameMacroDescription;
        private System.Windows.Forms.Label labelGameMacroDescription;
        private System.Windows.Forms.GroupBox groupAvailableActions;
        private System.Windows.Forms.RadioButton radioButtonFreeTextAction;
        private System.Windows.Forms.RadioButton radioButtonMultipleKeyAction;
        private System.Windows.Forms.RadioButton radioButtonRegularKeyAction;
        private System.Windows.Forms.RadioButton radioButtonMultipleVoiceTrigger;
        private System.Windows.Forms.RadioButton radioButtonWaitAction;
        private System.Windows.Forms.RadioButton radioButtonMultipleFuelAction;
        private System.Windows.Forms.Label labelSpecialActionParameter;
        private System.Windows.Forms.RadioButton radioButtonAdvancedEditAction;
        private System.Windows.Forms.Label labelActionKeys;
        private System.Windows.Forms.GroupBox groupBoxAvailableMacros;
        private System.Windows.Forms.RadioButton radioButtonAddNewMacro;
        private System.Windows.Forms.RadioButton radioButtonEditSelectedMacro;
        private System.Windows.Forms.RadioButton radioButtonViewOnly;
        private System.Windows.Forms.Button buttonUndoLastAction;
        private System.Windows.Forms.TextBox textBoxSpecialActionParameter;
        private System.Windows.Forms.Button buttonDeleteSelectedMacro;
        private System.Windows.Forms.Label labelMacroEditMode;
        private System.Windows.Forms.Button deleteAssignmentButton;
        private System.Windows.Forms.Label controllerListLabel;
        private System.Windows.Forms.Button addAssignmentButton;
        private System.Windows.Forms.ListBox controllersList;
        private System.Windows.Forms.Label currentAssignmentLabel;
        private System.Windows.Forms.RadioButton radioButtonModifierAndKey;
        private System.Windows.Forms.Label lableModifierKeys;
        private System.Windows.Forms.ComboBox comboBoxModifierKeySelection;
        private System.Windows.Forms.CheckBox autoExecuteEndMacro;
        private System.Windows.Forms.CheckBox autoExecuteStartMacro;
    }
}