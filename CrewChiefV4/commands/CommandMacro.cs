using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using CrewChiefV4.R3E;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsInput.Native;
namespace CrewChiefV4.commands
{
    // wrapper that actually runs the macro
    public class ExecutableCommandMacro
    {
        private static Object mutex = new Object();

        AudioPlayer audioPlayer;
        public Macro macro;
        private Thread executableCommandMacroThread = null;

        private bool macroExecutingOnCommandMacroThread = false;

        public ExecutableCommandMacro(AudioPlayer audioPlayer, Macro macro)
        {
            this.audioPlayer = audioPlayer;
            this.macro = macro;
        }
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// If the game process is not already the foreground window, set it to be.
        /// rFactor 2 in particular has 3 or 4 processes, only one is "the" window
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="currentForgroundWindow"></param>
        /// <returns>true: foreground was changed</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if unable
        /// to change foreground window</exception>
        private bool SetGameProcessAsForeground(String processName, IntPtr currentForgroundWindow)
        {
            Process[] matchingProcesses = Process.GetProcessesByName(processName);
            foreach (var gameProcess in matchingProcesses)
            {
                if (gameProcess.MainWindowHandle != (IntPtr)0 &&
                    gameProcess.MainWindowHandle != currentForgroundWindow)
                {
                    if (SetForegroundWindow(gameProcess.MainWindowHandle))
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Couldn't set {processName} to be the current window");
                        throw new System.InvalidOperationException($"Couldn't set {processName} to be the current window");
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// If the game window is not already the foreground window, set it to be.
        /// </summary>
        /// <param name="processName"> Name of the game</param>
        /// <param name="alternateProcessNames"> Optional list of alternate
        /// names the game might use</param>
        /// <param name="currentForgroundWindow"></param>
        /// <returns>true: foreground was changed</returns>
        bool BringGameWindowToFront(String processName, String[] alternateProcessNames, IntPtr currentForgroundWindow)
        {
            if (!UserSettings.GetUserSettings().getBoolean("bring_game_window_to_front_for_macros"))
            {
                return false;
            }
            if (SetGameProcessAsForeground(processName, currentForgroundWindow))
            {
                return true;
            }
            else if (alternateProcessNames != null && alternateProcessNames.Length > 0)
            {
                foreach (String alternateProcessName in alternateProcessNames)
                {
                    if (SetGameProcessAsForeground(alternateProcessName, currentForgroundWindow))
                    {
                        return true;
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
                    ((CrewChief.gameDefinition == GameDefinition.pCars2 || CrewChief.gameDefinition == GameDefinition.rfactor2_64bit || CrewChief.gameDefinition == GameDefinition.AMS2) &&
                     CrewChief.currentGameState != null && CrewChief.currentGameState.PitData.HasRequestedPitStop))
                {
                    // special case for R3E. Pit requested state doesn't clear after completing a stop, so we might need to execute the
                    // serve penalty macro even if we deem this request invalid (the pit request flag is already true)
                    if (CrewChief.gameDefinition == GameDefinition.raceRoom && CrewChief.currentGameState != null &&
                        (CrewChief.currentGameState.PenaltiesData.PenaltyType == GameState.PenatiesData.DetailedPenaltyType.DRIVE_THROUGH
                         || CrewChief.currentGameState.PenaltiesData.PenaltyType == GameState.PenatiesData.DetailedPenaltyType.STOP_AND_GO))
                    {
                        macroConfirmationMessage = AudioPlayer.folderAcknowlegeOK;
                        R3EPitMenuManager.selectServePenalty();
                        // we don't want to allow the requested macro (request pit) to execute in this case because a stop is already
                        // requested and doing so would actually cancel it - we just want to ensure the penalty is selected, so we're
                        // overriding the acknowledge and kicking off the selectServePenalty process in the R3E pit menu manager.
                    }
                    else
                    {
                        // we've already requested a stop, so change the confirm message to 'yeah yeah, we know'
                        if (macroConfirmationMessage != null)
                        {
                            macroConfirmationMessage = PitStops.folderPitAlreadyRequested;
                        }
                    }
                    isValid = false;
                }
                if (isValid)
                {
                    if (CrewChief.gameDefinition == GameDefinition.raceRoom && CrewChief.currentGameState != null &&
                        (CrewChief.currentGameState.PenaltiesData.PenaltyType == GameState.PenatiesData.DetailedPenaltyType.DRIVE_THROUGH
                         || CrewChief.currentGameState.PenaltiesData.PenaltyType == GameState.PenatiesData.DetailedPenaltyType.STOP_AND_GO))
                    {
                        // if we request a stop and we have a penalty, select the 'serve penalty' option in R3E
                        macroConfirmationMessage = AudioPlayer.folderAcknowlegeOK;
                        R3EPitMenuManager.selectServePenalty();
                    }
                    else if (MacroManager.enablePitExitPositionEstimates)
                    {
                        Strategy.playPitPositionEstimates = true;
                    }
                    if (CrewChief.gameDefinition == GameDefinition.raceRoom)
                    {
                        R3EPitMenuManager.outstandingPitstopRequest = true;
                        if (CrewChief.currentGameState != null && CrewChief.currentGameState.Now != null)
                            R3EPitMenuManager.timeWeCanAnnouncePitActions = CrewChief.currentGameState.Now.AddSeconds(10);
                    }
                }
            }
            // special case for 'cancel pit request' macro - check we've actually requested a stop
            else if (macro.name == MacroManager.CANCEL_REQUEST_PIT_IDENTIFIER)
            {
                // if there's a confirmation message set up here, suppress the PitStops event from triggering the same message when the pit request changes in the gamestate
                PitStops.playedPitRequestCancelledOnThisLap = macroConfirmationMessage != null;
                if ((CrewChief.gameDefinition == GameDefinition.raceRoom && !R3EPitMenuManager.hasRequestedPitStop()) ||
                    ((CrewChief.gameDefinition == GameDefinition.pCars2 || CrewChief.gameDefinition == GameDefinition.rfactor2_64bit || CrewChief.gameDefinition == GameDefinition.AMS2) &&
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

        public bool execute(String recognitionResult, Boolean supressConfirmationMessage, Boolean useNewThread)
        {
            bool allowedToRun = true;
            // blocking...
            Boolean isR3e = CrewChief.gameDefinition == GameDefinition.raceRoom;
            int multiplePressCountFromVoiceCommand = 0;
            if (macro.integerVariableVoiceTrigger != null && macro.integerVariableVoiceTrigger.Length > 0)
            {
                multiplePressCountFromVoiceCommand = macro.extractInt(recognitionResult, macro.startPhrase, macro.endPhrase);
            }
            // only execute for the requested game - is this check sensible?
            foreach (CommandSet commandSet in macro.commandSets.Where(cs => MacroManager.isCommandSetForCurrentGame(cs.gameDefinition)))
            {
                Boolean isValid = checkValidAndPlayConfirmation(commandSet, supressConfirmationMessage);
                if (isValid)
                {
                    if (useNewThread)
                    {
                        if (this.macroExecutingOnCommandMacroThread)
                        {
                            Log.Debug("Macro \"" + this.macro.name + "\" can't run while another macro is excuting");
                            allowedToRun = false;
                        }
                        else
                        {
                            this.macroExecutingOnCommandMacroThread = true;
                            ThreadManager.UnregisterTemporaryThread(executableCommandMacroThread);
                            executableCommandMacroThread = new Thread(() =>
                            {
                                // only allow macros to excute one at a time
                                lock (ExecutableCommandMacro.mutex)
                                {
                                    this.macroExecutingOnCommandMacroThread = true;
                                    try
                                    {
                                        runMacro(commandSet, isR3e, multiplePressCountFromVoiceCommand);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error executing command macro: " + e.ToString());
                                    }
                                    this.macroExecutingOnCommandMacroThread = false;
                                }
                            });
                            executableCommandMacroThread.Name = "CommandMacro.executableCommandMacroThread";
                            ThreadManager.RegisterTemporaryThread(executableCommandMacroThread);
                            executableCommandMacroThread.Start();
                        }
                    }
                    else
                    {
                        try
                        {
                            runMacro(commandSet, isR3e, multiplePressCountFromVoiceCommand);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error executing command macro: " + e.ToString());
                        }
                    }
                }
                break;
            }
            return allowedToRun;
        }

        // This must be called from static method SpeechRecogniser.getStartChatMacro()
        public ActionItem getSingleActionItemForChatStartAndEnd()
        {
            foreach (CommandSet commandSet in macro.commandSets.Where(cs => MacroManager.isCommandSetForCurrentGame(cs.gameDefinition)))
            {
                if (commandSet.getActionItems().Count == 1)
                {
                    return commandSet.getActionItems()[0];
                }
                break;
            }
            return null;
        }

        public int getWaitBetweenEachCommand()
        {
            //defaut to 100
            int waitTime = 100;
            foreach (CommandSet commandSet in macro.commandSets.Where(cs => MacroManager.isCommandSetForCurrentGame(cs.gameDefinition)))
            {
                waitTime = commandSet.waitBetweenEachCommand;
            }
            return waitTime;
        }

        private void runMacro(CommandSet commandSet, Boolean isR3e, int multiplePressCountFromVoiceCommand)
        {
            IntPtr currentForgroundWindow = GetForegroundWindow();
            try // catch "Couldn't set the game to foreground", don't send keys in that case
            {
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
                        if (actionItemIndex >= 2 && 
                            (actionItems[actionItemIndex].repeatCountFromVoiceCommand 
                                || "fuel".Equals(actionItems[actionItemIndex].eventToResolveRepeatCount, StringComparison.InvariantCultureIgnoreCase)))
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
                    if (actionItem.applicableSessionTypes == null || CrewChief.currentGameState == null || actionItem.applicableSessionTypes.Contains(CrewChief.currentGameState.SessionData.SessionType))
                    {
                        if (actionItem.waitTime > 0)
                        {
                            Thread.Sleep(actionItem.waitTime);
                        }
                        if (actionItemIndex > 0 && r3eFuelMacroSkipUntil != -1)
                        {
                            // we're skipping all actions items until the actual fuelling action and replacing it with the proper menu navigation stuff
                            if (actionItemIndex < r3eFuelMacroSkipUntil)
                            {
                                continue;
                            }
                            else if (actionItemIndex == r3eFuelMacroSkipUntil)
                            {
                                // eewwww... for the r3e fuel macro we want to ensure we're on 'fuel' and it's enabled before issuing the many
                                // commands to set the amount. In this case, there'll be multiple other button presses to get us to fuel so
                                // we need to catch the actual fuelling amount action and insert a crafty 'go to fuel item and deselect it' line
                                Thread.Sleep(100);
                                R3EPitMenuManager.goToMenuItem(SelectedItem.Fuel);
                                R3EPitMenuManager.unselectFuel(false);
                                Thread.Sleep(100);
                            }
                        }
                        int count = actionItem.fixedRepeatCount;
                        bool doR3EFuellingMenuHack = false;
                        if (actionItem.eventToResolveRepeatCount != null)
                        {
                            count = CrewChief.getEvent(actionItem.eventToResolveRepeatCount).resolveMacroKeyPressCount(macro.name);
                            doR3EFuellingMenuHack = isR3e && actionItem.eventToResolveRepeatCount.Equals("fuel", StringComparison.InvariantCultureIgnoreCase);
                        }
                        else if (actionItem.repeatCountFromVoiceCommand)
                        {
                            count = multiplePressCountFromVoiceCommand;
                            // no event to check against here - this is the case where we say "add fuel, 20 litres" - we want an additional 3 presses for this
                            doR3EFuellingMenuHack = isR3e && macro.name.Contains("fuel");
                        }
                        if (doR3EFuellingMenuHack)
                        {
                            // hack for R3E: fuel menu needs 3 presses to get it from the start to 0. 3 extra presses when dropping the fuel to zero doesn't matter
                            // so this added the extras to both command actions
                            count = count + 3;
                        }
                        if (count > 0 && actionItem.keyCodes != null)
                        {
                            int? keyPressTime = commandSet.keyPressTime;
                            if (actionItem.holdTime > 0)
                            {
                                keyPressTime = actionItem.holdTime;
                            }
                            sendKeys(count, actionItem, keyPressTime, commandSet.waitBetweenEachCommand);
                        }
                    }
                }
                // if we changed forground window we need to restore the old window again as the user could be running overlays or other apps they want to keep in forground.
                if (hasChangedForgroundWindow)
                {
                    SetForegroundWindow(currentForgroundWindow);
                }
            }
            catch (System.InvalidOperationException e)
            {
                // Couldn't set the game to foreground
            }
        }
 
        private void sendKeys(int count, ActionItem actionItem, int? keyPressTime, int waitBetweenKeys)
        {
            if (actionItem.allowFreeText)
            {
                // 3 cases here: either we have one or more actions either side the free text, or we have no actions either
                // side but have start / end chat macros, or we guess and surround the free text with T and ENTER
                if (actionItem.actionItemsBeforeFreeText.Count > 0)
                {
                    foreach (ActionItem beforeItem in actionItem.actionItemsBeforeFreeText)
                    {
                        // TODO: do we actually need to parse out the repeats here?
                        sendKeys(1, beforeItem, actionItem.holdTime, waitBetweenKeys);
                    }
                }
                else
                {
                    ActionItem startChatActionItem = SpeechRecogniser.getStartChatMacro() == null ? null : SpeechRecogniser.getStartChatMacro().getSingleActionItemForChatStartAndEnd();
                    if (startChatActionItem == null)
                    {
                        // yikes, no start chat macro, press T and hope
                        KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, VirtualKeyCode.VK_T), keyPressTime);
                    }
                    else
                    {
                        KeyPresser.SendKeyPresses(startChatActionItem.keyCodes, keyPressTime, startChatActionItem.waitTime);
                    }
                }                
                Console.WriteLine("Sending " + actionItem.freeText);
                KeyPresser.InputSim.Keyboard.TextEntry(actionItem.freeText);
                Thread.Sleep(getWaitBetweenEachCommand());

                if (actionItem.actionItemsAfterFreeText.Count > 0)
                {
                    foreach (ActionItem afterItem in actionItem.actionItemsAfterFreeText)
                    {
                        // TODO: do we actually need to parse out the repeats here?
                        sendKeys(1, afterItem, actionItem.holdTime, waitBetweenKeys);
                    }
                }
                else
                {
                    ActionItem endChatActionItem = SpeechRecogniser.getEndChatMacro() == null ? null : SpeechRecogniser.getEndChatMacro().getSingleActionItemForChatStartAndEnd();
                    if (endChatActionItem == null)
                    {
                        // yikes, no end chat macro, press ENTER and hope
                        KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, VirtualKeyCode.RETURN), keyPressTime);
                    }
                    else
                    {
                        KeyPresser.SendKeyPresses(endChatActionItem.keyCodes, keyPressTime, endChatActionItem.waitTime);
                    }
                }
            }
            // completely arbitrary sanity check on resolved count. We don't want the app trying to press 'right' MaxInt times
            else if (actionItem.keyCodes.Length * count > 300)
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
                    else
                    {
                        KeyPresser.SendKeyPresses(actionItem.keyCodes, keyPressTime, waitBetweenKeys);
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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? keyPressTime { get; set; }
        public int waitBetweenEachCommand { get; set; }
        [JsonIgnore]
        private List<ActionItem> actionItems = null;

        public Boolean loadActionItems()
        {
            this.actionItems = new List<ActionItem>();
            if (this.actionSequence == null)
            {
                Console.WriteLine("No action sequence for commandSet " + description);
                return false;
            }

            foreach (String action in actionSequence.Where(ai => ai.Contains(MacroManager.FREE_TEXT_IDENTIFIER)))
            {
                ActionItem actionItem = new ActionItem(action);
                if (actionItem.parsedSuccessfully)
                {
                    this.actionItems.Add(actionItem);
                    Console.WriteLine("Found " + MacroManager.FREE_TEXT_IDENTIFIER + " for commandSet " + description);
                    if (actionSequence.Length > 1)
                    {
                        // save the actions before and after. If these are set we'll play them, otherwise we'll 
                        // look for the start / end chat macros
                        bool before = true;
                        foreach (String surroundingAction in actionSequence)
                        {
                            if (surroundingAction.Contains(MacroManager.FREE_TEXT_IDENTIFIER))
                            {
                                before = false;
                            }
                            else if (before)
                            {
                                actionItem.actionItemsBeforeFreeText.Add(new ActionItem(surroundingAction));
                            }
                            else
                            {
                                actionItem.actionItemsAfterFreeText.Add(new ActionItem(surroundingAction));
                            }
                        }
                    }
                    return true;
                }
            }

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
        // note this is an array only because we may have multiple presses of the same key
        public Tuple<VirtualKeyCode?, VirtualKeyCode>[] keyCodes;
        public String actionText;
        public String freeText;
        //public String extendedType;
        //public String extendedTypeTextParam;
        public Boolean allowFreeText;

        public HashSet<SessionType> applicableSessionTypes = null;
        public int waitTime = -1;
        public int holdTime = -1;
        public int fixedRepeatCount = 1; // a fixed number of repeated presses specified in the macro
        public bool repeatCountFromVoiceCommand = false;
        public string eventToResolveRepeatCount = null;

        // these are used when playing free test macros
        public List<ActionItem> actionItemsBeforeFreeText = new List<ActionItem>();
        public List<ActionItem> actionItemsAfterFreeText = new List<ActionItem>();

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
                    String[] typeAndParamBlocks = action.Substring(start, end - start).Split('|');
                    foreach (string typeAndParamBlock in typeAndParamBlocks)
                    {
                        String[] typeAndParam = typeAndParamBlock.Split(',');
                        if (typeAndParam.Length == 1 && MacroManager.FREE_TEXT_IDENTIFIER.Equals(typeAndParam[0]))
                        {
                            allowFreeText = true;
                            parsedSuccessfully = true;
                        }
                        else if (typeAndParam.Length > 1 && MacroManager.APPLICABLE_SESSION_TYPES_IDENTIFIER.Equals(typeAndParam[0]))
                        {
                            HashSet<SessionType> parsedSessionTypes = new HashSet<SessionType>();
                            for (int i = 1; i < typeAndParam.Length; i++)
                            {
                                if (Enum.TryParse(typeAndParam[i].Trim(), out SessionType parsedSessionType))
                                {
                                    parsedSessionTypes.Add(parsedSessionType);
                                }
                            }
                            if (parsedSessionTypes.Count > 0)
                            {
                                this.applicableSessionTypes = parsedSessionTypes;
                                parsedSuccessfully = true;
                            }
                        }
                        else if (typeAndParam.Length == 2)
                        {
                            if (MacroManager.WAIT_IDENTIFIER.Equals(typeAndParam[0]))
                            {
                                if (int.TryParse(typeAndParam[1], out int millis))
                                {
                                    this.waitTime = millis;
                                    parsedSuccessfully = true;
                                }
                            }
                            else if (MacroManager.HOLD_TIME_IDENTIFIER.Equals(typeAndParam[0]))
                            {
                                if (int.TryParse(typeAndParam[1], out int millis))
                                {
                                    this.holdTime = millis;
                                    parsedSuccessfully = true;
                                }
                            }
                            else if (MacroManager.MULTIPLE_PRESS_IDENTIFIER.Equals(typeAndParam[0]))
                            {
                                if (MacroManager.MULTIPLE_PRESS_FROM_VOICE_TRIGGER_IDENTIFIER.Equals(typeAndParam[1]))
                                {
                                    this.repeatCountFromVoiceCommand = true;
                                    parsedSuccessfully = true;
                                }
                                else if (typeAndParam[1].All(char.IsDigit))
                                {
                                    this.fixedRepeatCount = int.Parse(typeAndParam[1]);
                                    parsedSuccessfully = true;
                                }
                                else
                                {
                                    this.eventToResolveRepeatCount = typeAndParam[1].Trim();
                                    parsedSuccessfully = true;
                                }
                            }
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
                    this.keyCodes = new Tuple<VirtualKeyCode?, VirtualKeyCode>[1];
                    // try and get it directly without going through the key bindings
                    parsedSuccessfully = KeyPresser.parseKeycode(action, out this.keyCodes[0]);
                    if (!parsedSuccessfully)
                    {
                        if (allowFreeText)
                        {
                            parsedSuccessfully = true;
                            this.freeText = action;
                            if (parsedSuccessfully)
                            {
                                Console.WriteLine("Free text action macro, text = " + freeText);
                            }
                        }
                        else
                        {
                            Console.WriteLine("actionItem = \"" + action + "\" not recognised");
                            parsedSuccessfully = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing action " + action + ", message:" + e.Message + " stackTrace: " + e.StackTrace);
                    parsedSuccessfully = false;
                }
            }
        }

        public override String ToString()
        {
            if (parsedSuccessfully)
            {
                string str = "Raw actionItem: " + actionText + ". Extracted data: ";
                if (this.waitTime > 0)
                {
                    str += "wait " + waitTime + "ms ";
                }
                if (this.repeatCountFromVoiceCommand)
                {
                    str += "repeat count from voice command ";
                }
                if (this.fixedRepeatCount > 1)
                {
                    str += "repeated " + this.fixedRepeatCount + " times ";
                }
                if (this.eventToResolveRepeatCount != null)
                {
                    str += "repeat count from event " + this.eventToResolveRepeatCount + " ";
                }
                if (this.applicableSessionTypes != null)
                {
                    str += "applicable session types " + String.Join(",", this.applicableSessionTypes) + " ";
                }
                if (this.allowFreeText)
                {
                    str += "free text " + this.freeText + " ";
                }
                if (keyCodes != null)
                {
                    bool addComma = false;
                    str += ". Keys: ";
                    foreach (Tuple<VirtualKeyCode?, VirtualKeyCode> keyCode in keyCodes)
                    {
                        if (addComma)
                        {
                            str += ", ";
                        }
                        if (keyCode.Item1 != null)
                        {
                            str += keyCode.Item1.ToString() + "+";
                        }
                        str += keyCode.Item2.ToString();
                        addComma = true;
                    }
                }
                return str;
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
