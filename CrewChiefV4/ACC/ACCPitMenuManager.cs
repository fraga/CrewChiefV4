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

        private static void mashKeysToPutPitMenuInKnownState()
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
            // go to the pit menu to put the cursor at the top
            moveCursorToTopOfPitMenu();

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
                // yay, we know where we are
            }
            else
            {
                // 2 possibilities here. Either we don't have the change-tyres checkbox selected and we've just 
                // selected 'change brakes', or we do have change-tyres selected and we're on the tyre set option
                // but the game's ignoring us

                // on the happy path, we either changed the brakes checkbox or the tyre type. A left press will either
                // reset the brake option or select dry tyres
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);

                // now go up - we'll either be on tyre set or change tyres
                sendKeyPressOrMacro(getMenuUpMacro(), upKey);
                sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                // now see if that change tyres
                Thread.Sleep(CrewChief.timeInterval * 2);
                if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                {
                    gotMenuInKnownState = true;
                    // yay, we know where we are
                }
                else
                {
                    // this last press must have enabled tyre change. We're on drys at this point so down-and-right must be what we want
                    sendKeyPressOrMacro(getMenuDownMacro(), downKey);
                    sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                    Thread.Sleep(CrewChief.timeInterval * 2);
                    if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                    {
                        gotMenuInKnownState = true;
                        // yay, we know where we are
                    }
                    else
                    {
                        // one more possibility, we're now on the tyre type option with wets selected - change to dry and try again
                        sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
                        sendKeyPressOrMacro(getMenuUpMacro(), upKey);
                        sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
                        Thread.Sleep(CrewChief.timeInterval * 2);
                        if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                        {
                            gotMenuInKnownState = true;
                            // yay, we know where we are
                        }
                    }
                }
            }
            if (gotMenuInKnownState)
            {
                // put the tyre set back to where it was
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
                // put the cursor back to the top
                moveCursorToTopOfPitMenu();
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
                // we didn't find a required key press macro so just press the most likely key anyway
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, fallbackKeyCode), 100);
            }
            Thread.Sleep(100);
        }

        private static void moveCursorToTopOfPitMenu()
        {
            sendKeyPressOrMacro(getPitMenuMacro(), pitMenuKey);
            // additional pause - sometimes this specific key is ignored or it takes a while to complete the action
            Thread.Sleep(100);
        }

        private static void selectWets()
        {
            if (CrewChief.currentGameState == null)
            {
                // meh
                return;
            }
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
        }

        private static void selectDrys()
        {
            if (CrewChief.currentGameState == null)
            {
                // meh
                return;
            }
            mashKeysToPutPitMenuInKnownState();
        }

        private static void dontChangeTyres()
        {
            if (CrewChief.currentGameState == null)
            {
                // meh
                return;
            }
            mashKeysToPutPitMenuInKnownState();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
        }

        private static void clearFuel()
        {
            if (CrewChief.currentGameState == null)
            {
                // meh
                return;
            }
            moveCursorToTopOfPitMenu();
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuDownMacro(), downKey);
            sendKeyPressOrMacro(getMenuRightMacro(), rightKey);
            for (int i = 0; i < 100; i++)
            {
                sendKeyPressOrMacro(getMenuLeftMacro(), leftKey);
            }
        }

        private static void increaseAllPressuresTo(float targetPressure)
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
