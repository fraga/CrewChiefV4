﻿using CrewChiefV4.Audio;
using CrewChiefV4.commands;
using CrewChiefV4.RaceRoom.RaceRoomData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrewChiefV4.R3E
{    
    /*
     * 
     * Behaviour:
     * SelectedItem is one of the SelectedItem enum. 
     * 
     * 
    // pit menu state:
    /*
    public Int32 Preset;

    // Pit menu actions
    public Int32 Penalty;
    public Int32 Driverchange;
    public Int32 Fuel;
    public Int32 FrontTires;
    public Int32 RearTires;
    public Int32 FrontWing;
    public Int32 RearWing;
    public Int32 Suspension;

    // Pit menu buttons
    public Int32 ButtonTop;
    public Int32 ButtonBottom;
    */

    public enum PitSelectionState
    {
        UNAVAILABLE, AVAILABLE, SELECTED, UNKNOWN
    }

    // mapped directly to game data
    public enum SelectedItem
    {
        Unavailable = -1,

        // Pit menu preset
        Preset = 0,

        // Pit menu actions
        Penalty = 1,
        Driverchange = 2,
        Fuel = 3,
        Fronttires = 4,
        Reartires = 5,
        Frontwing = 6,
        Rearwing = 7,
        Suspension = 8,

        // Pit menu buttons
        ButtonTop = 9,
        ButtonBottom = 10,

        // Pit menu nothing selected
        Max = 11
    }

    public class R3EPitMenuManager
    {
        public static Boolean outstandingPitstopRequest = false;
        public static DateTime timeWeCanAnnouncePitActions = DateTime.MinValue;

        // as we're not pressing loads of buttons here, sleep a while between key presses
        private const int DEFAULT_SLEEP_AFTER_BUTTON_PRESS = 400;
        private const int MENU_SCROLL_LIMIT = 8;

        private const String TOGGLE_PIT_MENU_MACRO_NAME = "open / close pit menu";
        private const String PIT_MENU_UP_MACRO_NAME = "pit menu up";
        private const String PIT_MENU_DOWN_MACRO_NAME = "pit menu down";
        private const String PIT_MENU_SELECT_MACRO_NAME = "pit menu select";

        private const string folderConfirmAllTyres = "mandatory_pit_stops/confirm_change_all_tyres";
        private const string folderConfirmFrontTyres = "mandatory_pit_stops/confirm_change_front_tyres";
        private const string folderConfirmRearTyres = "mandatory_pit_stops/confirm_change_rear_tyres";
        private const string folderConfirmNoTyres = "mandatory_pit_stops/confirm_change_no_tyres";
        private const string folderConfirmFixAllAero = "mandatory_pit_stops/confirm_fix_all_aero";
        private const string folderConfirmFixFrontAero = "mandatory_pit_stops/confirm_fix_front_aero";
        private const string folderConfirmFixRearAero = "mandatory_pit_stops/confirm_fix_rear_aero";
        private const string folderConfirmDontFixAero = "mandatory_pit_stops/confirm_dont_fix_aero";
        private const string folderConfirmFixSuspension = "mandatory_pit_stops/confirm_fix_suspension";
        private const string folderConfirmDontFixSuspension = "mandatory_pit_stops/confirm_dont_fix_suspension";
        private const string folderConfirmRefuelling = "mandatory_pit_stops/confirm_refuelling";
        private const string folderConfirmNoRefuelling = "mandatory_pit_stops/confirm_no_refuelling";


        // lazily initialised
        private static ExecutableCommandMacro menuToggleMacro;
        private static ExecutableCommandMacro menuDownMacro;
        private static ExecutableCommandMacro menuUpMacro;
        private static ExecutableCommandMacro menuSelectMacro;

        private static SelectedItem selectedItem;
        private static PitMenuState state;
        
        private static Object myLock = new Object();

        // set to false at the start of every session to avoid us using stale pit menu data. A bit flaky...
        public static Boolean hasStateForCurrentSession = false;
        public static Dictionary<SelectedItem, PitSelectionState> latestState = new Dictionary<SelectedItem, PitSelectionState>();

        private static Thread executeThread = null;

        static R3EPitMenuManager()
        {
            latestState.Add(SelectedItem.Driverchange, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Fronttires, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Reartires, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Frontwing, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Rearwing, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Suspension, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Fuel, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Penalty, PitSelectionState.UNKNOWN);
        }

        // called by the mapper on every tick
        public static void map(Int32 pitMenuSelection, PitMenuState state)
        {
            R3EPitMenuManager.selectedItem = (SelectedItem) pitMenuSelection;
            R3EPitMenuManager.state = state;
            if (state.ButtonBottom != -1 || state.ButtonTop != -1)
            {
                // one of the buttons is available so the menu must be open - snapshot its state
                hasStateForCurrentSession = true;
                latestState[SelectedItem.Driverchange] = getDriverchangeState();
                latestState[SelectedItem.Fronttires] = getChangeFrontTyresState();
                latestState[SelectedItem.Reartires] = getChangeRearTyresState();
                latestState[SelectedItem.Frontwing] = getFixFrontAeroState();
                latestState[SelectedItem.Rearwing] = getFixRearAeroState();
                latestState[SelectedItem.Suspension] = getFixSuspensionState();
                latestState[SelectedItem.Fuel] = getRefuelState();
                latestState[SelectedItem.Penalty] = getServePenaltyState();
            }
        }

        public static void processVoiceCommand(String voiceMessage, AudioPlayer audioPlayer)
        {
            lock (myLock)
            {
                // run this in a new thread as it may take a while to complete its work
                ThreadManager.UnregisterTemporaryThread(executeThread);
                executeThread = new Thread(() =>
                {
                    if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_CHANGE_ALL_TYRES))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmAllTyres, 0));
                        changeAllTyres();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_CHANGE_FRONT_TYRES))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFrontTyres, 0));
                        changeFrontTyresOnly();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_CHANGE_REAR_TYRES))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmRearTyres, 0));
                        changeRearTyresOnly();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_CLEAR_TYRES))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmNoTyres, 0));
                        changeNoTyres();
                    }
                    if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_FRONT_AERO))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFixFrontAero, 0));
                        fixFrontAeroOnly();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_REAR_AERO))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFixRearAero, 0));
                        fixRearAeroOnly();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_ALL_AERO))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFixAllAero, 0));
                        fixAllAero();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_NO_AERO))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmDontFixAero, 0));
                        fixNoAero();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_SUSPENSION))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFixSuspension, 0));
                        selectFixSuspension();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_DONT_FIX_SUSPENSION))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmDontFixSuspension, 0));
                        unselectFixSuspension();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_SERVE_PENALTY))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        selectServePenalty();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_DONT_SERVE_PENALTY))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        unselectServePenalty();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_REFUEL))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmRefuelling, 0));
                        selectFuel();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_DONT_REFUEL))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmNoRefuelling, 0));
                        unselectFuel();
                    }
                });
                executeThread.Name = "R3EPitMenuManager.executeThread";
                ThreadManager.RegisterTemporaryThread(executeThread);
                executeThread.Start();
            }
        }

        public static Boolean hasRequestedPitStop()
        {
            openPitMenuIfClosed(200);
            PitSelectionState state = getPitRequestState();
            closePitMenuIfOpen(200);
            return state == PitSelectionState.SELECTED;
        }

        // convenience methods to do non-trivial pit stuff that needs to know the state of the menu
        public static Boolean changeFrontTyresOnly(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fronttires, true);
                setItemToOnOrOff(SelectedItem.Reartires, false);
                success = getChangeFrontTyresState() == PitSelectionState.SELECTED && getChangeRearTyresState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean changeRearTyresOnly(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fronttires, false);
                setItemToOnOrOff(SelectedItem.Reartires, true);
                success = getChangeFrontTyresState() != PitSelectionState.SELECTED && getChangeRearTyresState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean changeAllTyres(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fronttires, true);
                setItemToOnOrOff(SelectedItem.Reartires, true);
                success = getChangeFrontTyresState() == PitSelectionState.SELECTED && getChangeRearTyresState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean changeNoTyres(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fronttires, false);
                setItemToOnOrOff(SelectedItem.Reartires, false);
                success = getChangeFrontTyresState() != PitSelectionState.SELECTED && getChangeRearTyresState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean fixFrontAeroOnly(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Frontwing, true);
                setItemToOnOrOff(SelectedItem.Rearwing, false);
                success = getFixFrontAeroState() == PitSelectionState.SELECTED && getFixRearAeroState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean fixRearAeroOnly(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Frontwing, false);
                setItemToOnOrOff(SelectedItem.Rearwing, true);
                success = getFixFrontAeroState() != PitSelectionState.SELECTED && getFixRearAeroState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean fixAllAero(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Frontwing, true);
                setItemToOnOrOff(SelectedItem.Rearwing, true);
                success = getFixFrontAeroState() == PitSelectionState.SELECTED && getFixRearAeroState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean fixNoAero(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Frontwing, false);
                setItemToOnOrOff(SelectedItem.Rearwing, false);
                success = getFixFrontAeroState() != PitSelectionState.SELECTED && getFixRearAeroState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean selectFixSuspension(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Suspension, true);
                success = getFixSuspensionState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean unselectFixSuspension(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Suspension, false);
                success = getFixSuspensionState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean selectServePenalty(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Penalty, true);
                success = getServePenaltyState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean unselectServePenalty(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Penalty, false);
                success = getServePenaltyState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean selectFuel(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fuel, true);
                success = getRefuelState() == PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }
        public static Boolean unselectFuel(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fuel, false);
                success = getRefuelState() != PitSelectionState.SELECTED;
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }

        // opens the pit menu so we can get information. IMPORTANT: this executes the macro which (obviously) has to be wired up properly.
        // MORE IMPORTANT: This makes pit menu state avaiable ONLY ON THE NEXT TICK. 
        public static Boolean openPitMenuIfClosed(int sleepAfter = R3EPitMenuManager.DEFAULT_SLEEP_AFTER_BUTTON_PRESS)
        {
            Console.WriteLine("Opening menu - selected item is " + R3EPitMenuManager.selectedItem + " isOpen = " + R3EPitMenuManager.menuIsOpen());
            ExecutableCommandMacro macro = R3EPitMenuManager.getMenuToggleMacro();
            if (macro != null && !R3EPitMenuManager.menuIsOpen())
            {
                executeMacro(macro, sleepAfter);
            }
            return R3EPitMenuManager.menuIsOpen();
        }

        public static Boolean closePitMenuIfOpen(int sleepAfter = R3EPitMenuManager.DEFAULT_SLEEP_AFTER_BUTTON_PRESS)
        {
            Console.WriteLine("Closing menu - selected item is " + R3EPitMenuManager.selectedItem + " isOpen = " + R3EPitMenuManager.menuIsOpen());
            ExecutableCommandMacro macro = R3EPitMenuManager.getMenuToggleMacro();
            if (macro != null && R3EPitMenuManager.menuIsOpen())
            {
                executeMacro(macro, sleepAfter);
            }
            return R3EPitMenuManager.menuIsOpen();
        }

        public static Boolean menuIsOpen()
        {
            return R3EPitMenuManager.selectedItem != SelectedItem.Unavailable;
        }

        public static Boolean setItemToOnOrOff(SelectedItem item, Boolean requiredState)
        {
            int currentState = getStateForItem(item);
            int requiredStateInt = requiredState ? 1 : 0;
            Console.WriteLine("attempting to set state of item " + item + " from " + currentState + " to " + requiredState);
            if (currentState == -1)
            {
                return false;
            }
            if (requiredStateInt == currentState)
            {
                return true;
            }
            else
            {
                // try and set the state
                goToMenuItem(item);
                ExecutableCommandMacro selectMacro = getMenuSelectMacro();
                executeMacro(selectMacro);
                int newState = getStateForItem(item);         
                return newState == requiredStateInt;            
            }
        }

        private static void executeMacro(ExecutableCommandMacro macro, int sleepAfter = R3EPitMenuManager.DEFAULT_SLEEP_AFTER_BUTTON_PRESS)
        {
            // suppress macro confirmation messages, and run the macro on the caller's thread:
            macro.execute(null, true, false);
            Thread.Sleep(sleepAfter);
        }
                
        public static Boolean goToMenuItem(SelectedItem selectedItem)
        {
            Console.WriteLine("attempting to go to menu item " + selectedItem);
            if (menuIsOpen() && R3EPitMenuManager.getStateForItem(selectedItem) != -1)
            {
                ExecutableCommandMacro downMacro = getMenuDownMacro();
                if (downMacro != null)
                {
                    int count = 0;
                    while (R3EPitMenuManager.selectedItem != selectedItem && count < R3EPitMenuManager.MENU_SCROLL_LIMIT)
                    {
                        executeMacro(downMacro);
                        count++;
                    }
                }
                if (R3EPitMenuManager.selectedItem == selectedItem)
                {
                    return true;
                }
                else
                {
                    ExecutableCommandMacro upMacro = getMenuUpMacro();
                    if (upMacro != null)
                    {
                        int count = 0;
                        while (R3EPitMenuManager.selectedItem != selectedItem && count < R3EPitMenuManager.MENU_SCROLL_LIMIT)
                        {
                            executeMacro(upMacro);
                            count++;
                        }
                    }
                    if (R3EPitMenuManager.selectedItem == selectedItem)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static PitSelectionState getPitRequestState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            // requested => bottom button available (which is 'cancel')
            if (R3EPitMenuManager.state.ButtonBottom == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.ButtonTop == 1)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getFixFrontAeroState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.FrontWing == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.FrontWing == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getFixRearAeroState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.RearWing == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.RearWing == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getChangeFrontTyresState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.FrontTires == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.FrontTires == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getChangeRearTyresState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.RearTires == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.RearTires == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getFixSuspensionState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.Suspension == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.Suspension == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getServePenaltyState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.Penalty == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.Penalty == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getRefuelState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.Fuel == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.Fuel == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }

        // don't think this is used:
        public static PitSelectionState getDriverchangeState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.Driverchange == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.Driverchange == 0)
            {
                return PitSelectionState.AVAILABLE;
            }
            return PitSelectionState.UNAVAILABLE;
        }
                
        // utility stuff
        private static int getStateForItem(SelectedItem selectedItem)
        {
            int state = -1;
            switch (selectedItem)
            {
                case SelectedItem.ButtonBottom:
                    state = R3EPitMenuManager.state.ButtonTop;
                    break;
                case SelectedItem.ButtonTop:
                    state = R3EPitMenuManager.state.ButtonBottom;
                    break;
                case SelectedItem.Driverchange:
                    state = R3EPitMenuManager.state.Driverchange;
                    break;
                case SelectedItem.Fronttires:
                    state = R3EPitMenuManager.state.FrontTires;
                    break;
                case SelectedItem.Frontwing:
                    state = R3EPitMenuManager.state.FrontWing;
                    break;
                case SelectedItem.Fuel:
                    state = R3EPitMenuManager.state.Fuel;
                    break;
                case SelectedItem.Penalty:
                    state = R3EPitMenuManager.state.Penalty;
                    break;
                case SelectedItem.Preset:
                    state = R3EPitMenuManager.state.Preset;
                    break;
                case SelectedItem.Reartires:
                    state = R3EPitMenuManager.state.RearTires;
                    break;
                case SelectedItem.Rearwing:
                    state = R3EPitMenuManager.state.RearWing;
                    break;
                case SelectedItem.Suspension:
                    state = R3EPitMenuManager.state.Suspension;
                    break;
                default:
                    break;
            }
            Console.WriteLine("State for item " + selectedItem + " = " + state);
            return state;
        }
        
        private static ExecutableCommandMacro getMenuToggleMacro()
        {
            if (R3EPitMenuManager.menuToggleMacro == null)
            {
                MacroManager.macros.TryGetValue(TOGGLE_PIT_MENU_MACRO_NAME, out R3EPitMenuManager.menuToggleMacro);
            }
            return R3EPitMenuManager.menuToggleMacro;
        }

        private static ExecutableCommandMacro getMenuSelectMacro()
        {
            if (R3EPitMenuManager.menuSelectMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_SELECT_MACRO_NAME, out R3EPitMenuManager.menuSelectMacro);
            }
            return R3EPitMenuManager.menuSelectMacro;
        }

        private static ExecutableCommandMacro getMenuDownMacro()
        {
            if (R3EPitMenuManager.menuDownMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_DOWN_MACRO_NAME, out R3EPitMenuManager.menuDownMacro);
            }
            return R3EPitMenuManager.menuDownMacro;
        }

        private static ExecutableCommandMacro getMenuUpMacro()
        {
            if (R3EPitMenuManager.menuUpMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_UP_MACRO_NAME, out R3EPitMenuManager.menuUpMacro);
            }
            return R3EPitMenuManager.menuUpMacro;
        }
    }
}
