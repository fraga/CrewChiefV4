using CrewChiefV4.Audio;
using CrewChiefV4.commands;
using System;
using System.Threading;
using WindowsInput.Native;

namespace CrewChiefV4.ACC
{
    // functions to fart around with ACC's pit menu
    class ACCPitMenuManager
    {
        // hard-coded keys to fall back on if we don't find the menu navigation macro definitions
        private static VirtualKeyCode upKey = VirtualKeyCode.UP;
        private static VirtualKeyCode downKey = VirtualKeyCode.DOWN;
        private static VirtualKeyCode leftKey = VirtualKeyCode.LEFT;
        private static VirtualKeyCode rightKey = VirtualKeyCode.RIGHT;
        private static VirtualKeyCode pitMenuKey = VirtualKeyCode.VK_P;
        private static VirtualKeyCode standingsMenuKey = VirtualKeyCode.VK_U;

        // lazily initialised
        private static ExecutableCommandMacro pitMenuMacro;
        private static ExecutableCommandMacro standingsMenuMacro;
        private static ExecutableCommandMacro menuUpMacro;
        private static ExecutableCommandMacro menuDownMacro;
        private static ExecutableCommandMacro menuRightMacro;
        private static ExecutableCommandMacro menuLeftMacro;

        private const String PIT_MENU_MACRO_NAME = "show acc pit menu";
        private const String STANDINGS_MACRO_NAME = "show acc standings menu";
        private const String PIT_MENU_UP_MACRO_NAME = "pit menu up";
        private const String PIT_MENU_DOWN_MACRO_NAME = "pit menu down";
        private const String PIT_MENU_RIGHT_MACRO_NAME = "pit menu right";
        private const String PIT_MENU_LEFT_MACRO_NAME = "pit menu left";

        private const string folderConfirmChangeTyres = "mandatory_pit_stops/confirm_change_all_tyres";  // TODO record "confirm change tyres"
        private const string folderConfirmWetTyres = "mandatory_pit_stops/confirm_wet_tyres";
        private const string folderConfirmDryTyres = "mandatory_pit_stops/confirm_hard_tyres";  // TODO: record "confirm dry tyres"
        private const string folderConfirmNoTyres = "mandatory_pit_stops/confirm_change_no_tyres";
        private const string folderConfirmNoRefuelling = "mandatory_pit_stops/confirm_no_refuelling";

        public static void processVoiceCommand(string recognisedText, AudioPlayer audioPlayer)
        {
            bool recognised = false;
            if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_CHANGE_TYRES))
            {
                mashKeysToPutPitMenuInKnownState();
                recognised = true;
                audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmChangeTyres, 0));
            }
            if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_DRY_TYRES))
            {
                mashKeysToPutPitMenuInKnownState();
                recognised = true;
                audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmDryTyres, 0));
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_CLEAR_TYRES))
            {
                dontChangeTyres();
                recognised = true;
                audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmNoTyres, 0));
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_WET_TYRES))
            {
                selectWets();
                recognised = true;
                audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmWetTyres, 0));
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_DONT_REFUEL))
            {
                clearFuel();
                recognised = true;
                audioPlayer.playMessageImmediately(new QueuedMessage(folderConfirmNoRefuelling, 0));
            }
            if (!recognised)
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
            }
        }

        public static void mashKeysToPutPitMenuInKnownState()
        {
            if (CrewChief.currentGameState == null)
            {
                // meh
                return;
            }
            // keep track of this
            int currentSelectedTyreSet = CrewChief.currentGameState.TyreData.selectedSet;
            // mash keys until this changes
            bool gotMenuInKnownState = false;
            // go to a random non-pit menu, then to the pit menu to put the cursor at the top
            sendKeyPressOrMacro(getStandingsMenuMacro(), standingsMenuKey);
            sendKeyPressOrMacro(getPitMenuMacro(), pitMenuKey);

            // now go down 4 times and press right. If change tyres is selected this will change the selected tyre set
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
            // now wait a moment to ensure we have a new game update
            Thread.Sleep(CrewChief.timeInterval * 2);
            if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
            {
                gotMenuInKnownState = true;
                // yay, we know where we are - put the tyre set back to where it was
            }
            else
            {
                // 2 possibilities here. Either we don't have the change-tyres checkbox selected and we've just 
                // selected 'change brakes', or we do have change-tyres selected and we're on the tyre set option
                // but the game's ignoring us

                // assume the first, unselect change brakes
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
                sendKeyPressOrMacro(getMenuUpMacro(), upKey);
                sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                sendKeyPressOrMacro(getMenuDownMacro(), downKey);
                sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                // now wait a moment to ensure we have a new game update
                Thread.Sleep(CrewChief.timeInterval * 2);
                if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                {
                    gotMenuInKnownState = true;
                }
                else
                {
                    // ok, so now we have enabled tyre change but the cursor skips the tyre set because we have
                    // wets selected. Select drys and try to change the tyre set again
                    sendKeyPressOrMacro(getMenuUpMacro(), upKey);
                    sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
                    sendKeyPressOrMacro(getMenuDownMacro(), downKey);
                    sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                    Thread.Sleep(CrewChief.timeInterval * 2);
                    // now wait a moment to ensure we have a new game update
                    if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                    {
                        gotMenuInKnownState = true;
                        // ok, we now have a tyre set change. Don't reinstate wets - we want the menu in a known state
                    }
                }
            }
            if (gotMenuInKnownState)
            {
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
                // now exit and re-enter the pit menu to put the cursor back to the top
                sendKeyPressOrMacro(getStandingsMenuMacro(), standingsMenuKey);
                sendKeyPressOrMacro(getPitMenuMacro(), pitMenuKey);
            }
        }

        private static void sendKeyPressOrMacro(ExecutableCommandMacro macro, VirtualKeyCode fallbackKeyCode)
        {
            if (macro != null)
            {
                // suppress macro confirmation messages, and run the macro on the caller's thread:
                macro.execute(null, true, false);
            }
            else
            {
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, fallbackKeyCode));
            }
            Thread.Sleep(10);
        }

        public static void selectWets()
        {
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
        }

        public static void selectDrys()
        {
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getPitMenuMacro(), pitMenuKey);
        }

        public static void dontChangeTyres()
        {
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
        }

        public static void addFuel(int litres)
        {
            clearFuel();
            for (int i = 0; i < litres; i++)
            {
                sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
            }
        }

        public static void clearFuel()
        {
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
            for (int i = 0; i < 100; i++)
            {
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
            }
        }

        public static void increaseAllPressuresTo(float targetPressure)
        {
            // TODO: map MFD tyre pressures and work out the correct number of button presses
        }

        private static ExecutableCommandMacro getPitMenuMacro()
        {
            if (ACCPitMenuManager.pitMenuMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_MACRO_NAME, out ACCPitMenuManager.pitMenuMacro);
            }
            return ACCPitMenuManager.pitMenuMacro;
        }
        
        private static ExecutableCommandMacro getStandingsMenuMacro()
        {
            if (ACCPitMenuManager.standingsMenuMacro == null)
            {
                MacroManager.macros.TryGetValue(STANDINGS_MACRO_NAME, out ACCPitMenuManager.standingsMenuMacro);
            }
            return ACCPitMenuManager.standingsMenuMacro;
        }

        private static ExecutableCommandMacro getMenuUpMacro()
        {
            if (ACCPitMenuManager.menuUpMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_UP_MACRO_NAME, out ACCPitMenuManager.menuUpMacro);
            }
            return ACCPitMenuManager.menuUpMacro;
        }

        private static ExecutableCommandMacro getMenuDownMacro()
        {
            if (ACCPitMenuManager.menuDownMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_DOWN_MACRO_NAME, out ACCPitMenuManager.menuDownMacro);
            }
            return ACCPitMenuManager.menuDownMacro;
        }

        private static ExecutableCommandMacro getMenuRightMacro()
        {
            if (ACCPitMenuManager.menuRightMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_RIGHT_MACRO_NAME, out ACCPitMenuManager.menuRightMacro);
            }
            return ACCPitMenuManager.menuRightMacro;
        }

        private static ExecutableCommandMacro getMenuLeftMacro()
        {
            if (ACCPitMenuManager.menuLeftMacro == null)
            {
                MacroManager.macros.TryGetValue(PIT_MENU_LEFT_MACRO_NAME, out ACCPitMenuManager.menuLeftMacro);
            }
            return ACCPitMenuManager.menuLeftMacro;
        }
    }
}
