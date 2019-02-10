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
        Assignment curentSelectedGameAssignments = null;
        public MacroEditor()
        {
            InitializeComponent();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.SuspendLayout();
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            groupBoxGlobalOptins.Text = Configuration.getUIString("global_macro_settings");
            labelGame.Text = Configuration.getUIString("game");
            labelAvailableMacros.Text = Configuration.getUIString("available_macros");
            labelGlobalMacroDescription.Text = Configuration.getUIString("macro_description");
            labelNewMacroName.Text = Configuration.getUIString("new_macro_name");
            labelConfirmationMessage.Text = Configuration.getUIString("confirmation_message");
            buttonAddNewMacro.Text = Configuration.getUIString("add_or_edit_macro");
            buttonDeleteSelectedMacro.Text = Configuration.getUIString("delete_selected_macro");
            buttonSelectConfirmationMessage.Text = Configuration.getUIString("select_confirmation_message");
            groupBoxGlobalMacroVoiceTrigger.Text = Configuration.getUIString("global_macro_voice_trigger");
            groupBoxGameSettings.Text = Configuration.getUIString("game_specific_settings");            
            labelAvailableActions.Text = Configuration.getUIString("available_actions");
            labelActionSequence.Text = Configuration.getUIString("action_sequence");
            labelNewActionName.Text = Configuration.getUIString("new_action");
            labelKeyPressTime.Text = Configuration.getUIString("keypress_time");
            labelWaitBetweenEachCommand.Text = Configuration.getUIString("keypress_wait_time");
            buttonSaveSelectedKey.Text = Configuration.getUIString("save_action_key");
            buttonAddActionSequence.Text = Configuration.getUIString("add_action_sequence");
            buttonAddActionToSequence.Text = Configuration.getUIString("add_action_to_sequence");
            buttonAddSelectedKeyToSequence.Text = Configuration.getUIString("add_key_to_sequence");
            buttonLoadUserMacroSettings.Text = Configuration.getUIString("load_user_macro_settings");
            buttonLoadDefaultMacroSettings.Text = Configuration.getUIString("load_default_macro_settings");
            buttonSaveMacroSettings.Text = Configuration.getUIString("save_macro_settings");
            labelGameMacroDescription.Text = Configuration.getUIString("macro_description");

            radioButtonRegularVoiceTrigger.Text = Configuration.getUIString("regular_macro_voice_command");
            radioButtonIntegerVoiceTrigger.Text = Configuration.getUIString("integer_macro_voice_command");
            groupBoxGlobalMacroVoiceTrigger.Text = Configuration.getUIString("macro_voice_trigger");
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
            availableMacroGames = GameDefinition.getAllGameDefinitions().Where(gd => gd.gameEnum != GameEnum.PCARS2_NETWORK).ToList();
            var items = from name in availableMacroGames orderby name.friendlyName ascending select name;
            availableMacroGames = items.ToList();
            listBoxGames.Items.Clear();         
            foreach (var mapping in availableMacroGames)
            {
                listBoxGames.Items.Add(mapping.friendlyName);                
            }
            listBoxGames.SetSelected(MainWindow.instance.gameDefinitionList.SelectedIndex, true);  
            updateMacroList();                      
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void listBoxGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxAvailableActions.Items.Clear();
            textBoxActionSequence.Text = "";
            textBoxDescription.Text = "";

            comboBoxKeySelection.SelectedIndex = -1;

            var actionsForGame = availableMacroGames[listBoxGames.SelectedIndex];
            curentSelectedGameAssignments = macroContainer.assignments.FirstOrDefault(mc => mc.gameDefinition == actionsForGame.gameEnum.ToString());
            
            if (curentSelectedGameAssignments != null)
            {
                foreach (var binding in curentSelectedGameAssignments.keyBindings)
                {
                    listBoxAvailableActions.Items.Add(binding.action  + " " + Configuration.getUIString("assigned_to") + " " + binding.key);                
                }            
            }
            if(listBoxAvailableMacros.SelectedIndex != -1)
            {
                listBoxAvailableMacros.SetSelected(listBoxAvailableMacros.SelectedIndex, true); 
            }
        }

        private void listBoxAvailableActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAvailableActions.SelectedIndex != -1 && curentSelectedGameAssignments != null)
            {
                comboBoxKeySelection.SelectedIndex = builtInKeyMappings.IndexOf(curentSelectedGameAssignments.keyBindings[listBoxAvailableActions.SelectedIndex].key); 
            }
        }

        private void buttonSaveSelectedKey_Click(object sender, EventArgs e)
        {
            if (comboBoxKeySelection.SelectedIndex == -1 || listBoxGames.SelectedIndex == -1)
                return;
            string selectedKeyName = comboBoxKeySelection.Items[comboBoxKeySelection.SelectedIndex].ToString();
            var currentSelectedGame = availableMacroGames[listBoxGames.SelectedIndex];
            if (!string.IsNullOrEmpty(textBoxAddNewAction.Text) && curentSelectedGameAssignments != null)
            {                    
                List<KeyBinding> currentKeyBindings = curentSelectedGameAssignments.keyBindings.ToList();
                
                currentKeyBindings.Add(new KeyBinding() { action = textBoxAddNewAction.Text, key = selectedKeyName });
                curentSelectedGameAssignments.keyBindings = currentKeyBindings.ToArray();
                
                listBoxAvailableActions.Items.Add(textBoxAddNewAction.Text + " " + Configuration.getUIString("assigned_to") + " " + selectedKeyName);
                textBoxAddNewAction.Text = "";
            }
            else if (curentSelectedGameAssignments == null && !string.IsNullOrEmpty(textBoxAddNewAction.Text))
            {
                List<Assignment> currentAssignments = macroContainer.assignments.ToList();
                List<KeyBinding> keyBindings = new List<KeyBinding>();
                
                keyBindings.Add(new KeyBinding(){action = textBoxAddNewAction.Text, key = selectedKeyName});
                currentAssignments.Add(new Assignment() { gameDefinition = currentSelectedGame.gameEnum.ToString(), keyBindings = keyBindings.ToArray() });
                macroContainer.assignments = currentAssignments.ToArray();
                
                listBoxAvailableActions.Items.Add(textBoxAddNewAction.Text + " " + Configuration.getUIString("assigned_to") + " " + selectedKeyName);
                textBoxAddNewAction.Text = "";
            }
            else if (listBoxAvailableActions.SelectedIndex != -1 && curentSelectedGameAssignments != null)
            {
                listBoxAvailableActions.Items[listBoxAvailableActions.SelectedIndex] = (curentSelectedGameAssignments.keyBindings[listBoxAvailableActions.SelectedIndex].action + " " + Configuration.getUIString("assigned_to") + " " + selectedKeyName);                        
                curentSelectedGameAssignments.keyBindings[listBoxAvailableActions.SelectedIndex].key = selectedKeyName;
            }
            listBoxAvailableActions_SelectedIndexChanged(sender, e);            
        }

        private void listBoxAvailableMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxActionSequence.Text = "";
            textBoxDescription.Text = "";
            textBoxConfirmationMessage.Text = "";
            textBoxKeyPressTime.Text = "";
            textBoxWaitBetweenEachCommand.Text = "";
            textBoxGameMacroDescription.Text = "";
            if(listBoxGames.SelectedIndex == -1)
            {
                listBoxGames.SelectedIndex = 0;
            }

            var currentSelectedGame = availableMacroGames[listBoxGames.SelectedIndex];
            var macro = macroContainer.macros.FirstOrDefault(m => m.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
            if(macro != null)
            {
                textBoxDescription.Text = macro.description;
                if (macro.integerVariableVoiceTrigger != null)
                {
                    textBoxVoiceTriggers.Text = macro.integerVariableVoiceTrigger;
                    textBoxVoiceTriggers.Multiline = false;
                    radioButtonIntegerVoiceTrigger.Checked = true;
                }
                else
                {                    
                    textBoxVoiceTriggers.Lines = macro.voiceTriggers;
                    textBoxVoiceTriggers.Multiline = true;
                    radioButtonRegularVoiceTrigger.Checked = true;
                }
                textBoxConfirmationMessage.Text = macro.confirmationMessage;
                textBoxAddNewMacro.Text = macro.name;
                if(macro.commandSets != null)
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

        private void buttonAddActionToSequence_Click(object sender, EventArgs e)
        {
            if (listBoxAvailableActions.SelectedIndex != -1 && curentSelectedGameAssignments != null)
            {
                List<string> currentLines = textBoxActionSequence.Lines.ToList();
                currentLines.Add(curentSelectedGameAssignments.keyBindings[listBoxAvailableActions.SelectedIndex].action);
                textBoxActionSequence.Lines = currentLines.ToArray();
            }
        }

        private void buttonAddSelectedKeyToSequence_Click(object sender, EventArgs e)
        {
            if (comboBoxKeySelection.SelectedIndex != -1 )
            {
                List<string> currentLines = textBoxActionSequence.Lines.ToList();
                currentLines.Add(comboBoxKeySelection.Items[comboBoxKeySelection.SelectedIndex].ToString());
                textBoxActionSequence.Lines = currentLines.ToArray();
            }
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
                catch(Exception) {}
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
            Macro currentMacro = macroContainer.macros.FirstOrDefault(mc => mc.name == textBoxAddNewMacro.Text);

            if (canAddVoiceTrigger(currentMacro != null))
            {
                List<Macro> currentMacros = macroContainer.macros.ToList();
                if(currentMacro == null)
                {                    
                    currentMacro = new Macro();
                    currentMacro.name = textBoxAddNewMacro.Text;
                    currentMacro.description = textBoxDescription.Text;
                    if(radioButtonRegularVoiceTrigger.Enabled)
                    {
                        currentMacro.voiceTriggers = textBoxVoiceTriggers.Lines;
                    }
                    else
                    {
                        currentMacro.integerVariableVoiceTrigger = textBoxVoiceTriggers.Text;
                    }
                    if (textBoxConfirmationMessage.Text.Length > 0)
                    {
                        currentMacro.confirmationMessage = textBoxConfirmationMessage.Text;
                    }                                   
                    currentMacros.Add(currentMacro);
                    macroContainer.macros = currentMacros.ToArray();
                    textBoxAddNewMacro.Text = "";
                    textBoxDescription.Text = "";
                    textBoxVoiceTriggers.Text = "";
                    updateMacroList(true);
                }
                else
                {
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
                        currentMacro.confirmationMessage = textBoxConfirmationMessage.Text;
                    }
                    textBoxAddNewMacro.Text = "";
                    textBoxDescription.Text = "";
                    textBoxVoiceTriggers.Text = "";
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
        private void updateMacroList(bool setToEnd = false)
        {
            int currentSelectedIndex = listBoxAvailableMacros.SelectedIndex;
            listBoxAvailableMacros.Items.Clear();
            foreach (var macro in macroContainer.macros)
            {
                listBoxAvailableMacros.Items.Add(macro.name);
            }
            if(setToEnd)
            {
                listBoxAvailableMacros.SetSelected(listBoxAvailableMacros.Items.Count - 1, true);
            }
            else if (currentSelectedIndex != -1 && currentSelectedIndex <= listBoxAvailableMacros.Items.Count)
            {
                listBoxAvailableMacros.SetSelected(currentSelectedIndex, true);
            }
            else
            {
                listBoxAvailableMacros.SetSelected(0, true);
            }
        }

        private void buttonAddActionSequence_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(textBoxActionSequence.Text) && 
                listBoxAvailableMacros.SelectedIndex != -1 &&
                !string.IsNullOrWhiteSpace(textBoxKeyPressTime.Text) && 
                !string.IsNullOrWhiteSpace(textBoxWaitBetweenEachCommand.Text))
            {
                var currentSelectedGame = availableMacroGames[listBoxGames.SelectedIndex];
                Macro currentMacro =  macroContainer.macros.FirstOrDefault(mc => mc.name == listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString());
                if(currentMacro == null)
                {
                    return;
                }
                List<CommandSet> currentCommandSets = new List<CommandSet>();
                if(currentMacro.commandSets != null)
                {
                    currentCommandSets = currentMacro.commandSets.ToList();
                }                
                CommandSet currentCommandSet = currentCommandSets.FirstOrDefault(cs => cs.gameDefinition == currentSelectedGame.gameEnum.ToString());
                List<string> actions = textBoxActionSequence.Lines.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                if(currentCommandSet == null)
                {
                    currentCommandSet = new CommandSet();
                    currentCommandSet.gameDefinition = currentSelectedGame.gameEnum.ToString();
                    if(!string.IsNullOrWhiteSpace(textBoxGameMacroDescription.Text))
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
                    //MacroManager.saveCommands(macroContainer);
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
                    //MacroManager.saveCommands(macroContainer);
                }
            }
        }

        private void buttonDeleteSelectedMacro_Click(object sender, EventArgs e)
        {
            if (listBoxAvailableMacros.SelectedIndex == -1)
            {
                return;
            }
            macroContainer.macros = macroContainer.macros.Where(val => val.name != listBoxAvailableMacros.Items[listBoxAvailableMacros.SelectedIndex].ToString()).ToArray();
            updateMacroList(false);
        }

        private void buttonSaveMacroSettings_Click(object sender, EventArgs e)
        {
            MacroManager.saveCommands(macroContainer);
            updateMacroList(false);
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
    }
}
