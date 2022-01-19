using CrewChiefV4.Audio;
using CrewChiefV4.commands;
using CrewChiefV4.GameState;
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
    public Int32 Body;
    public Int32 FrontWing;
    public Int32 RearWing;
    note that Suspension is no longer in the data - Body got added after RearTires, the wing elements got bumped along and the suspension element bumped out

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
        Body = 6,
        Frontwing = 7,
        Rearwing = 8,
        Suspension = 9,
        RequestPit = 10,

        // Pit menu nothing selected
        Max = 11
    }

    public class R3EPitMenuManager
    {
        public static Boolean outstandingPitstopRequest = false;
        public static DateTime timeWeCanAnnouncePitActions = DateTime.MinValue;

        // as we're not pressing loads of buttons here, sleep a while between key presses
        private const int DEFAULT_SLEEP_AFTER_BUTTON_PRESS = 400;
        private const int SLEEP_AFTER_SEARCH_BUTTON_PRESS = 200;    // shorter sleep while we're whizzing through the menu looking for items
        private const int MENU_SCROLL_LIMIT = 8;

        private const String TOGGLE_PIT_MENU_MACRO_NAME = "open / close pit menu";
        private const String PIT_MENU_UP_MACRO_NAME = "pit menu up";
        private const String PIT_MENU_DOWN_MACRO_NAME = "pit menu down";
        private const String PIT_MENU_SELECT_MACRO_NAME = "pit menu select";
        private const String PIT_MENU_RIGHT_MACRO_NAME = "pit menu right";
        private const String PIT_MENU_LEFT_MACRO_NAME = "pit menu left";

        private const string folderConfirmAllTyres = "mandatory_pit_stops/confirm_change_all_tyres";
        private const string folderConfirmFrontTyres = "mandatory_pit_stops/confirm_change_front_tyres";
        private const string folderConfirmRearTyres = "mandatory_pit_stops/confirm_change_rear_tyres";
        private const string folderConfirmNoTyres = "mandatory_pit_stops/confirm_change_no_tyres";
        private const string folderConfirmFixBody = "mandatory_pit_stops/confirm_fix_body";
        private const string folderConfirmFixAllAero = "mandatory_pit_stops/confirm_fix_all_aero";
        private const string folderConfirmFixFrontAero = "mandatory_pit_stops/confirm_fix_front_aero";
        private const string folderConfirmFixRearAero = "mandatory_pit_stops/confirm_fix_rear_aero";
        private const string folderConfirmDontFixAero = "mandatory_pit_stops/confirm_dont_fix_aero";
        private const string folderConfirmFixSuspension = "mandatory_pit_stops/confirm_fix_suspension";
        private const string folderConfirmDontFixSuspension = "mandatory_pit_stops/confirm_dont_fix_suspension";
        private const string folderConfirmRefuelling = "mandatory_pit_stops/confirm_refuelling";
        private const string folderConfirmNoRefuelling = "mandatory_pit_stops/confirm_no_refuelling";

        // tyre compound responses
        private const string folderConfirmSoftTyres = "mandatory_pit_stops/confirm_soft_tyres";
        private const string folderConfirmMediumTyres = "mandatory_pit_stops/confirm_medium_tyres";
        private const string folderConfirmHardTyres = "mandatory_pit_stops/confirm_hard_tyres";
        private const string folderConfirmPrimeTyres = "mandatory_pit_stops/confirm_prime_tyres";
        private const string folderConfirmOptionTyres = "mandatory_pit_stops/confirm_option_tyres";
        private const string folderConfirmAlternateTyres = "mandatory_pit_stops/confirm_alternate_tyres";
        private const string folderRequestedTyreNotAvailable = "mandatory_pit_stops/confirm_requested_tyre_not_available";

        // lazily initialised
        private static ExecutableCommandMacro menuToggleMacro;
        private static ExecutableCommandMacro menuDownMacro;
        private static ExecutableCommandMacro menuUpMacro;
        private static ExecutableCommandMacro menuSelectMacro;
        private static ExecutableCommandMacro menuRightMacro;
        private static ExecutableCommandMacro menuLeftMacro;

        private static SelectedItem selectedItem;
        private static PitMenuState state;
        private static int pitState;

        private static Object myLock = new Object();

        // set to false at the start of every session to avoid us using stale pit menu data. A bit flaky...
        public static Boolean hasStateForCurrentSession = false;
        public static Dictionary<SelectedItem, PitSelectionState> latestState = new Dictionary<SelectedItem, PitSelectionState>();

        private static Thread executeThread = null;

        // per-car tyre options, the array of TyreType is in the order it appears in the pit menu
        private static Dictionary<CarData.CarClassEnum, TyreType[]> tyreOptions = new Dictionary<CarData.CarClassEnum, TyreType[]>
        {
            { CarData.CarClassEnum.F1_90S, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} },
            { CarData.CarClassEnum.F1, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} },
            { CarData.CarClassEnum.DTM_92, new TyreType[]{ TyreType.Soft, TyreType.Hard} },
            { CarData.CarClassEnum.INDYCAR, new TyreType[]{ TyreType.Alternate, TyreType.Prime} },  // note we use Prime here, not Primary - the SRE recognises either
            { CarData.CarClassEnum.GROUPC, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} },
            { CarData.CarClassEnum.GT2, new TyreType[]{ TyreType.Soft, TyreType.Hard} },
            { CarData.CarClassEnum.GROUP5, new TyreType[]{ TyreType.Soft, TyreType.Hard} },
            { CarData.CarClassEnum.M1_PROCAR, new TyreType[]{ TyreType.Soft, TyreType.Hard} },
            { CarData.CarClassEnum.GTE, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} },
            { CarData.CarClassEnum.HILL_CLIMB_ICONS, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} },
            { CarData.CarClassEnum.GROUPA, new TyreType[]{ TyreType.Soft, TyreType.Hard} },
            { CarData.CarClassEnum.GTO, new TyreType[]{ TyreType.Soft, TyreType.Medium, TyreType.Hard} }
        };

        private static HashSet<CarData.CarClassEnum> classesAllowingMismatchedTyres = new HashSet<CarData.CarClassEnum>
        {
            CarData.CarClassEnum.F1_90S, CarData.CarClassEnum.DTM_92, CarData.CarClassEnum.GTO,
            CarData.CarClassEnum.GROUPC, CarData.CarClassEnum.GT2, CarData.CarClassEnum.GROUP5, CarData.CarClassEnum.M1_PROCAR,
            CarData.CarClassEnum.GTE, CarData.CarClassEnum.HILL_CLIMB_ICONS, CarData.CarClassEnum.GROUPA
        };

        static R3EPitMenuManager()
        {
            latestState.Add(SelectedItem.Driverchange, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Fronttires, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Reartires, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Body, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Frontwing, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Rearwing, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Suspension, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Fuel, PitSelectionState.UNKNOWN);
            latestState.Add(SelectedItem.Penalty, PitSelectionState.UNKNOWN);
        }

        // called by the mapper on every tick
        // pitState is -1 = N/A, 0 = None, 1 = Requested stop, 2 = Entered pitlane heading for pitspot, 3 = Stopped at pitspot, 4 = Exiting pitspot heading for pit exit
        public static void map(Int32 pitMenuSelection, PitMenuState state, int pitState)
        {
            R3EPitMenuManager.selectedItem = (SelectedItem) pitMenuSelection;
            R3EPitMenuManager.state = state;
            R3EPitMenuManager.pitState = pitState;
            if (state.RequestPit != -1 || state.Suspension != -1 || state.Body != -1 || state.Driverchange != -1 || state.FrontTires != -1 || state.FrontWing != -1
                || state.Fuel != -1 || state.Penalty != -1 || state.Preset != -1 || state.RearTires != -1 || state.RearWing != -1)
            {
                // one of the buttons is available so the menu must be open - snapshot its state
                hasStateForCurrentSession = true;
                latestState[SelectedItem.Driverchange] = getDriverchangeState();
                latestState[SelectedItem.Fronttires] = getChangeFrontTyresState();
                latestState[SelectedItem.Reartires] = getChangeRearTyresState();
                latestState[SelectedItem.Body] = getFixBodyState();
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
                    if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_BODY))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmFixBody, 0));
                        fixBody();
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
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_ALL))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        fixBody();
                        fixAllAero();
                        selectFixSuspension();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_FIX_NONE))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        fixNoAero();
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
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_NEXT_TYRE_COMPOUND))
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        selectNextTyreCompound();
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_SOFT_TYRES))
                    {
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Soft, audioPlayer);
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_MEDIUM_TYRES))
                    {
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Medium, audioPlayer);
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_HARD_TYRES))
                    {
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Hard, audioPlayer);
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_ALTERNATE_TYRES))
                    {
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Alternate, audioPlayer);
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_PRIME_TYRES))
                    {
                        // special case for prime - this is primary (indycar) OR prime (dtm2014)
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Prime, audioPlayer);
                    }
                    else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PIT_STOP_OPTION_TYRES))
                    {
                        // don't play the ack here - let the method call work it out
                        selectTyreCompound(TyreType.Option, audioPlayer);
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
        public static Boolean fixBody(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Body, true);
                success = getFixBodyState() == PitSelectionState.SELECTED;
                success = true;
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
                setItemToOnOrOff(SelectedItem.Body, true);
                // success if any of the aero fixes are selected - maybe only 1 or 2 are available
                success = getFixFrontAeroState() == PitSelectionState.SELECTED || getFixRearAeroState() == PitSelectionState.SELECTED || getFixBodyState() == PitSelectionState.SELECTED;
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
                setItemToOnOrOff(SelectedItem.Body, false);
                success = getFixFrontAeroState() != PitSelectionState.SELECTED && getFixRearAeroState() != PitSelectionState.SELECTED && getFixBodyState() != PitSelectionState.SELECTED;
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
        public static Boolean selectNextTyreCompound(Boolean closeAfterSetting = true)
        {
            Boolean success = false;
            if (openPitMenuIfClosed())
            {
                setItemToOnOrOff(SelectedItem.Fronttires, false);
                setItemToOnOrOff(SelectedItem.Reartires, false);
                // here we're assuming that changing the rear compound wil also change the front. This is 
                // probably OK at the moment - they're tied together for the classes with multiple compounds 
                // and the menu enforces this.
                goToMenuItem(SelectedItem.Reartires);
                ExecutableCommandMacro rightMacro = getMenuRightMacro();
                if (rightMacro != null)
                {
                    executeMacro(rightMacro);
                }
                setItemToOnOrOff(SelectedItem.Reartires, true);
                setItemToOnOrOff(SelectedItem.Fronttires, true);
            }
            if (closeAfterSetting)
            {
                closePitMenuIfOpen();
            }
            return success;
        }

        public static void selectTyreCompound(TyreType tyreType, AudioPlayer audioPlayer, Boolean closeAfterSetting = true)
        {
            if (!tyreOptions.Keys.Contains(CrewChief.carClass) || !tyreOptions[CrewChief.carClass].Contains(tyreType))
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(folderRequestedTyreNotAvailable, 0));
            }
            else
            {
                switch (tyreType)
                {
                    case TyreType.Soft:
                        if (SoundCache.hasSingleSound(folderConfirmSoftTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmSoftTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    case TyreType.Medium:
                        if (SoundCache.hasSingleSound(folderConfirmMediumTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmMediumTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    case TyreType.Hard:
                        if (SoundCache.hasSingleSound(folderConfirmHardTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmHardTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    case TyreType.Prime:
                        if (SoundCache.hasSingleSound(folderConfirmPrimeTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmPrimeTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    case TyreType.Option:
                        if (SoundCache.hasSingleSound(folderConfirmOptionTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmOptionTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    case TyreType.Alternate:
                        if (SoundCache.hasSingleSound(folderConfirmAlternateTyres))
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmAlternateTyres, 0));
                        else
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                    default:
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                        break;
                }

                if (openPitMenuIfClosed())
                {
                    setItemToOnOrOff(SelectedItem.Fronttires, false);
                    setItemToOnOrOff(SelectedItem.Reartires, false);
                    // some car classes allow mismatched tyres
                    goToMenuItem(SelectedItem.Reartires);
                    setSelectedTyres(tyreType);
                    if (classesAllowingMismatchedTyres.Contains(CrewChief.carClass))
                    {
                        goToMenuItem(SelectedItem.Fronttires);
                        setSelectedTyres(tyreType);
                    }
                    setItemToOnOrOff(SelectedItem.Fronttires, true);
                    setItemToOnOrOff(SelectedItem.Reartires, true);
                }
                if (closeAfterSetting)
                {
                    // wait a couple of seconds before closing
                    Thread.Sleep(2000);
                    closePitMenuIfOpen();
                }
            }
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
            Console.WriteLine("attempting to set state of item " + item + " from " + currentState + " to " + requiredStateInt);
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
                if (selectMacro != null)
                {
                    executeMacro(selectMacro);
                }
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
            Console.WriteLine("attempting to go to menu item " + selectedItem + " state for this item is " + R3EPitMenuManager.getStateForItem(selectedItem) + " current item = " + selectedItem);
            if (menuIsOpen() && R3EPitMenuManager.getStateForItem(selectedItem) != -1)
            {
                ExecutableCommandMacro downMacro = getMenuDownMacro();
                if (downMacro != null)
                {
                    int count = 0;
                    while (R3EPitMenuManager.selectedItem != selectedItem && count < R3EPitMenuManager.MENU_SCROLL_LIMIT)
                    {
                        executeMacro(downMacro, R3EPitMenuManager.SLEEP_AFTER_SEARCH_BUTTON_PRESS);
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
                            executeMacro(upMacro, R3EPitMenuManager.SLEEP_AFTER_SEARCH_BUTTON_PRESS);
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
            // based on pitstate flag first:
            if (R3EPitMenuManager.pitState == 1)
            {
                return PitSelectionState.SELECTED;
            }

            // pit menu states are broken. Hopefully this will be fixed in-game, for now interpret the button states
            // based on the current incorrect behaviour:
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.RequestPit == -1) // request pit is replaced with cancel pit request
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.RequestPit == 1) // feels like this should be 0 if it's available but not selected
            {
                return PitSelectionState.AVAILABLE;
            }

            // 'Correct' button state behaviour prior to the addition of 'fix suspension' menu item, which has broken
            // the button state logic
            // requested => bottom button available (which is 'cancel')
            /*
            if (R3EPitMenuManager.state.ButtonBottom == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.ButtonTop == 1)
            {
                return PitSelectionState.AVAILABLE;
            }*/
            return PitSelectionState.UNAVAILABLE;
        }

        public static PitSelectionState getFixBodyState()
        {
            if (!R3EPitMenuManager.menuIsOpen())
            {
                return PitSelectionState.UNKNOWN;
            }
            if (R3EPitMenuManager.state.Body == 1)
            {
                return PitSelectionState.SELECTED;
            }
            if (R3EPitMenuManager.state.Body == 0)
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
                case SelectedItem.RequestPit:
                    state = R3EPitMenuManager.state.RequestPit;
                    break;
                case SelectedItem.Suspension:
                    state = R3EPitMenuManager.state.Suspension;
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
                case SelectedItem.Body:
                    state = R3EPitMenuManager.state.Body;
                    break;
                // hopefully we can re-instate this
                // case SelectedItem.Suspension:
                //     state = R3EPitMenuManager.state.Suspension;
                //     break;
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

        private static ExecutableCommandMacro getMenuRightMacro()
        {
            if (R3EPitMenuManager.menuRightMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_RIGHT_MACRO_NAME, out R3EPitMenuManager.menuRightMacro);
            }
            return R3EPitMenuManager.menuRightMacro;
        }

        private static ExecutableCommandMacro getMenuLeftMacro()
        {
            if (R3EPitMenuManager.menuLeftMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_LEFT_MACRO_NAME, out R3EPitMenuManager.menuLeftMacro);
            }
            return R3EPitMenuManager.menuLeftMacro;
        }

        private static void setSelectedTyres(TyreType tyreType)
        {
            int resetCount = tyreOptions[CrewChief.carClass].Length - 1;
            int selectCount = Array.IndexOf(tyreOptions[CrewChief.carClass], tyreType);
            ExecutableCommandMacro leftMacro = getMenuLeftMacro();
            ExecutableCommandMacro rightMacro = getMenuRightMacro();
            if (leftMacro != null && rightMacro != null)
            {
                for (int i = 0; i < resetCount; i++)
                    executeMacro(leftMacro);
                for (int i = 0; i < selectCount; i++)
                    executeMacro(rightMacro);
            }
        }
    }
}
