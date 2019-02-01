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

        }
        public override void respond(String voiceMessage)
        {

        }
    }
}
