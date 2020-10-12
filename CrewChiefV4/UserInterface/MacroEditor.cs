using System;
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

namespace CrewChiefV4
{

    public partial class MacroEditor : Form
    {
        public static List<String> builtInKeyMappings = new List<String>();
        MacroContainer macroContainer = null;
        List<GameDefinition> availableMacroGames = null;
        public MacroEditor(Form parent)
        {
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

            availableMacroGames = GameDefinition.getAllGameDefinitions().Where(
                gd => gd.gameEnum != GameEnum.PCARS2_NETWORK && gd.gameEnum != GameEnum.AMS2 && gd.gameEnum != GameEnum.AMS2_NETWORK).ToList();
            var items = from name in availableMacroGames orderby name.friendlyName ascending select name;
            availableMacroGames = items.ToList();
            listBoxGames.Items.Clear();
            foreach (var mapping in availableMacroGames)
            {
                listBoxGames.Items.Add(mapping.macroEditorName);
            }
            // try to select the CME game matching the main window game
            var selection = Math.Min(MainWindow.instance.gameDefinitionList.SelectedIndex,
                listBoxGames.Items.Count-1);
            if (selection != -1)
            {
                listBoxGames.SetSelected(selection, true);
            }
            updateMacroList();
            listBoxGames.Select();

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
            textBoxKeyPressTime.Text = "20";
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
                        textBoxKeyPressTime.Text = macroForGame.keyPressTime.ToString();
                        textBoxWaitBetweenEachCommand.Text = macroForGame.waitBetweenEachCommand.ToString();
                        textBoxGameMacroDescription.Text = macroForGame.description;
                    }
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
                catch (Exception) {}
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
            if(string.IsNullOrWhiteSpace(textBoxKeyPressTime.Text) || string.IsNullOrWhiteSpace(textBoxWaitBetweenEachCommand.Text))
            {
                MessageBox.Show(Configuration.getUIString("empty_keypress_time_start") + " " +
                    labelKeyPressTime.Text + " " + Configuration.getUIString("empty_keypress_time_middle") + " " +
                    labelWaitBetweenEachCommand.Text + " " + Configuration.getUIString("empty_keypress_time_end"));
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
                currentCommandSet.keyPressTime = int.Parse(textBoxKeyPressTime.Text);
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
                currentCommandSet.keyPressTime = int.Parse(textBoxKeyPressTime.Text);
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
    }
}
