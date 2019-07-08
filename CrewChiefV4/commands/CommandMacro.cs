﻿using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.R3E;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CrewChiefV4.commands
{
    // wrapper that actually runs the macro
    public class ExecutableCommandMacro
    {
        private static Object mutex = new Object();

        AudioPlayer audioPlayer;
        public Macro macro;
        private Thread executableCommandMacroThread = null;

        public ExecutableCommandMacro(AudioPlayer audioPlayer, Macro macro)
        {
            this.audioPlayer = audioPlayer;
            this.macro = macro;
        }
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        bool BringGameWindowToFront(String processName, String[] alternateProcessNames, IntPtr currentForgroundWindow)
        {
            if (!MacroManager.bringGameWindowToFrontForMacros)
            {
                return false;
            }
            Process[] p = Process.GetProcessesByName(processName);
            if (p.Count() > 0)
            {
                if (p[0].MainWindowHandle != currentForgroundWindow)
                {
                    SetForegroundWindow(p[0].MainWindowHandle);
                    return true;
                }               
            }                
            else if (alternateProcessNames != null && alternateProcessNames.Length > 0)
            {
                foreach (String alternateProcessName in alternateProcessNames)
                {
                    p = Process.GetProcessesByName(processName);
                    if (p.Count() > 0)
                    {
                        if (p[0].MainWindowHandle != currentForgroundWindow)
                        {
                            SetForegroundWindow(p[0].MainWindowHandle);
                            return true;
                        }                       
                    } 
                }
            }
            return false;
        }

        private Boolean checkValidAndPlayConfirmation(CommandSet commandSet, Boolean supressConfirmationMessage)
        {
            Boolean isValid = true;
            String macroConfirmationMessage = macro.confirmationMessage != null && macro.confirmationMessage.Length > 0 && !supressConfirmationMessage ? 
                macro.confirmationMessage : null;


            // special case for 'request pit' macro - check we've not already requested a stop, and we might want to play the pitstop strategy estimate
            if (macro.name == MacroManager.REQUEST_PIT_IDENTIFIER)
            {
                // if there's a confirmation message set up here, suppress the PitStops event from triggering the same message when the pit request changes in the gamestate
                PitStops.playedRequestPitOnThisLap = macroConfirmationMessage != null;
                if ((CrewChief.gameDefinition == GameDefinition.raceRoom && R3EPitMenuManager.hasRequestedPitStop()) ||
                    ((CrewChief.gameDefinition == GameDefinition.pCars2 || CrewChief.gameDefinition == GameDefinition.rfactor2_64bit) &&
                     CrewChief.currentGameState != null && CrewChief.currentGameState.PitData.HasRequestedPitStop))
                {
                    // we've already requested a stop, so change the confirm message to 'yeah yeah, we know'
                    if (macroConfirmationMessage != null)
                    {
                        macroConfirmationMessage = PitStops.folderPitAlreadyRequested;
                    }
                    isValid = false;
                }
                if (isValid && MacroManager.enablePitExitPositionEstimates)
                {
                    Strategy.playPitPositionEstimates = true;
                }
                if (isValid && CrewChief.gameDefinition == GameDefinition.raceRoom)
                {
                    R3EPitMenuManager.outstandingPitstopRequest = true;
                    R3EPitMenuManager.timeWeCanAnnouncePitActions = CrewChief.currentGameState.Now.AddSeconds(10);
                }
            }
            // special case for 'cancel pit request' macro - check we've actually requested a stop
            else if (macro.name == MacroManager.CANCEL_REQUEST_PIT_IDENTIFIER)
            {
                // if there's a confirmation message set up here, suppress the PitStops event from triggering the same message when the pit request changes in the gamestate
                PitStops.playedPitRequestCancelledOnThisLap = macroConfirmationMessage != null;
                if ((CrewChief.gameDefinition == GameDefinition.raceRoom && !R3EPitMenuManager.hasRequestedPitStop()) ||
                    ((CrewChief.gameDefinition == GameDefinition.pCars2 || CrewChief.gameDefinition == GameDefinition.rfactor2_64bit) &&
                     CrewChief.currentGameState != null && !CrewChief.currentGameState.PitData.HasRequestedPitStop))
                {
                    // we don't have a stop requested, so change the confirm message to 'what? we weren't waiting anyway'
                    if (macroConfirmationMessage != null)
                    {
                        macroConfirmationMessage = PitStops.folderPitNotRequested;
                    }
                    isValid = false;
                }
                R3EPitMenuManager.outstandingPitstopRequest = false;
            }
            if (macroConfirmationMessage != null)
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(macroConfirmationMessage, 0));
            }
            return isValid;
        }

        public void execute(String recognitionResult, Boolean supressConfirmationMessage, Boolean useNewThread)
        {
            // blocking...
            Boolean isR3e = CrewChief.gameDefinition == GameDefinition.raceRoom;
            int multiplePressCountFromVoiceCommand = 0;
            if (macro.integerVariableVoiceTrigger != null && macro.integerVariableVoiceTrigger.Length > 0)
            {
                multiplePressCountFromVoiceCommand = macro.extractInt(recognitionResult, macro.startPhrase, macro.endPhrase);
            }
            // only execute for the requested game - is this check sensible?
            foreach (CommandSet commandSet in macro.commandSets.Where(cs => cs.gameDefinition == CrewChief.gameDefinition.gameEnum.ToString()))
            {                
                Boolean isValid = checkValidAndPlayConfirmation(commandSet, supressConfirmationMessage);
                if (isValid)
                {
                    if (useNewThread)
                    {
                        ThreadManager.UnregisterTemporaryThread(executableCommandMacroThread);
                        executableCommandMacroThread = new Thread(() =>
                        {
                            // only allow macros to excute one at a time
                            lock (ExecutableCommandMacro.mutex)
                            {
                                runMacro(commandSet, isR3e, multiplePressCountFromVoiceCommand);
                            }
                        });
                        executableCommandMacroThread.Name = "CommandMacro.executableCommandMacroThread";
                        ThreadManager.RegisterTemporaryThread(executableCommandMacroThread);
                        executableCommandMacroThread.Start();
                    }
                    else
                    {
                        runMacro(commandSet, isR3e, multiplePressCountFromVoiceCommand);
                    }
                }
                break;
            }            
        }

        private void runMacro(CommandSet commandSet, Boolean isR3e, int multiplePressCountFromVoiceCommand)
        {
            IntPtr currentForgroundWindow = GetForegroundWindow();
            bool hasChangedForgroundWindow = BringGameWindowToFront(CrewChief.gameDefinition.processName, CrewChief.gameDefinition.alternativeProcessNames, currentForgroundWindow);

            List<ActionItem> actionItems = commandSet.getActionItems();
            int actionItemsCount = actionItems.Count();

            // R3E set fuel macro is a special case. There are some menu commands to move the cursor to fuel and deselect it. We want to skip
            // all of these and simply move the cursor to fuel and deselect it (if necessary) using the new menu stuff. So skip all fuel key
            // presses after the initial command (which opens the pit menu) but before the event or SRE driven fuelling amount:
            int r3eFuelMacroSkipUntil = -1;
            if (isR3e && macro.name.Contains("fuel"))
            {
                for (int actionItemIndex = 0; actionItemIndex < actionItemsCount; actionItemIndex++)
                {
                    if (actionItems[actionItemIndex].extendedTypeTextParam != null && actionItemIndex >= 2)
                    {
                        // this is the action item that actually adds the fuel. We want the action item before this one to trigger (this resets
                        // fuel to 0) but action items before that reset will be skipped
                        r3eFuelMacroSkipUntil = actionItemIndex - 1;
                        break;
                    }
                }
            }
            for (int actionItemIndex = 0; actionItemIndex < actionItemsCount; actionItemIndex++)
            {
                ActionItem actionItem = actionItems[actionItemIndex];
                if (MacroManager.stopped)
                {
                    break;
                }
                if (MacroManager.WAIT_IDENTIFIER.Equals(actionItem.extendedType))
                {
                    Thread.Sleep(actionItem.extendedTypeNumericParam);
                }
                else
                {
                    int count;
                    if (actionItemIndex > 0 && r3eFuelMacroSkipUntil != -1)
                    {
                        // we're skipping all actions items until the actual fuelling action and replacing it with the proper menu navigation stuff
                        if (actionItemIndex < r3eFuelMacroSkipUntil)
                        {
                            continue;
                        }
                        else if (actionItemIndex == r3eFuelMacroSkipUntil)
                        {
                            Thread.Sleep(100);
                            R3EPitMenuManager.goToMenuItem(SelectedItem.Fuel);
                            R3EPitMenuManager.unselectFuel(false);
                            Thread.Sleep(100);
                        }
                    }
                    if (MacroManager.MULTIPLE_PRESS_IDENTIFIER.Equals(actionItem.extendedType))
                    {
                        if (actionItem.extendedTypeTextParam != null)
                        {
                            if (MacroManager.MULTIPLE_PRESS_FROM_VOICE_TRIGGER_IDENTIFIER.Equals(actionItem.extendedTypeTextParam))
                            {
                                count = multiplePressCountFromVoiceCommand;
                            }
                            else
                            {
                                count = CrewChief.getEvent(actionItem.extendedTypeTextParam).resolveMacroKeyPressCount(macro.name);
                            }

                            // eewwww... for the r3e fuel macro we want to ensure we're on 'fuel' and it's enabled before issuing the many
                            // commands to set the amount. In this case, there'll be multiple other button presses to get us to fuel so
                            // we need to catch the actual fuelling amount action and insert a crafty 'go to fuel item and deselect it' line
                            if (isR3e && macro.name.Contains("fuel"))
                            {
                                // hack for R3E: fuel menu needs 3 presses to get it from the start to 0
                                count = count + 3;
                            }
                        }
                        else
                        {
                            count = actionItem.extendedTypeNumericParam;
                        }
                    }
                    else
                    {
                        count = 1;
                    }
                    // only wait if there's another key press in the sequence
                    int wait = actionItemIndex == actionItemsCount - 1 ? 0 : commandSet.waitBetweenEachCommand;
                    sendKeys(count, actionItem, commandSet.keyPressTime, wait);
                }
            }
            // if we changed forground window we need to restore the old window again as the user could be running overlays or other apps they want to keep in forground.
            if (hasChangedForgroundWindow)
            {
                SetForegroundWindow(currentForgroundWindow);
            }
        }

        private void sendKeys(int count, ActionItem actionItem, int keyPressTime, int waitBetweenKeys)
        {
            // completely arbitrary sanity check on resolved count. We don't want the app trying to press 'right' MaxInt times
            if (actionItem.keyCodes.Length * count > 300)
            {
                Console.WriteLine("Macro item " + actionItem.actionText + " has > 300 key presses and will be ignored");
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (MacroManager.stopped)
                    {
                        break;
                    }
                    for (int keyIndex = 0; keyIndex < actionItem.keyCodes.Length; keyIndex++)
                    {
                        KeyPresser.SendScanCodeKeyPress(actionItem.keyCodes[keyIndex], actionItem.forcedUpperCases[keyIndex], keyPressTime);
                        Thread.Sleep(waitBetweenKeys);
                    }
                }
            }
        }
    }

    // JSON objects
    public class MacroContainer
    {
        // Legacy field needed for conversion
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Assignment[] assignments { get; set; }
        public Macro[] macros { get; set; }
    }

    public class Assignment
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String description { get; set; }
        public String gameDefinition { get; set; }
        public KeyBinding[] keyBindings { get; set; }
    }

    public class KeyBinding
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String description { get; set; }
        public String action { get; set; }
        public String key { get; set; }
    }

    public class Macro
    {
        public String name { get; set; }        
        public String description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String confirmationMessage { get; set; }
		
        public String[] voiceTriggers { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ButtonTrigger[] buttonTriggers { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CommandSet[] commandSets { get; set; }

        [JsonIgnore]
        private String _integerVariableVoiceTrigger;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String integerVariableVoiceTrigger
        {
            get { return _integerVariableVoiceTrigger; }
            set
            {
                this._integerVariableVoiceTrigger = value;
                parseIntRangeAndPhrase();
            }
        }

        [JsonIgnore]
        public Tuple<int, int> intRange;
        [JsonIgnore]
        public String startPhrase;
        [JsonIgnore]
        public String endPhrase;

        public int extractInt(String recognisedVoiceCommand, String start, String end)
        {
            foreach (KeyValuePair<String[], int> entry in SpeechRecogniser.numberToNumber)
            {
                foreach (String numberStr in entry.Key)
                {
                    if (recognisedVoiceCommand.Contains(start + numberStr + end))
                    {
                        return entry.Value;
                    }
                }
            }
            return 0;
        }

        private void parseIntRangeAndPhrase()
        {
            try
            {
                Boolean success = false;
                int start = this._integerVariableVoiceTrigger.IndexOf("{") + 1;
                int end = this._integerVariableVoiceTrigger.IndexOf("}", start);
                if (start != -1 && end > -1)
                {
                    String[] range = this._integerVariableVoiceTrigger.Substring(start, end - start).Split(',');
                    if (range.Length == 2)
                    {
                        this.startPhrase = this._integerVariableVoiceTrigger.Substring(0, this._integerVariableVoiceTrigger.IndexOf("{"));
                        this.endPhrase = this._integerVariableVoiceTrigger.Substring(this._integerVariableVoiceTrigger.IndexOf("}") + 1);
                        this.intRange = new Tuple<int, int>(int.Parse(range[0]), int.Parse(range[1]));
                        success = true;
                    }
                }
                if (!success)
                {
                    Console.WriteLine("Failed to parse range and phrase from voice trigger " + this._integerVariableVoiceTrigger + " in macro " + this.name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing range and phrase from voice trigger " + this._integerVariableVoiceTrigger + " in macro " + this.name + ", " + e.StackTrace);
            }
        }
    }

    public class CommandSet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String description { get; set; }
        public String gameDefinition { get; set; }
		public String[] actionSequence { get; set; }
		public int keyPressTime { get; set; }
        public int waitBetweenEachCommand { get; set; }
        [JsonIgnore]
        private List<ActionItem> actionItems = null;

        public Boolean loadActionItems()
        {
            this.actionItems = new List<ActionItem>();
            foreach (String action in actionSequence)
            {
                ActionItem actionItem = new ActionItem(action);
                if (actionItem.parsedSuccessfully)
                {
                    this.actionItems.Add(actionItem);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public List<ActionItem> getActionItems()
        {
            Console.WriteLine("Sending actions " + String.Join(", ", actionSequence));
            Console.WriteLine("Pressing keys " + String.Join(", ", actionItems));
            return actionItems;
        }
    }

    public class ActionItem
    {
        public Boolean parsedSuccessfully = false;
        public KeyPresser.KeyCode[] keyCodes;
        // for free-text entry, capital letters (e.g. in chat commands) will trigger holding down the LSHIFT key
        // for single action items this will be a single element array with 'false'
        public Boolean[] forcedUpperCases;
        public String actionText;
        public String extendedType;
        public String extendedTypeTextParam;
        public Boolean allowFreeText;
        public int extendedTypeNumericParam;

        public ActionItem(String action)
        {
            this.actionText = action;
            if (actionText.StartsWith("Multiple "))
            {
                Console.WriteLine("Action item \"" + action + "\" may need to be changed to \"{MULTIPLE," + action.Substring(action.IndexOf(" ") + 1) + "}\"");
            }
            else if (actionText.StartsWith("WAIT_"))
            {
                Console.WriteLine("Action item \"" + action + "\" may need to be changed to \"{WAIT," + action.Substring(action.IndexOf("_") + 1) + "}\"");
            }
            if (action.StartsWith("{"))
            {
                int start = action.IndexOf("{") + 1;
                int end = action.IndexOf("}", start);
                if (start != -1 && end > -1)
                {
                    String[] typeAndParam = action.Substring(start, end - start).Split(',');
                    if (typeAndParam.Length == 1 && MacroManager.FREE_TEXT_IDENTIFIER.Equals(typeAndParam[0]))
                    {
                        extendedType = typeAndParam[0];
                        allowFreeText = true;
                    }
                    else if (typeAndParam.Length == 2)
                    {
                        extendedType = typeAndParam[0];
                        if (typeAndParam[1].All(char.IsDigit))
                        {
                            extendedTypeNumericParam = int.Parse(typeAndParam[1]);
                        }
                        else
                        {
                            extendedTypeTextParam = typeAndParam[1];
                        }
                    }
                }
                action = action.Substring(action.IndexOf("}") + 1);
            }
            if (action.Length > 0)
            {
                try
                {
                    // first assume we have a single key binding
                    this.keyCodes = new KeyPresser.KeyCode[1];
                    this.forcedUpperCases = new Boolean[] { false };
                    // try and get it directly without going through the key bindings
                    parsedSuccessfully = parseKeycode(action, false, out this.keyCodes[0], out this.forcedUpperCases[0]);
                    if (!parsedSuccessfully)
                    {
                        if (allowFreeText)
                        {
                            // finally, try to parse each letter
                            this.keyCodes = new KeyPresser.KeyCode[action.Length];
                            // any of the free text chars might be upper case
                            this.forcedUpperCases = new Boolean[action.Length];
                            for (int i = 0; i < action.Length; i++)
                            {
                                parsedSuccessfully = parseKeycode(action[i].ToString(), true, out this.keyCodes[i], out this.forcedUpperCases[i]);
                                if (!parsedSuccessfully)
                                {
                                    Console.WriteLine("Unable to convert character " + action[i] + " to a key press");
                                    break;
                                }
                            }
                            if (parsedSuccessfully)
                            {
                                Console.WriteLine("Free text action macro, text = \"" + action + "\" key presses to send = " + String.Join(", ", keyCodes));
                            }
                        }
                        else
                        {
                            Console.WriteLine("actionItem = \"" + action + "\" not recognised");
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Action " + action + " not recognised");
                }
            }
            else
            {
                parsedSuccessfully = extendedType != null;
            }
        }

        private Boolean parseKeycode(String keyString, Boolean freeText, out KeyPresser.KeyCode keyCode, out Boolean forcedUppercase)
        {
            // assume we don't need to hold shift for this press:
            forcedUppercase = false;
            // some character literal replacements, only applicable to free text macros:
            if (freeText)
            {
                if (",".Equals(keyString))
                {
                    keyCode = KeyPresser.KeyCode.OEM_COMMA;
                    return true;
                }
                if (" ".Equals(keyString))
                {
                    keyCode = KeyPresser.KeyCode.SPACE_BAR;
                    return true;
                }
                if (".".Equals(keyString))
                {
                    keyCode = KeyPresser.KeyCode.OEM_PERIOD;
                    return true;
                }
                if ("-".Equals(keyString))
                {
                    keyCode = KeyPresser.KeyCode.OEM_MINUS;
                    return true;
                }
            }
            if (Enum.TryParse(keyString, true, out keyCode))
            {
                return true;
            }
            if (Enum.TryParse("KEY_" + keyString, true, out keyCode))
            {
                // if we're parsing this as a raw key and we're in free-text mode, hold shift if it's upper case
                forcedUppercase = freeText && Char.IsUpper(keyString[0]);
                return true;
            }
            return false;
        }

        public override String ToString()
        {
            if (parsedSuccessfully)
            {
                if (extendedType != null)
                {
                    String additionalInfo = "";
                    if (extendedTypeNumericParam > 0) {
                        additionalInfo = ": " + extendedTypeNumericParam;
                    }
                    else if (extendedTypeTextParam != null) {
                        additionalInfo = ": " + extendedTypeTextParam;
                    }
                    return extendedType + additionalInfo;
                }
                else
                {
                    return String.Join(",", keyCodes);
                }
            }
            else
            {
                return "unable to parse action " + actionText;
            }
        }
    }

    public class ButtonTrigger
    {
        public String description { get; set; }
        public String deviceId { get; set; }
        public int buttonIndex { get; set; }
    }
}
