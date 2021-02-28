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
        // TODO: get these from macro key bindings if they're configured
        private static VirtualKeyCode upKey = VirtualKeyCode.UP;
        private static VirtualKeyCode downKey = VirtualKeyCode.DOWN;
        private static VirtualKeyCode leftKey = VirtualKeyCode.LEFT;
        private static VirtualKeyCode rightKey = VirtualKeyCode.RIGHT;
        private static VirtualKeyCode pitMenuKey = VirtualKeyCode.VK_P;
        private static VirtualKeyCode standingsMenuKey = VirtualKeyCode.VK_U;

        public void processVoiceCommand(string recognisedText, AudioPlayer audioPlayer)
        {
            if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_CHANGE_TYRES) || SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_DRY_TYRES))
            {
                mashKeysToPutPitMenuInKnownState();
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_CHANGE_TYRES))
            {
                dontChangeTyres();
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_WET_TYRES))
            {
                selectWets();
            }
            else if (SpeechRecogniser.ResultContains(recognisedText, SpeechRecogniser.PIT_STOP_DONT_REFUEL))
            {
                clearFuel();
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
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, standingsMenuKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, pitMenuKey));

            // now go down 4 times and press right. If change tyres is selected this will change the selected tyre set
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
            // now wait a moment to ensure we have a new game update
            Thread.Sleep(100);
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
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, leftKey));
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, upKey));
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
                if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                {
                    gotMenuInKnownState = true;
                }
                else
                {
                    // ok, so now we have enabled tyre change but the cursor skips the tyre set because we have
                    // wets selected. Select drys and try to change the tyre set again
                    KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, upKey));
                    KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, leftKey));
                    KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
                    KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
                    if (currentSelectedTyreSet != CrewChief.currentGameState.TyreData.selectedSet)
                    {
                        gotMenuInKnownState = true;
                        // ok, we now have a tyre set change. Don't reinstate wets - we want the menu in a known state
                    }
                }
            }
            if (gotMenuInKnownState)
            {
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, leftKey));
                // now exit and re-enter the pit menu to put the cursor back to the top
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, standingsMenuKey));
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, pitMenuKey));
            }
        }

        public static void selectWets()
        {
            mashKeysToPutPitMenuInKnownState();
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
        }

        public static void selectDrys()
        {
            mashKeysToPutPitMenuInKnownState();
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, pitMenuKey));
        }

        public static void dontChangeTyres()
        {
            mashKeysToPutPitMenuInKnownState();
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
        }

        public static void addFuel(int litres)
        {
            clearFuel();
            for (int i = 0; i < litres; i++)
            {
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
            }
        }

        public static void clearFuel()
        {
            mashKeysToPutPitMenuInKnownState();
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, downKey));
            KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, rightKey));
            for (int i = 0; i < 100; i++)
            {
                KeyPresser.SendKeyPress(new Tuple<VirtualKeyCode?, VirtualKeyCode>(null, leftKey));
            }
        }

        public static void increaseAllPressuresTo(float targetPressure)
        {
            // TODO: map MFD tyre pressures and work out the correct number of button presses
        }
    }
}
