﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrewChiefV4.commands;
using System.IO;
using System.Threading;
using static CrewChiefV4.ControllerConfiguration;

namespace CrewChiefV4
{

    public partial class MacroEditor : Form
    {
        public static List<String> builtInKeyMappings = new List<String>();
        MacroContainer macroContainer = null;
        List<GameDefinition> availableMacroGames = null;
        ControllerConfiguration controllerConfiguration;

        private Thread assignButtonThread = null;
        bool isAssigningButton = false;

        public MacroEditor(Form parent, ControllerConfiguration controllerConfiguration)
        {
            this.controllerConfiguration = controllerConfiguration;
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.SuspendLayout();
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            //Global
            groupBoxGlobalOptins.Text = Configuration.getUIString("global_macro_settings");
            labelGame.Text = Configuration.getUIString("game");

            groupBoxAvailableMacros.Text = Configuration.getUIString("available_macros");
            buttonAddNewMacro.Text = Configuration.getUIString("add_or_edit_macro");
            buttonDeleteSelectedMacro.Text = Configuration.getUIString("delete_selected_macro");
            radioButtonViewOnly.Text = Configuration.getUIString("macro_view_only");
            radioButtonEditSelectedMacro.Text = Configuration.getUIString("edit_selected_macro");
            radioButtonAddNewMacro.Text = Configuration.getUIString("create_new_macro");

            groupBoxGlobalMacroVoiceTrigger.Text = Configuration.getUIString("macro_voice_trigger");
            radioButtonRegularVoiceTrigger.Text = Configuration.getUIString("regular_macro_voice_command");
            radioButtonIntegerVoiceTrigger.Text = Configuration.getUIString("integer_macro_voice_command");
            labelMacroEditMode.Text = Configuration.getUIString("macro_edit_mode");

            labelConfirmationMessage.Text = Configuration.getUIString("confirmation_message");
            buttonSelectConfirmationMessage.Text = Configuration.getUIString("select_confirmation_message");
            labelGlobalMacroDescription.Text = Configuration.getUIString("macro_description");

            //Game Specific
            groupBoxGameSettings.Text = Configuration.getUIString("game_specific_settings");

            labelActionSequence.Text = Configuration.getUIString("action_sequence");
            labelKeyPressTime.Text = Configuration.getUIString("keypress_time");
            labelWaitBetweenEachCommand.Text = Configuration.getUIString("keypress_wait_time");
            buttonAddActionSequence.Text = Configuration.getUIString("add_action_sequence");


            labelGameMacroDescription.Text = Configuration.getUIString("macro_description");


            groupAvailableActions.Text = Configuration.getUIString("available_actions");
            radioButtonRegularKeyAction.Text = Configuration.getUIString("regular_key_action");
            radioButtonMultipleKeyAction.Text = Configuration.getUIString("multiple_key_action");
            radioButtonMultipleVoiceTrigger.Text = Configuration.getUIString("voice_trigger_action");
            radioButtonMultipleFuelAction.Text = Configuration.getUIString("multiple_fuel_action");
            radioButtonWaitAction.Text = Configuration.getUIString("wait_action");
            radioButtonFreeTextAction.Text = Configuration.getUIString("free_text_action");
            radioButtonAdvancedEditAction.Text = Configuration.getUIString("advanced_edit_action");
            buttonAddSelectedKeyToSequence.Text = Configuration.getUIString("add_key_to_sequence");
            buttonUndoLastAction.Text = Configuration.getUIString("undo_last_action");
            labelActionKeys.Text = Configuration.getUIString("actions_keys");
            // Load
            buttonLoadUserMacroSettings.Text = Configuration.getUIString("load_user_macro_settings");
            buttonLoadDefaultMacroSettings.Text = Configuration.getUIString("load_default_macro_settings");

            deleteAssignmentButton.Text = Configuration.getUIString("delete_assignment");
            addAssignmentButton.Text = Configuration.getUIString("assign_control");
            currentAssignmentLabel.Text = Configuration.getUIString("no_control_assigned");

            radioButtonRegularKeyAction.Checked = true;
            radioButtonViewOnly.Checked = true;

            if (builtInKeyMappings.Count <= 0)
            {
                foreach (KeyPresser.KeyCode value in Enum.GetValues(typeof(KeyPresser.KeyCode)))
                {
                    builtInKeyMappings.Add(value.ToString());
                }
            }
            if (comboBoxKeySelection.Items.Count <= 0)
            {
                comboBoxKeySelection.Items.AddRange(builtInKeyMappings.ToArray());
            }
            macroContainer = MacroManager.loadCommands(MacroManager.getMacrosFileLocation());

            availableMacroGames = GameDefinition.getAllAvailableGameDefinitions(true).Where(
                gd => gd.gameEnum != GameEnum.PCARS2_NETWORK).ToList();
            var items = from name in availableMacroGames orderby name.friendlyName ascending select name;
            availableMacroGames = items.ToList();
            listBoxGames.Items.Clear();
            foreach (var mapping in availableMacroGames)
            {
                listBoxGames.Items.Add(mapping.macroEditorName);
            }
            // try to select the CME game matching the main window game
            var selectionIndexFromPosition = Math.Min(MainWindow.instance.gameDefinitionList.SelectedIndex,
                listBoxGames.Items.Count-1);
            var selectionIndexFromName = listBoxGames.Items.IndexOf(MainWindow.instance.gameDefinitionList.SelectedItem);

            if (selectionIndexFromName != -1)
            {
                listBoxGames.SetSelected(selectionIndexFromName, true);
            }
            else if (selectionIndexFromPosition != -1)
            {
                listBoxGames.SetSelected(selectionIndexFromPosition, true);
            }
            updateMacroList();
            listBoxGames.Select();

            updateControllersList();
            updateAssignmentElements(true);

            this.KeyPreview = true;
            this.KeyDown += this.MacroEditor_KeyDown;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void MacroEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void listBoxGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxActionSequence.Text = "";
            textBoxDescription.Text = "";
            comboBoxKeySelection.SelectedIndex = -1;
            if (listBoxGames.SelectedIndex != -1)
            {
                buttonAddActionSequence.Text = Configuration.getUIString("add_action_sequence") + " " + listBoxGames.Items[listBoxGames.SelectedIndex].ToString();
            }
            if (listBoxAvailableMacros.SelectedIndex != -1)
            {
                listBoxAvailableMacros.SetSelected(listBoxAvailableMacros.SelectedIndex, true);
            }
        }

        private void listBoxAvailableMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxActionSequence.Text = "";
            textBoxDescription.Text = "";
            textBoxConfirmationMessage.Text = "";
            textBoxKeyPressTime.Text = "";
            textBoxWaitBetweenEachCommand.Text = "60";
            textBoxGameMacroDescription.Text = "";
            textBoxAddNewMacro.Text = "";
            textBoxVoiceTriggers.Text = "";
            if (listBoxGames.SelectedIndex == -1)
            {
                listBoxGames.SelectedIndex = 0;
            }
            if (listBoxAvailableMacros.SelectedIndex == -1)
            {
                return;
            }
            var currentSelectedGame = availableMacroGames[listBoxGames.SelectedIndex];
            var macro = macroContainer.macros.FirstOrDefault(m => m.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
            if (macro != null)
            {
                textBoxDescription.Text = macro.description;
                if (macro.integerVariableVoiceTrigger != null)
                {
                    textBoxVoiceTriggers.Text = macro.integerVariableVoiceTrigger;
                    textBoxVoiceTriggers.Multiline = false;
                    radioButtonIntegerVoiceTrigger.Checked = true;
                    radioButtonMultipleVoiceTrigger.Enabled = true;
                }
                else
                {
                    textBoxVoiceTriggers.Lines = macro.voiceTriggers;
                    textBoxVoiceTriggers.Multiline = true;
                    radioButtonRegularVoiceTrigger.Checked = true;
                    radioButtonMultipleVoiceTrigger.Enabled = false;
                }
                textBoxConfirmationMessage.Text = macro.confirmationMessage;
                textBoxAddNewMacro.Text = macro.name;
                if (macro.commandSets != null)
                {
                    var macroForGame = macro.commandSets.FirstOrDefault(cs => cs.gameDefinition == currentSelectedGame.gameEnum.ToString());
                    if (macroForGame != null)
                    {
                        textBoxActionSequence.Lines = macroForGame.actionSequence;
                        textBoxKeyPressTime.Text = macroForGame.keyPressTime == null ? "" : macroForGame.keyPressTime.Value.ToString();
                        textBoxWaitBetweenEachCommand.Text = macroForGame.waitBetweenEachCommand.ToString();
                        textBoxGameMacroDescription.Text = macroForGame.description;
                    }
                }

            }
            updateAssignmentElements(true);
        }

        private void updateAssignmentElements(bool updateSelectedController)
        {
            Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
            this.addAssignmentButton.Enabled = currentMacro != null && this.controllersList.SelectedIndex > -1 && ((MainWindow.ControllerUiEntry)this.controllersList.Items[this.controllersList.SelectedIndex]).isConnected;
            if (currentMacro != null && currentMacro.buttonTriggers != null && currentMacro.buttonTriggers.Length > 0
                && currentMacro.buttonTriggers[0].deviceId != null && currentMacro.buttonTriggers[0].deviceId.Length > 0)
            {
                this.currentAssignmentLabel.Text = "Assigned to " + currentMacro.buttonTriggers[0].description + ", Button " + currentMacro.buttonTriggers[0].buttonIndex;
                this.deleteAssignmentButton.Enabled = true;
                if (updateSelectedController)
                {
                    int index = 0;
                    foreach (var item in this.controllersList.Items)
                    {
                        if (item.ToString().Equals(currentMacro.buttonTriggers[0].description))
                        {
                            this.controllersList.SelectedIndex = index;
                            break;
                        }
                        index++;
                    }
                }
            }
            else
            {
                this.currentAssignmentLabel.Text = Configuration.getUIString("no_control_assigned");
                this.deleteAssignmentButton.Enabled = false;
                if (updateSelectedController)
                {
                    this.controllersList.SelectedIndex = -1;
                }
            }
        }

        private void textBoxKeyPressTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;         //Just Digits
            if (e.KeyChar == (char)8)
                e.Handled = false;        //Allow Backspace
        }

        private void textBoxWaitBetweenEachCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;         //Just Digits
            if (e.KeyChar == (char)8)
                e.Handled = false;        //Allow Backspace
        }

        private void buttonAddSelectedActionToSequence_Click(object sender, EventArgs e)
        {
            if (!radioButtonRegularKeyAction.Checked && string.IsNullOrWhiteSpace(textBoxSpecialActionParameter.Text))
            {
                MessageBox.Show(labelSpecialActionParameter.Text + " " + Configuration.getUIString("special_action_text_cant_be_empty"));

                return;
            }

            string formatedAction = "";
            List<string> currentLines = textBoxActionSequence.Lines.ToList();

            if (radioButtonWaitAction.Checked)
            {
                formatedAction = "{WAIT," + textBoxSpecialActionParameter.Text + "}";
            }
            else if (radioButtonFreeTextAction.Checked)
            {
                formatedAction = "{FREE_TEXT}" + textBoxSpecialActionParameter.Text;
            }
            else if(comboBoxKeySelection.SelectedIndex != -1)
            {
                if (radioButtonMultipleKeyAction.Checked)
                {
                    formatedAction = "{MULTIPLE," + textBoxSpecialActionParameter.Text + "}" + comboBoxKeySelection.Items[comboBoxKeySelection.SelectedIndex].ToString();
                }
                else if (radioButtonMultipleFuelAction.Checked || radioButtonMultipleVoiceTrigger.Checked)
                {
                    formatedAction = textBoxSpecialActionParameter.Text + comboBoxKeySelection.Items[comboBoxKeySelection.SelectedIndex].ToString();
                }
                else if (radioButtonRegularKeyAction.Checked)
                {
                    formatedAction = comboBoxKeySelection.Items[comboBoxKeySelection.SelectedIndex].ToString();
                }
            }
            else
            {
                MessageBox.Show(Configuration.getUIString("must_select_a_key"));
                return;
            }

            if (!string.IsNullOrWhiteSpace(formatedAction))
            {
                currentLines.Add(formatedAction);
            }
            textBoxActionSequence.Lines = currentLines.ToArray();
        }

        private void buttonSelectConfirmationMessage_Click(object sender, EventArgs e)
        {
            String soundPackLocationOverride = UserSettings.GetUserSettings().getString("override_default_sound_pack_location") + @"\voice";
            string soundPackLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CrewChiefV4\sounds\voice";
            if (soundPackLocationOverride != null && soundPackLocationOverride.Length > 0)
            {
                try
                {
                    if (Directory.Exists(soundPackLocationOverride))
                    {
                        soundPackLocation = soundPackLocationOverride;
                    }
                }
                catch (Exception ee) { Log.Exception(ee); }
            }
            try
            {
                if (!Directory.Exists(soundPackLocation))
                {
                    MessageBox.Show(Configuration.getUIString("macro_please_download_soundpack"));
                    return;
                }
                else
                {
                    using (OpenFileDialog folderBrowser = new OpenFileDialog())
                    {
                        folderBrowser.ValidateNames = false;
                        folderBrowser.CheckFileExists = false;
                        folderBrowser.CheckPathExists = true;
                        folderBrowser.FileName = "Folder Selection.";
                        folderBrowser.InitialDirectory = soundPackLocation;
                        if (folderBrowser.ShowDialog() == DialogResult.OK)
                        {
                            textBoxConfirmationMessage.Text = Path.GetDirectoryName(folderBrowser.FileName).Substring(soundPackLocation.Length).TrimStart('\\');
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        private void buttonAddNewMacro_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxAddNewMacro.Text) || string.IsNullOrWhiteSpace(textBoxVoiceTriggers.Text))
            {
                MessageBox.Show(Configuration.getUIString("empty_name_or_voicetrigger"));
                return;
            }

            if (canAddVoiceTrigger(radioButtonEditSelectedMacro.Checked))
            {
                List<Macro> currentMacros = macroContainer.macros.ToList();
                if (radioButtonAddNewMacro.Checked)
                {
                    Macro macro = new Macro();
                    macro.name = textBoxAddNewMacro.Text;
                    macro.description = textBoxDescription.Text;
                    if (radioButtonRegularVoiceTrigger.Enabled)
                    {
                        macro.voiceTriggers = textBoxVoiceTriggers.Lines;
                    }
                    else
                    {
                        macro.integerVariableVoiceTrigger = textBoxVoiceTriggers.Text;
                    }
                    if (textBoxConfirmationMessage.Text.Length > 0)
                    {
                        macro.confirmationMessage = textBoxConfirmationMessage.Text.Replace('\\', '/');
                    }
                    currentMacros.Add(macro);
                    macroContainer.macros = currentMacros.ToArray();
                    radioButtonViewOnly.Checked = true;
                    saveMacroSettings();
                    updateMacroList(true);

                }
                else if (listBoxAvailableMacros.SelectedIndex != -1)
                {
                    Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.SelectedItem.ToString());
                    currentMacro.name = textBoxAddNewMacro.Text;
                    currentMacro.description = textBoxDescription.Text;
                    if (radioButtonRegularVoiceTrigger.Enabled)
                    {
                        currentMacro.voiceTriggers = textBoxVoiceTriggers.Lines;
                    }
                    else
                    {
                        currentMacro.integerVariableVoiceTrigger = textBoxVoiceTriggers.Text;
                    }
                    if (textBoxConfirmationMessage.Text.Length > 0)
                    {
                        currentMacro.confirmationMessage = textBoxConfirmationMessage.Text.Replace('\\', '/');
                    }
                    radioButtonViewOnly.Checked = true;
                    saveMacroSettings();
                    updateMacroList(false);
                }
            }
        }
        private bool canAddVoiceTrigger(bool editExisting)
        {
            // also make sure we dont have any existing voice triggers with same name(s)

            if (!editExisting)
            {
                foreach (var m in macroContainer.macros)
                {
                    if (m.voiceTriggers != null)
                    {
                        foreach (var voiceTrigger in m.voiceTriggers)
                        {
                            if (textBoxVoiceTriggers.Lines.Contains(voiceTrigger))
                            {
                                MessageBox.Show(Configuration.getUIString("macro_voicetrigger_already_exists"));
                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var m in macroContainer.macros.Where(m => m.name != textBoxAddNewMacro.Text))
                {
                    if (m.voiceTriggers != null)
                    {
                        foreach (var voiceTrigger in m.voiceTriggers)
                        {
                            if (textBoxVoiceTriggers.Lines.Contains(voiceTrigger))
                            {
                                MessageBox.Show(Configuration.getUIString("macro_voicetrigger_already_exists"));
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        private void updateMacroList(bool setToEnd = false, bool retainIndex = true)
        {
            listBoxAvailableMacros.Items.Clear();
            if (macroContainer != null && macroContainer.macros != null)
            {
                foreach (var macro in macroContainer.macros)
                {
                    listBoxAvailableMacros.Items.Add(macro.name);
                }
                if (setToEnd)
                {
                    listBoxAvailableMacros.SetSelected(listBoxAvailableMacros.Items.Count - 1, true);
                }
                else
                {
                    listBoxAvailableMacros.SetSelected(0, true);
                }
            }
        }

        private void buttonAddActionSequence_Click(object sender, EventArgs e)
        {
            if(listBoxAvailableMacros.SelectedIndex == -1)
            {
                MessageBox.Show(Configuration.getUIString("must_select_a_macro"));
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxActionSequence.Text))
            {
                MessageBox.Show(Configuration.getUIString("action_sequence_cant_be_empty"));
                return;
            }
            if(string.IsNullOrWhiteSpace(textBoxWaitBetweenEachCommand.Text))
            {
                MessageBox.Show(labelWaitBetweenEachCommand.Text + " " + Configuration.getUIString("empty_wait_time_end"));
                return;
            }
            int currentSelectedMacroIndex = listBoxAvailableMacros.SelectedIndex;
            var currentSelectedGame = availableMacroGames[listBoxGames.SelectedIndex];
            Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
            if (currentMacro == null)
            {
                return;
            }
            List<CommandSet> currentCommandSets = new List<CommandSet>();
            if (currentMacro.commandSets != null)
            {
                currentCommandSets = currentMacro.commandSets.ToList();
            }
            CommandSet currentCommandSet = currentCommandSets.FirstOrDefault(cs => cs.gameDefinition == currentSelectedGame.gameEnum.ToString());
            List<string> actions = textBoxActionSequence.Lines.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (currentCommandSet == null)
            {
                currentCommandSet = new CommandSet();
                currentCommandSet.gameDefinition = currentSelectedGame.gameEnum.ToString();
                if (!string.IsNullOrWhiteSpace(textBoxGameMacroDescription.Text))
                {
                    currentCommandSet.description = textBoxGameMacroDescription.Text;
                }
                else
                {
                    currentCommandSet.description = currentSelectedGame.friendlyName + " version";
                }
                currentCommandSet.actionSequence = actions.ToArray();
                if (string.IsNullOrWhiteSpace(textBoxKeyPressTime.Text))
                {
                    currentCommandSet.keyPressTime = null;
                }
                else
                {
                    currentCommandSet.keyPressTime = int.Parse(textBoxKeyPressTime.Text);
                }
                currentCommandSet.waitBetweenEachCommand = int.Parse(textBoxWaitBetweenEachCommand.Text);
                currentCommandSets.Add(currentCommandSet);
                currentMacro.commandSets = currentCommandSets.ToArray();
                saveMacroSettings();
                updateMacroList(false);
                listBoxAvailableMacros.SetSelected(currentSelectedMacroIndex, true);

            }
            else
            {
                currentCommandSet.gameDefinition = currentSelectedGame.gameEnum.ToString();
                currentCommandSet.actionSequence = actions.ToArray();
                if (string.IsNullOrWhiteSpace(textBoxKeyPressTime.Text))
                {
                    currentCommandSet.keyPressTime = null;
                }
                else
                {
                    currentCommandSet.keyPressTime = int.Parse(textBoxKeyPressTime.Text);
                }
                currentCommandSet.waitBetweenEachCommand = int.Parse(textBoxWaitBetweenEachCommand.Text);
                if (!string.IsNullOrWhiteSpace(textBoxGameMacroDescription.Text))
                {
                    currentCommandSet.description = textBoxGameMacroDescription.Text;
                }
                else
                {
                    currentCommandSet.description = currentSelectedGame.friendlyName + " version";
                }
                saveMacroSettings();
                updateMacroList(false);
                listBoxAvailableMacros.SetSelected(currentSelectedMacroIndex, true);
            }
        }

        private void buttonDeleteSelectedMacro_Click(object sender, EventArgs e)
        {
            if (listBoxAvailableMacros.SelectedIndex == -1)
            {
                return;
            }
            if (MessageBox.Show(Configuration.getUIString("delete_selected_macro_confirmation"), Configuration.getUIString("delete_selected_macro_confirmation_title") + " " + listBoxAvailableMacros.SelectedItem.ToString(),
                MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                macroContainer.macros = macroContainer.macros.Where(val => val.name != listBoxAvailableMacros.SelectedItem.ToString()).ToArray();
                saveMacroSettings();
                updateMacroList(false);
            }

        }

        private void saveMacroSettings()
        {
            MacroManager.saveCommands(macroContainer);
        }

        private void buttonLoadUserMacroSettings_Click(object sender, EventArgs e)
        {
            macroContainer = MacroManager.loadCommands(MacroManager.getMacrosFileLocation());
            updateMacroList(false);
        }

        private void buttonLoadDefaultMacroSettings_Click(object sender, EventArgs e)
        {
            macroContainer = MacroManager.loadCommands(MacroManager.getMacrosFileLocation(true));
            updateMacroList(false);
        }

        private void radioButtonAvailableActions_CheckedChanged(object sender, EventArgs e)
        {
            buttonAddSelectedKeyToSequence.Enabled = true;
            textBoxDescription.ShortcutsEnabled = false;
            comboBoxKeySelection.Enabled = true;
            if (radioButtonRegularKeyAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = false;
                textBoxSpecialActionParameter.Text = "";
                labelSpecialActionParameter.Text = "";
            }
            else if (radioButtonMultipleKeyAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = true;
                textBoxSpecialActionParameter.Text = "";
                labelSpecialActionParameter.Text = Configuration.getUIString("action_repeated_key_presses");
            }
            else if (radioButtonMultipleFuelAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = false;
                textBoxSpecialActionParameter.Text = "{MULTIPLE,Fuel}";
                labelSpecialActionParameter.Text = "";
            }
            else if (radioButtonMultipleVoiceTrigger.Checked)
            {
                textBoxSpecialActionParameter.Enabled = false;
                textBoxSpecialActionParameter.Text = "{MULTIPLE,VOICE_TRIGGER}";
                labelSpecialActionParameter.Text = "";
            }
            else if (radioButtonWaitAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = true;
                textBoxSpecialActionParameter.Text = "";
                comboBoxKeySelection.Enabled = false;
                labelSpecialActionParameter.Text = Configuration.getUIString("action_wait_time");
            }
            else if (radioButtonFreeTextAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = true;
                textBoxSpecialActionParameter.Text = "";
                comboBoxKeySelection.Enabled = false;
                labelSpecialActionParameter.Text = Configuration.getUIString("action_free_text");
            }
            else if (radioButtonAdvancedEditAction.Checked)
            {
                textBoxSpecialActionParameter.Enabled = false;
                comboBoxKeySelection.Enabled = false;
                textBoxSpecialActionParameter.Text = "";
                labelSpecialActionParameter.Text = "";
                textBoxActionSequence.Enabled = true;
                buttonAddSelectedKeyToSequence.Enabled = false;
                textBoxDescription.ShortcutsEnabled = true;
            }

        }

        private void textBoxSpecialActionParameter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (radioButtonWaitAction.Checked || radioButtonMultipleKeyAction.Checked)
            {
                if (!char.IsDigit(e.KeyChar))
                    e.Handled = true;         //Just Digits
                if (e.KeyChar == (char)8)
                    e.Handled = false;        //Allow Backspace
            }

        }

        private void radioButtonAvailableMacros_CheckedChanged(object sender, EventArgs e)
        {
            buttonDeleteSelectedMacro.Enabled = false;
            groupBoxGameSettings.Enabled = false;
            if (radioButtonViewOnly.Checked)
            {
                buttonAddNewMacro.Enabled = false;
                textBoxAddNewMacro.Enabled = false;
                groupBoxGlobalMacroVoiceTrigger.Enabled = false;
                buttonSelectConfirmationMessage.Enabled = false;
                textBoxDescription.ShortcutsEnabled = false;
                groupBoxGameSettings.Enabled = true;
            }
            else
            {
                buttonAddNewMacro.Enabled = true;
                textBoxAddNewMacro.Enabled = true;
                groupBoxGlobalMacroVoiceTrigger.Enabled = true;
                buttonSelectConfirmationMessage.Enabled = true;
                textBoxDescription.ShortcutsEnabled = true;
            }
            if (radioButtonAddNewMacro.Checked)
            {
                listBoxAvailableMacros.ClearSelected();
                listBoxAvailableMacros.Enabled = false;
                textBoxConfirmationMessage.Text = "";
            }
            else
            {
                listBoxAvailableMacros.Enabled = true;
            }
            if(radioButtonEditSelectedMacro.Checked)
            {
                buttonDeleteSelectedMacro.Enabled = true;
            }

        }

        private void textBoxDescription_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (radioButtonViewOnly.Checked)
            {
                e.Handled = true;
            }
        }

        private void textBoxActionSequence_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!radioButtonAdvancedEditAction.Checked)
            {
                e.Handled = true;
            }
        }

        private void buttonUndoLastAction_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxActionSequence.Text))
            {
                textBoxActionSequence.Lines = textBoxActionSequence.Lines.Take(textBoxActionSequence.Lines.Count() - 1).ToArray();
            }
        }

        private void controllersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateAssignmentElements(false);
        }

        private void updateControllersList()
        {
            foreach (ControllerConfiguration.ControllerData configData in this.controllerConfiguration.controllers)
            {
                this.controllersList.Items.Add(new MainWindow.ControllerUiEntry(configData.deviceName, isConnected: true));
            }

            // Now, add grayed out (inactive) controllers
            foreach (ControllerConfiguration.ControllerData configData in this.controllerConfiguration.knownControllers)
            {
                if (this.controllerConfiguration.controllers.Exists(c => c.guid == configData.guid))
                {
                    continue;
                }
                this.controllersList.Items.Add(new MainWindow.ControllerUiEntry(configData.deviceName, isConnected: false));
            }
        }

        private void addAssignment_Click(object sender, EventArgs e)
        {
            if (!this.isAssigningButton)
            {
                if (this.controllersList.SelectedIndex >= 0)
                {
                    isAssigningButton = true;
                    this.addAssignmentButton.Text = Configuration.getUIString("waiting_for_button_click_to_cancel");
                    ThreadStart assignButtonWork = assignButton;
                    ThreadManager.UnregisterTemporaryThread(assignButtonThread);
                    assignButtonThread = new Thread(assignButtonWork);
                    assignButtonThread.Name = "MacroWindow.assignButtonThread";
                    ThreadManager.RegisterTemporaryThread(assignButtonThread);
                    assignButtonThread.Start();
                }
            }
            else
            {
                isAssigningButton = false;
                controllerConfiguration.listenForAssignment = false;
                this.addAssignmentButton.Text = Configuration.getUIString("assign_control");
            }
        }

        private void assignButton()
        {
            ButtonAssignment buttonAssignment = new ButtonAssignment();
            if (controllerConfiguration.assignButton(this, this.controllersList.SelectedIndex, buttonAssignment))
            {
                // update the macro with the controller guid and button id
                isAssigningButton = false;
                Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
                if (currentMacro != null)
                {
                    if (currentMacro.buttonTriggers == null || currentMacro.buttonTriggers.Length == 0)
                    {
                        currentMacro.buttonTriggers = new ButtonTrigger[] { new ButtonTrigger() };
                    }
                    currentMacro.buttonTriggers[0].buttonIndex = buttonAssignment.buttonIndex;
                    currentMacro.buttonTriggers[0].deviceId = buttonAssignment.deviceGuid;
                    currentMacro.buttonTriggers[0].description = buttonAssignment.controller.deviceName;
                    saveMacroSettings();
                    updateAssignmentElements(true);
                }
            }
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.addAssignmentButton.Text = Configuration.getUIString("assign_control");
                });
            }
            catch (Exception)
            {
                // Shutdown.
            }
        }

        private void deleteAssignment_Click(object sender, EventArgs e)
        {
            Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
            if (currentMacro != null && currentMacro.buttonTriggers != null && currentMacro.buttonTriggers.Length > 0)
            {
                bool remove = false;
                int i = 0;
                // remove the loaded button assignment for this macro. Here we match on macro name, which is kind of nasty. Note that if we move button assignments
                // to be in the per-game element this won't work
                for (; i < this.controllerConfiguration.buttonAssignments.Count; i++)
                {
                    if (this.controllerConfiguration.buttonAssignments[i].executableCommandMacro != null
                        && this.controllerConfiguration.buttonAssignments[i].executableCommandMacro.macro.name == currentMacro.name)
                    {
                        remove = true;
                        break;
                    }
                }
                if (remove)
                {
                    this.controllerConfiguration.buttonAssignments.RemoveAt(i);
                }
                currentMacro.buttonTriggers = null;
                saveMacroSettings();
                updateAssignmentElements(true);
            }
        }

        private void MacroEditor_Load(object sender, EventArgs e)
        {

        }
    }
}
