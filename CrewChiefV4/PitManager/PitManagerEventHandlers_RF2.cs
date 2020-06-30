using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;

namespace CrewChiefV4.PitManager
{
    class PitManagerEventHandlers_RF2 : AbstractEvent
    {
        public override void clearState()
        {
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (CrewChief.gameDefinition.gameEnum != GameEnum.RF2_64BIT)
            {
                return;
            }
        }
        public PitManagerEventHandlers_RF2(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

            /// <summary>
            /// The event handlers for rF2
            /// </summary>
            static public bool actionHandler_example()
        {
            return true;

        }
        static public bool actionHandler_example2()
        {
            return true;
        }
        /// <summary>
        /// The response handlers for rF2
        /// </summary>
        static public bool responseHandler_example()
        {
            //audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
            return true;
        }
    }
}
