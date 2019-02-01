using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.Events
{
    class ButtonAssignment : AbstractEvent
    {
        private Boolean initialised = false;
        private Boolean keepQuietEnabled = false;
        public ButtonAssignment(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }
        public override void clearState()
        {
            initialised = false;
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            initialised = true;
        }
        public override void respond(String voiceMessage)
        {
            Console.WriteLine(voiceMessage);
        }

        public void enableKeepQuietMode()
        {
            keepQuietEnabled = true;
            audioPlayer.enableKeepQuietMode();
        }
        public void disableKeepQuietMode()
        {
            keepQuietEnabled = false;
            audioPlayer.disableKeepQuietMode();
        }
    }
}
